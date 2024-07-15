namespace Sibedge.Plv8Server
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Models;

    /// <summary> Service for inserting / updating data </summary>
    public class ChangeService : DataServiceBase
    {
        private const string OpenApiSchemaCacheKey = "__OpenApiSchema";
        private readonly IMemoryCache memoryCache;

        /// <summary> Initializes a new instance of the <see cref="ChangeService"/> class. </summary>
        public ChangeService(IDbConnection connection, IOptions<Plv8Settings> settings, IMemoryCache memoryCache)
            : base(connection, settings.Value)
        {
            this.memoryCache = memoryCache;
        }

        /// <summary> Insert data into table </summary>
        /// <param name="tableName"> Table name </param>
        /// <param name="data"> Data to insert </param>
        /// <param name="idKeys"> Primary key fields </param>
        /// <param name="operation"> Data change operation </param>
        /// <param name="authData"> Authorization data </param>
        public Task<string> Change(
            string tableName,
            string data,
            IList<string> idKeys,
            ChangeOperation operation,
            AuthData authData)
        {
            return this.Change(tableName, data, idKeys, operation, authData, CancellationToken.None);
        }

        /// <summary> Insert data into table </summary>
        /// <param name="tableName"> Table name </param>
        /// <param name="data"> Data to insert </param>
        /// <param name="idKeys"> Primary key fields </param>
        /// <param name="operation"> Data change operation </param>
        /// <param name="authData"> Authorization data </param>
        /// <param name="cancellationToken"> Propagates notification that operations should be canceled </param>
        public async Task<string> Change(
            string tableName,
            string data,
            IList<string> idKeys,
            ChangeOperation operation,
            AuthData authData,
            CancellationToken cancellationToken)
        {
            var sql =
                "SELECT * FROM plv8.sql_change(@tableName, @data::jsonb, @idKeys, @operation, @schema, @user::jsonb);";

            var parameters = new Dictionary<string, object>
            {
                { nameof(tableName), tableName },
                { nameof(data), data },
                { "idKeys", idKeys?.Any() == true ? idKeys : this.Settings.DefaultKeys },
                { "operation", operation.GetDescription() },
                { "schema", this.Settings.Schema },
                { "user", authData.Serialize().AsSqlParameter() },
            };

            var result = await this.Connection.ReadJson(sql, parameters, cancellationToken);

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

                    var auditParameters = new Dictionary<string, object>
                    {
                        { nameof(tableName), tableName },
                        { nameof(operation), operation.GetDescription() },
                        { "accountId", authData?.UserId ?? 0 },
                        { "made", DateTime.UtcNow },
                        { nameof(data), data },
                        { nameof(result), result },
                    };

                    await this.Connection.RunCommand(auditSql, auditParameters, cancellationToken);
                }
            }

            return result;
        }

        /// <summary> Returns Open API schema JSON for change methods </summary>
        /// <param name="baseUrl"> Base URL for change endpoints </param>
        public ValueTask<string> GetSchema(string baseUrl)
        {
            return GetSchema(baseUrl, null, CancellationToken.None);
        }
        
        /// <summary> Returns Open API schema JSON for change methods </summary>
        /// <param name="baseUrl"> Base URL for change endpoints </param>
        /// <param name="cancellationToken"> Propagates notification that operations should be canceled </param>
        public ValueTask<string> GetSchema(string baseUrl, CancellationToken cancellationToken)
        {
            return GetSchema(baseUrl, null, cancellationToken);
        }

        /// <summary> Returns Open API schema JSON for change methods </summary>
        /// <param name="baseUrl"> Base URL for change endpoints </param>
        /// <param name="filterTables"> Optionally allowed to set tables (other will be ignored) </param>
        public ValueTask<string> GetSchema(string baseUrl, IList<string> filterTables)
        {
            return GetSchema(baseUrl, filterTables, CancellationToken.None);
        }

        /// <summary> Returns Open API schema JSON for change methods </summary>
        /// <param name="baseUrl"> Base URL for change endpoints </param>
        /// <param name="filterTables"> Optionally allowed to set tables (other will be ignored) </param>
        /// <param name="cancellationToken"> Propagates notification that operations should be canceled </param>
        public async ValueTask<string> GetSchema(string baseUrl, IList<string> filterTables, CancellationToken cancellationToken)
        {
            var key = OpenApiSchemaCacheKey;

            if (filterTables?.Any() == true)
            {
                key += "_" + string.Join("_", filterTables);
            }

            if (this.memoryCache.TryGetValue(key, out string cachedJson))
            {
                return cachedJson;
            }

            var sql = "SELECT * FROM plv8.openapi(@baseUrl, @schemaName, @filterTables::jsonb);";

            var parameters = new Dictionary<string, object>
            {
                { nameof(baseUrl), baseUrl },
                { "schemaName", this.Settings.Schema },
                { "filterTables", (filterTables?.Serialize()).AsSqlParameter() },
            };

            var json = await this.Connection.ReadJson(sql, parameters, cancellationToken);

            this.memoryCache.Set(key, json);

            return json;
        }
    }
}
