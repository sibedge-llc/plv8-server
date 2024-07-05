namespace Sibedge.Plv8Server
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Models;

    /// <summary> GraphQL service </summary>
    public class GraphQlService : DataServiceBase
    {
        private const string IntrospectionCacheKey = "__GraphQlIntrospectionData";

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

            if (query.IsIntrospectionQuery())
            {
                if (this.memoryCache.TryGetValue(IntrospectionCacheKey, out string cachedJson))
                {
                    return cachedJson;
                }

                var sql = "SELECT graphql.introspection(@schema, @idField, @idPostfix, @aggPostfix);";

                var parameters = new Dictionary<string, object>
                {
                    { "schema", this.Settings.Schema },
                    { "idField", this.Settings.IdField },
                    { "idPostfix", this.Settings.IdPostfix },
                    { "aggPostfix", this.Settings.AggPostfix },
                };

                json = await this.Connection.ReadJson(sql, parameters);

                this.memoryCache.Set(IntrospectionCacheKey, json);
            }
            else
            {
                var sql = "SELECT graphql.execute(@query, @schema, @user::jsonb, @variables::jsonb);";

                var parameters = new Dictionary<string, object>
                {
                    { "query", query.Query },
                    { "schema", this.Settings.Schema },
                    { "user", authData.Serialize().AsSqlParameter() },
                    { "variables", query.Variables.Serialize().AsSqlParameter() },
                };

                json = await this.Connection.ReadJson(sql, parameters);
            }

            return json;
        }
    }
}
