namespace Sibedge.Plv8Server
{
    using System.Data;
    using System.Globalization;
    using System.Linq;
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
                    "SELECT graphql.introspection('{0}', '{1}', '{2}', '{3}');",
                    this.Settings.Schema,
                    this.Settings.IdField,
                    this.Settings.IdPostfix,
                    this.Settings.AggPostfix);

                json = await this.Connection.QueryFirstAsync<string>(sql);

                this.memoryCache.Set(IntrospectionCacheKey, json);
            }
            else
            {
                string authJson = authData != null ? $"'{authData.Serialize()}'" : "NULL";
                string variablesJson = query.Variables?.Any() == true ? $"'{query.Variables.Serialize()}'" : "NULL";

                var sql = $"SELECT graphql.execute('{query.Query}', '{this.Settings.Schema}', {authJson}, {variablesJson});";
                json = await this.Connection.QueryFirstAsync<string>(sql);
            }

            return json;
        }
    }
}
