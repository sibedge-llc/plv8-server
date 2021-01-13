namespace Sibedge.Plv8Server
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper;

    /// <summary> Service for inserting / updating data </summary>
    public class InsertService
    {
        private IDbConnection _connection;

        /// <summary> ctor </summary>
        public InsertService(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary> Insert data into table </summary>
        /// <param name="tableName"> Table name </param>
        /// <param name="data"> Data to insert </param>
        /// <param name="idKeys"> Primary key fields </param>
        /// <param name="upsert"> Use "ON CONFLICT UPDATE" </param>
        public Task<string> Insert(string tableName, string data, IList<string> idKeys, bool upsert = false)
        {
            var sql = "SELECT * FROM plv8.sql_change(@tableName, @data::jsonb, @idKeys, @upsert);";

            return _connection.QueryFirstAsync<string>(sql,
                new
                {
                    tableName,
                    data,
                    idKeys,
                    upsert
                });
        }
    }
}
