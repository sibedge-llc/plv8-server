namespace Sibedge.Plv8Server
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Dapper;
    using Helpers;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi;
    using Microsoft.OpenApi.Extensions;
    using Microsoft.OpenApi.Models;
    using Models;

    /// <summary> Service for inserting / updating data </summary>
    public class ChangeService : DataServiceBase
    {
        private const string ObjectType = "object";

        private static readonly string[] ContentTypes = { "application/json", "text/json", "application/*+json" };

        /// <summary> Initializes a new instance of the <see cref="ChangeService"/> class. </summary>
        public ChangeService(IDbConnection connection, IOptions<Plv8Settings> settings)
            : base(connection, settings.Value)
        {
        }

        /// <summary> Insert data into table </summary>
        /// <param name="tableName"> Table name </param>
        /// <param name="data"> Data to insert </param>
        /// <param name="idKeys"> Primary key fields </param>
        /// <param name="operation"> Data change operation </param>
        /// <param name="authData"> Authorization data </param>
        public async Task<string> Change(
            string tableName,
            string data,
            IList<string> idKeys,
            ChangeOperation operation,
            AuthData authData)
        {
            var sql = "SELECT * FROM plv8.sql_change(@tableName, @data::jsonb, @idKeys, @operation, @schema, @user::jsonb);";

            var result = await this.Connection.QueryFirstAsync<string>(
                sql,
                new
                {
                    tableName,
                    data,
                    idKeys = idKeys?.Any() == true ? idKeys : this.Settings.DefaultKeys,
                    operation = operation.GetDescription(),
                    schema = this.Settings.Schema,
                    user = authData.Serialize(),
                });

            if (this.Settings.Audit?.Enabled == true)
            {
                bool isChanged = false;

                if (operation == ChangeOperation.Delete)
                {
                    var resultData = JsonSerializer.Deserialize<List<int>>(result);
                    isChanged = resultData?.FirstOrDefault() > 0;
                }
                else
                {
                    var resultData = JsonSerializer.Deserialize<List<JsonElement>>(result);
                    if (resultData.Any())
                    {
                        var value = resultData.First();
                        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i))
                        {
                            isChanged = i > 0;
                        }
                        else
                        {
                            isChanged = true;
                        }
                    }
                }

                if (isChanged)
                {
                    var auditSql = $@"INSERT INTO ""{this.Settings.Audit.Schema}"".""{this.Settings.Audit.TableName}""
                        (table_name, operation, account_id, made, query, result) VALUES
                        (@tableName, @operation, @accountId, @made, @data::jsonb, @result::jsonb)";

                    await this.Connection.ExecuteAsync(
                        auditSql,
                        new
                        {
                            tableName,
                            operation = operation.GetDescription(),
                            accountId = authData?.UserId ?? 0,
                            made = DateTime.UtcNow,
                            data,
                            result,
                        });
                }
            }

            return result;
        }

        /// <summary> Returns Open API schema JSON for change methods </summary>
        /// <param name="baseUrl"> Base URL for change endpoints </param>
        public async Task<string> GetSchema(string baseUrl)
        {
            var fieldInfoList = (await this.GetFieldInfo()).ToList();

            var schemas = this.GenerateSchemas(fieldInfoList);

            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "Change data endpoints",
                },
                Paths = this.GeneratePaths(fieldInfoList, baseUrl),
                Components = new OpenApiComponents
                {
                    Schemas = schemas,
                },
            };

            return document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
        }

        private IDictionary<string, OpenApiSchema> GenerateSchemas(IList<FieldInfo> fieldInfoList)
        {
            var tables = fieldInfoList.GroupBy(x => x.TableName);
            var ret = new Dictionary<string, OpenApiSchema>();

            foreach (var table in tables)
            {
                var schema = new OpenApiSchema
                {
                    Type = ObjectType,
                    Properties = table.ToDictionary(c => c.ColumnName, c => new OpenApiSchema
                    {
                        Type = c.DataType.ToTypeName(),
                        Nullable = c.IsNullable,
                    }),
                };

                ret.Add(table.Key, schema);
            }

            return ret;
        }

        private OpenApiPaths GeneratePaths(IList<FieldInfo> fieldInfoList, /*IDictionary<string, OpenApiSchema> schemas,*/ string baseUrl)
        {
            var tables = fieldInfoList.GroupBy(x => x.TableName);
            var ret = new OpenApiPaths();

            foreach (var table in tables)
            {
                ret.Add(
                    $"{baseUrl}/{table.Key}",
                    new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Post] = new OpenApiOperation
                            {
                                Tags = new List<OpenApiTag> { new OpenApiTag { Name = table.Key } },
                                Summary = $"Create {table.Key}",
                                RequestBody = new OpenApiRequestBody
                                {
                                    Content = ContentTypes
                                        .ToDictionary(
                                            contentType => contentType,
                                            contentType => new OpenApiMediaType
                                            {
                                                Schema = new OpenApiSchema
                                                {
                                                    Reference = new OpenApiReference
                                                    {
                                                        ExternalResource = $"#/components/schemas/{table.Key}",
                                                    },
                                                }, ////schemas[table.Key],
                                            }),
                                },
                            },
                        },
                    });
            }

            return ret;
        }
    }
}
