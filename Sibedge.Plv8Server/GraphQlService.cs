namespace Sibedge.Plv8Server
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Dapper;
    using Helpers;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Models;

    /// <summary> GraphQL service </summary>
    public class GraphQlService : DataServiceBase
    {
        private const string IntrospectionCacheKey = "__IntrospectionData";

        private readonly IMemoryCache memoryCache;

        /// <summary> Initializes a new instance of the <see cref="GraphQlService"/> class. </summary>
        public GraphQlService(IDbConnection connection, IOptions<Plv8Settings> settings, IMemoryCache memoryCache)
            : base(connection, settings.Value)
        {
            this.memoryCache = memoryCache;
        }

        /// <summary> Get string value for GraphQl variable </summary>
        /// <param name="element"> Variable element </param>
        /// <returns> String value for direct insertion into query </returns>
        public static string GetArgumentStringValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var enumerator = element.EnumerateObject().GetEnumerator();
                var objectItems = enumerator.Select(x => $"{x.Name}: {GetArgumentStringValue(x.Value)}").ToList();

                var ret = $"{{{string.Join(',', objectItems)}}}";
                return ret;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                var enumerator = element.EnumerateArray().GetEnumerator();
                var arrayItems = enumerator.Select(GetArgumentStringValue).ToList();

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

                var sql = string.Format(
                    CultureInfo.InvariantCulture,
                    "SELECT plv8.introspection('{0}', '{1}', '{2}', '{3}');",
                    this.Settings.Schema,
                    this.Settings.IdField,
                    this.Settings.IdPostfix,
                    this.Settings.AggPostfix);

                json = await this.Connection.QueryFirstAsync<string>(sql);

                this.memoryCache.Set(IntrospectionCacheKey, json);
            }
            else
            {
                if (query.Variables?.Any() == true)
                {
                    query.Query = $"query {query.Query.Substring(query.Query.IndexOf('{', StringComparison.InvariantCulture) - 1)}";

                    foreach (var variable in query.Variables)
                    {
                        var value = GetArgumentStringValue(variable.Value);

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
    }
}
