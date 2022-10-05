namespace Sibedge.Plv8Server
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Dapper;
    using Helpers;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Models;
    using Models.Introspection;
    using Type = Models.Introspection.Type;

    /// <summary> GraphQL service </summary>
    public class GraphQlService : DataServiceBase
    {
        private const string IntrospectionCacheKey = "__IntrospectionData";

        private static readonly string[] FilterOperatorsInt = { "equals", "notEquals", "less", "greater", "lessOrEquals", "greaterOrEquals" };
        private static readonly string[] FilterOperatorsText = { "contains", "notContains", "arrayContains", "arrayNotContains", "starts", "ends", "equalsNoCase", "jsquery" };
        private static readonly string[] FilterOperatorsBool = { "isNull" };
        private static readonly string[] FilterOperatorsArray = { "in" };
        private static readonly string[] FilterOperatorsObject = { "children" };

        private static readonly string[] NumericTypes = { "integer", "bigint", "real", "double_precision", "numeric" };
        private static readonly string[] DateTypes = { "timestamp", "date", "time" };
        private static readonly string[] DateAggFunctions = { "max", "min" };
        private static readonly string[] AggFunctions = new[] { "avg", "sum" }.Union(DateAggFunctions).ToArray();

        private readonly IMemoryCache memoryCache;

        /// <summary> Initializes a new instance of the <see cref="GraphQlService"/> class. </summary>
        public GraphQlService(IDbConnection connection, IOptions<Plv8Settings> settings, IMemoryCache memoryCache)
            : base(connection, settings.Value)
        {
            this.memoryCache = memoryCache;
        }

        /// <summary> Execute graphQL query </summary>
        /// <param name="query"> Query data </param>
        /// <param name="authData"> Authorization data </param>
        public async ValueTask<string> PerformQuery(GraphQlQuery query, AuthData authData)
        {
            string json;

            if (query.OperationName == "IntrospectionQuery")
            {
                if (this.memoryCache.TryGetValue(IntrospectionCacheKey, out string cachedJson))
                {
                    return cachedJson;
                }

                var schema = await this.GetIntrospectionData();

                var data = new
                {
                    data = new
                    {
                        __schema = schema,
                    },
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                json = JsonSerializer.Serialize(data, options);

                this.memoryCache.Set(IntrospectionCacheKey, json);
            }
            else
            {
                if (query.Variables?.Any() == true)
                {
                    query.Query = $"query {query.Query.Substring(query.Query.IndexOf('{', StringComparison.InvariantCulture) - 1)}";

                    foreach (var variable in query.Variables)
                    {
                        var value = this.GetStringValue(variable.Value);

                        query.Query = query.Query
                            .Replace($"${variable.Key}", value, StringComparison.InvariantCultureIgnoreCase);
                    }
                }

                string authJson = authData != null ? $"'{authData.Serialize()}'" : "NULL";

                var sql = $"SELECT graphql.execute('{query.Query}', '{this.Settings.Schema}', {authJson});";
                json = await this.Connection.QueryFirstAsync<string>(sql);
            }

            return json;
        }

        private string GetStringValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var enumerator = element.EnumerateObject().GetEnumerator();
                var objectItems = enumerator.Select(x => $"{x.Name}: {this.GetStringValue(x.Value)}").ToList();

                var ret = $"{{{string.Join(',', objectItems)}}}";
                return ret;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                var enumerator = element.EnumerateArray().GetEnumerator();
                var arrayItems = enumerator.Select(this.GetStringValue).ToList();

                var ret = $"[{string.Join(',', arrayItems)}]";
                return ret;
            }

            return element.ValueKind switch
            {
                JsonValueKind.Null => "null",
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.String => $"\"{element}\"",
                _ => element.ToString(),
            };
        }

        private async ValueTask<IntrospectionSchema> GetIntrospectionData()
        {
            var result = new IntrospectionSchema
            {
                Directives = new List<object>(),
                MutationType = new NamedItem("Mutation"),
                SubscriptionType = new NamedItem("Subscription"),
                QueryType = new NamedItem("Query"),
                Types = await this.GetTypes(),
            };

            return result;
        }

        private async ValueTask<List<Element>> GetTypes()
        {
            var ret = new List<Element>();
            var fieldInfoList = (await this.GetFieldInfo()).ToList();
            var foreignKeyInfoList = (await this.GetForeignKeyInfo()).ToList();

            ret.Add(this.CreateNode(fieldInfoList));
            ret.Add(this.CreateQuery(fieldInfoList));
            ret.AddRange(this.CreateTables(fieldInfoList, foreignKeyInfoList));
            ret.AddRange(this.CreateFilters(fieldInfoList, foreignKeyInfoList));
            ret.AddRange(this.CreateAggregates(fieldInfoList, foreignKeyInfoList));

            // Data types
            ret.Add(new Element
            {
                Name = "Id",
                Description = "The `Id` scalar type represents a unique identifier.",
                Kind = Kinds.Scalar,
            });

            foreach (var dataType in fieldInfoList.Select(x => x.DataType)
                .Union(new[] { DataTypes.Integer })
                .Union(new[] { DataTypes.Text })
                .Union(new[] { DataTypes.Boolean })
                .Distinct())
            {
                ret.Add(new Element
                {
                    Name = dataType.ToTypeName(),
                    Description = $"The '{dataType}' scalar type.",
                    Kind = Kinds.Scalar,
                });
            }

            // Mutation, subscription
            ret.Add(new Element
            {
                Name = "Mutation",
                Interfaces = new List<Type>(),
                Fields = new List<Field>(),
                Kind = Kinds.Object,
            });

            ret.Add(new Element
            {
                Name = "Subscription",
                Interfaces = new List<Type>(),
                Fields = new List<Field>(),
                Kind = Kinds.Object,
            });

            return ret;
        }

        private Task<IEnumerable<ForeignKeyInfo>> GetForeignKeyInfo()
        {
            var sql = @"SELECT table_name AS ""TableName"", column_name AS ""ColumnName"",
                          foreign_table_name AS ""ForeignTableName"", foreign_column_name AS ""ForeignColumnName""
                        FROM graphql.schema_foreign_keys";

            return this.Connection.QueryAsync<ForeignKeyInfo>(sql);
        }

        private Element CreateNode(List<FieldInfo> fieldInfoList)
        {
            var ret = new Element
            {
                Name = "Node",
                Description = "An object with an ID",
                Fields = new List<Field>
                {
                    new Field
                    {
                        Name = "id",
                        Description = "The id of the object.",
                        Type = Type.CreateNonNull(Kinds.Scalar, "Id"),
                    },
                },
                Kind = Kinds.Interface,
                PossibleTypes = new List<Type>(),
            };

            foreach (var tableName in fieldInfoList.Select(x => x.TableName).Distinct())
            {
                ret.PossibleTypes.Add(new Type(Kinds.Object, tableName));
            }

            return ret;
        }

        private Element CreateQuery(List<FieldInfo> fieldInfoList)
        {
            var ret = new Element
            {
                Name = "Query",
                Interfaces = new List<Type>(),
                Kind = Kinds.Object,
                Fields = new List<Field>(),
            };

            foreach (var tableName in fieldInfoList.Select(x => x.TableName).Distinct())
            {
                ret.Fields.Add(new Field
                {
                    Name = tableName,
                    Type = Type.CreateNonNullList(Kinds.Object, tableName),
                    Args = new List<InputField>
                    {
                        new InputField
                        {
                            Name = "id",
                            Type = new Type(Kinds.Scalar, DataTypes.Integer),
                        },
                        new InputField
                        {
                            Name = "filter",
                            Type = new Type(Kinds.InputObject, $"{tableName}Filter"),
                        },
                        new InputField
                        {
                            Name = "orderBy",
                            Type = new Type(Kinds.Enum, $"{tableName}OrderBy"),
                        },
                        new InputField
                        {
                            Name = "orderByDescending",
                            Type = new Type(Kinds.Enum, $"{tableName}OrderByDescending"),
                        },
                        new InputField
                        {
                            Name = "skip",
                            Type = new Type(Kinds.Scalar, DataTypes.Integer),
                        },
                        new InputField
                        {
                            Name = "take",
                            Type = new Type(Kinds.Scalar, DataTypes.Integer),
                        },
                    },
                });

                ret.Fields.Add(new Field
                {
                    Name = tableName + this.Settings.AggPostfix,
                    Type = new Type(Kinds.InputObject, tableName + this.Settings.AggPostfix),
                    Args = new List<InputField>
                    {
                        new InputField
                        {
                            Name = "filter",
                            Type = new Type(Kinds.InputObject, $"{tableName}Filter"),
                        },
                        new InputField
                        {
                            Name = "groupBy",
                            Type = new Type(Kinds.Enum, $"{tableName}OrderBy"),
                        },
                        new InputField
                        {
                            Name = "aggFilter",
                            Type = new Type(Kinds.InputObject, $"{tableName}AggFilter"),
                        },
                    },
                });
            }

            return ret;
        }

        private List<Element> CreateTables(List<FieldInfo> fieldInfoList, List<ForeignKeyInfo> foreignKeyList)
        {
            var ret = new List<Element>();

            var tables = fieldInfoList.GroupBy(x => x.TableName);

            foreach (var table in tables)
            {
                var element = new Element
                {
                    Name = table.Key,
                    Description = table.Key,
                    Interfaces = new List<Type> { new Type(Kinds.Interface, "Node") },
                    Kind = Kinds.Object,
                    Fields = new List<Field>(),
                };

                foreach (var column in table)
                {
                    bool isIdColumn = column.ColumnName.ToLowerInvariant() == this.Settings.IdField.ToLowerInvariant();

                    var dataTypeName = isIdColumn
                        ? "Id"
                        : column.DataType.ToTypeName();

                    var field = new Field
                    {
                        Name = column.ColumnName,
                        Type = column.IsNullable
                            ? new Type(Kinds.Scalar, dataTypeName)
                            : Type.CreateNonNull(Kinds.Scalar, dataTypeName),
                    };

                    if (isIdColumn)
                    {
                        field.RawType = Type.CreateNonNull(Kinds.Scalar, column.DataType.ToTypeName());
                    }

                    element.Fields.Add(field);
                }

                var singleLinks = foreignKeyList.Where(x => x.TableName == table.Key);
                foreach (var singleLink in singleLinks)
                {
                    element.Fields.Add(new Field
                    {
                        Name = singleLink.ColumnName.EndsWith(this.Settings.IdPostfix)
                            ? singleLink.ColumnName.Substring(0, singleLink.ColumnName.Length - this.Settings.IdPostfix.Length)
                            : singleLink.ColumnName,
                        Type = new Type(Kinds.Object, singleLink.ForeignTableName),
                        Args = new List<InputField>
                        {
                            new InputField
                            {
                                Name = "filter",
                                Type = new Type(Kinds.InputObject, $"{singleLink.ForeignTableName}Filter"),
                            },
                        },
                    });
                }

                var multipleLinks = foreignKeyList.Where(x => x.ForeignTableName == table.Key);
                foreach (var multipleLink in multipleLinks)
                {
                    element.Fields.Add(new Field
                    {
                        Name = multipleLink.TableName,
                        Type = Type.CreateList(Kinds.Object, multipleLink.TableName),
                        Args = new List<InputField>
                        {
                            new InputField
                            {
                                Name = "filter",
                                Type = new Type(Kinds.InputObject, $"{multipleLink.TableName}Filter"),
                            },
                            new InputField
                            {
                                Name = "orderBy",
                                Type = new Type(Kinds.Enum, $"{multipleLink.TableName}OrderBy"),
                            },
                            new InputField
                            {
                                Name = "orderByDescending",
                                Type = new Type(Kinds.Enum, $"{multipleLink.TableName}OrderByDescending"),
                            },
                        },
                    });

                    element.Fields.Add(new Field
                    {
                        Name = multipleLink.TableName + this.Settings.AggPostfix,
                        Type = new Type(Kinds.InputObject, $"{multipleLink.TableName}{this.Settings.AggPostfix}Nested"),
                        Args = new List<InputField>
                        {
                            new InputField
                            {
                                Name = "filter",
                                Type = new Type(Kinds.InputObject, $"{multipleLink.TableName}Filter"),
                            },
                        },
                    });
                }

                ret.Add(element);
            }

            return ret;
        }

        private List<Element> CreateAggregates(List<FieldInfo> fieldInfoList, List<ForeignKeyInfo> foreignKeyList)
        {
            var ret = new List<Element>();

            Expression<Func<string, string>> selectExpr = x => x;

            if (this.Settings.AggPostfix[0] == '_')
            {
                selectExpr = x => x + "_";
            }

            var dateAggFunctions = DateAggFunctions.AsQueryable().Select(selectExpr).ToArray();
            var aggFunctions = AggFunctions.AsQueryable().Select(selectExpr).ToArray();

            var distinctStart = "distinct" + ((this.Settings.AggPostfix[0] == '_') ? "_" : string.Empty);

            var tables = fieldInfoList.GroupBy(x => x.TableName);

            foreach (var table in tables)
            {
                var countField = new Field
                {
                    Name = "count",
                    Type = new Type(Kinds.Scalar, DataTypes.Integer),
                };

                var elementRoot = new Element
                {
                    Name = table.Key + this.Settings.AggPostfix,
                    Description = "Aggregate function for " + table.Key,
                    Interfaces = new List<Type> { new Type(Kinds.Interface, "Node") },
                    Kind = Kinds.Object,
                };

                var element = new Element
                {
                    Name = elementRoot.Name + "Nested",
                    Description = elementRoot.Description,
                    Interfaces = elementRoot.Interfaces,
                    Kind = elementRoot.Kind,
                    Fields = new List<Field> { countField },
                };

                foreach (var column in table)
                {
                    if (column.ColumnName.ToLowerInvariant() == this.Settings.IdField.ToLowerInvariant())
                    {
                        continue;
                    }

                    var dataTypeName = column.DataType.ToTypeName();

                    element.Fields.Add(new Field
                    {
                        Name = distinctStart + column.ColumnName,
                        Type = Type.CreateList(Kinds.Object, dataTypeName),
                    });

                    if (!column.ColumnName.EndsWith(this.Settings.IdPostfix))
                    {
                        var aggFunctionsList = Array.Empty<string>();

                        if (NumericTypes.Contains(dataTypeName))
                        {
                            aggFunctionsList = aggFunctions;
                        }
                        else if (DateTypes.Any(x => dataTypeName.StartsWith(x)))
                        {
                            aggFunctionsList = dateAggFunctions;
                        }

                        foreach (var aggFunction in aggFunctionsList)
                        {
                            element.Fields.Add(new Field
                            {
                                Name = aggFunction + column.ColumnName,
                                Type = new Type(Kinds.Scalar, DataTypes.Integer),
                            });
                        }
                    }
                }

                var singleLinks = foreignKeyList.Where(x => x.TableName == table.Key);
                foreach (var singleLink in singleLinks)
                {
                    element.Fields.Add(new Field
                    {
                        Name = singleLink.ColumnName.EndsWith(this.Settings.IdPostfix)
                            ? singleLink.ColumnName.Substring(0, singleLink.ColumnName.Length - this.Settings.IdPostfix.Length)
                            : singleLink.ColumnName,
                        Type = new Type(Kinds.Object, singleLink.ForeignTableName),
                        Args = new List<InputField>
                        {
                            new InputField
                            {
                                Name = "filter",
                                Type = new Type(Kinds.InputObject, $"{singleLink.ForeignTableName}Filter"),
                            },
                        },
                    });
                }

                elementRoot.Fields = element.Fields.ToList();
                elementRoot.Fields.Add(new Field
                {
                    Name = "key",
                    Type = new Type(Kinds.Scalar, "Id"),
                });

                ret.Add(element);
                ret.Add(elementRoot);
            }

            return ret;
        }

        private List<Element> CreateFilters(List<FieldInfo> fieldInfoList, List<ForeignKeyInfo> foreignKeyList)
        {
            var ret = new List<Element>
            {
                new Element
                {
                    Name = "FreeFieldsFilter",
                    Kind = Kinds.InputObject,
                    InputFields = new List<InputField>(),
                },
                new Element
                {
                    Name = "OperatorFilter",
                    Kind = Kinds.InputObject,
                    InputFields = FilterOperatorsText
                        .Select(x => new InputField
                        {
                            Name = x,
                            Description = $"'{x}' operator.",
                            Type = new Type(Kinds.Scalar, DataTypes.Text),
                        })
                        .Union(FilterOperatorsInt.Select(x => new InputField
                        {
                            Name = x,
                            Description = $"'{x}' operator.",
                            Type = new Type(Kinds.Scalar, DataTypes.Integer),
                        }))
                        .Union(FilterOperatorsBool.Select(x => new InputField
                        {
                            Name = x,
                            Description = $"'{x}' operator.",
                            Type = new Type(Kinds.Scalar, DataTypes.Boolean),
                        }))
                        .Union(FilterOperatorsArray.Select(x => new InputField
                        {
                            Name = x,
                            Description = $"'{x}' operator.",
                            Type = Type.CreateList(Kinds.Scalar, DataTypes.Integer),
                        }))
                        .Union(FilterOperatorsObject.Select(x => new InputField
                        {
                            Name = x,
                            Description = $"'{x}' operator.",
                            Type = new Type(Kinds.InputObject, "FreeFieldsFilter"),
                        }))
                        .ToList(),
                },
            };

            Expression<Func<string, string>> selectExpr = x => x;

            if (this.Settings.AggPostfix[0] == '_')
            {
                selectExpr = x => x + "_";
            }

            var dateAggFunctions = DateAggFunctions.AsQueryable().Select(selectExpr).ToArray();
            var aggFunctions = AggFunctions.AsQueryable().Select(selectExpr).ToArray();

            var tables = fieldInfoList.GroupBy(x => x.TableName);

            foreach (var table in tables)
            {
                var filerInputFields = table.Select(
                    x => new InputField
                    {
                        Name = x.ColumnName,
                        Description = x.ColumnName,
                        Type = new Type(Kinds.Object, "OperatorFilter"),
                    }).ToList();

                var singleLinks = foreignKeyList.Where(x => x.TableName == table.Key);
                foreach (var singleLink in singleLinks)
                {
                    var relationField = new InputField
                    {
                        Name = singleLink.ColumnName.EndsWith(this.Settings.IdPostfix)
                            ? singleLink.ColumnName.Substring(0, singleLink.ColumnName.Length - this.Settings.IdPostfix.Length)
                            : singleLink.ColumnName,
                        Type = new Type(Kinds.Scalar, DataTypes.Boolean),
                    };

                    relationField.Description = $"{relationField.Name} relation existing";

                    filerInputFields.Add(relationField);
                }

                var multipleLinks = foreignKeyList.Where(x => x.ForeignTableName == table.Key);
                foreach (var multipleLink in multipleLinks)
                {
                    var relationField = new InputField
                    {
                        Name = multipleLink.TableName,
                        Type = new Type(Kinds.Scalar, DataTypes.Boolean),
                    };

                    relationField.Description = $"{relationField.Name} reverse relation existing";

                    filerInputFields.Add(relationField);
                }

                var orField = new InputField
                {
                    Name = "or",
                    Type = Type.CreateList(Kinds.InputObject, $"{table.Key}Filter"),
                };

                filerInputFields.Add(orField);

                ret.Add(new Element
                {
                    Name = $"{table.Key}Filter",
                    Kind = Kinds.InputObject,
                    InputFields = filerInputFields,
                });

                ret.Add(new Element
                {
                    Name = $"{table.Key}OrderBy",
                    Kind = Kinds.Enum,
                    EnumValues = table.Select(x => new EnumValue()
                    {
                        Name = x.ColumnName,
                    }).ToList(),
                });

                ret.Add(new Element
                {
                    Name = $"{table.Key}OrderByDescending",
                    Kind = Kinds.Enum,
                    EnumValues = table.Select(x => new EnumValue()
                    {
                        Name = x.ColumnName,
                    }).ToList(),
                });

                var aggFilerInputFields = new List<InputField>
                {
                    new InputField
                    {
                        Name = "count",
                        Description = "count",
                        Type = new Type(Kinds.Object, "OperatorFilter"),
                    },
                };

                foreach (var column in table
                    .Where(x => !x.ColumnName.EndsWith(this.Settings.IdPostfix)
                                && x.ColumnName != this.Settings.IdField))
                {
                    var dataTypeName = column.DataType.ToTypeName();
                    var aggFunctionsList = Array.Empty<string>();

                    if (NumericTypes.Contains(dataTypeName))
                    {
                        aggFunctionsList = aggFunctions;
                    }
                    else if (DateTypes.Any(x => dataTypeName.StartsWith(x)))
                    {
                        aggFunctionsList = dateAggFunctions;
                    }

                    foreach (var aggFunction in aggFunctionsList)
                    {
                        aggFilerInputFields.Add(
                            new InputField
                            {
                                Name = aggFunction + column.ColumnName,
                                Description = aggFunction + column.ColumnName,
                                Type = new Type(Kinds.Object, "OperatorFilter"),
                            });
                    }
                }

                ret.Add(new Element
                {
                    Name = $"{table.Key}AggFilter",
                    Kind = Kinds.InputObject,
                    InputFields = aggFilerInputFields,
                });
            }

            return ret;
        }
    }
}
