namespace Sibedge.Plv8Server
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using Helpers;
    using Microsoft.Extensions.Options;
    using Models;

    /// <summary> Service for inserting / updating data </summary>
    public class ChangeService
    {
        private readonly IDbConnection connection;
        private readonly Settings settings;

        /// <summary> Initializes a new instance of the <see cref="ChangeService"/> class. </summary>
        public ChangeService(IDbConnection connection, IOptions<Settings> settings)
        {
            this.connection = connection;
            this.settings = settings.Value;
        }

        /// <summary> Insert data into table </summary>
        /// <param name="tableName"> Table name </param>
        /// <param name="data"> Data to insert </param>
        /// <param name="idKeys"> Primary key fields </param>
        /// <param name="operation"> Data change operation </param>
        public Task<string> Change(string tableName, string data, IList<string> idKeys, ChangeOperation operation)
        {
            var sql = "SELECT * FROM plv8.sql_change(@tableName, @data::jsonb, @idKeys, @operation, @schema);";

            return this.connection.QueryFirstAsync<string>(
                sql,
                new
                {
                    tableName,
                    data,
                    idKeys = idKeys?.Any() == true ? idKeys : this.settings.DefaultKeys,
                    operation = operation.GetDescription(),
                    schema = this.settings.Schema,
                });
        }
    }
}
