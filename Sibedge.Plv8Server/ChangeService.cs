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
        private readonly IDbConnection _connection;
        private readonly Settings _settings;

        /// <summary> ctor </summary>
        public ChangeService(IDbConnection connection, IOptions<Settings> settings)
        {
            _connection = connection;
            _settings = settings.Value;
        }

        /// <summary> Insert data into table </summary>
        /// <param name="tableName"> Table name </param>
        /// <param name="data"> Data to insert </param>
        /// <param name="idKeys"> Primary key fields </param>
        /// <param name="operation"> Data change operation </param>
        public Task<string> Change(string tableName, string data, IList<string> idKeys, ChangeOperation operation)
        {
            var sql = "SELECT * FROM plv8.sql_change(@tableName, @data::jsonb, @idKeys, @operation, @schema);";

            return _connection.QueryFirstAsync<string>(sql,
                new
                {
                    tableName,
                    data,
                    idKeys = idKeys?.Any() == true ? idKeys : _settings.DefaultKeys,
                    operation = operation.GetDescription(),
                    schema = _settings.Schema
                });
        }
    }
}
