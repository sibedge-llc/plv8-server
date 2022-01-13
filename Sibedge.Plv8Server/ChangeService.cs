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
    using Models;

    /// <summary> Service for inserting / updating data </summary>
    public class ChangeService
    {
        private readonly IDbConnection connection;
        private readonly Plv8Settings settings;

        /// <summary> Initializes a new instance of the <see cref="ChangeService"/> class. </summary>
        public ChangeService(IDbConnection connection, IOptions<Plv8Settings> settings)
        {
            this.connection = connection;
            this.settings = settings.Value;
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

            var result = await this.connection.QueryFirstAsync<string>(
                sql,
                new
                {
                    tableName,
                    data,
                    idKeys = idKeys?.Any() == true ? idKeys : this.settings.DefaultKeys,
                    operation = operation.GetDescription(),
                    schema = this.settings.Schema,
                    user = authData.Serialize(),
                });

            if (this.settings.Audit?.Enabled == true)
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
                    var auditSql = $@"INSERT INTO ""{this.settings.Audit.Schema}"".""{this.settings.Audit.TableName}""
                        (table_name, operation, account_id, made, query, result) VALUES
                        (@tableName, @operation, @accountId, @made, @data::jsonb, @result::jsonb)";

                    await this.connection.ExecuteAsync(
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
    }
}
