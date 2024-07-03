using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;

namespace Sibedge.Plv8Server.Helpers
{
    /// <summary> SQL queries helper </summary>
    internal static class SqlHelper
    {
        /// <summary> Reads JSON value from PostgreSQL </summary>
        public static async Task<string> ReadJson(this IDbConnection connection, string queryString, IDictionary<string, object> parameters)
        {
            var npqsqlConnection = connection as NpgsqlConnection;
            var command = new NpgsqlCommand(queryString, npqsqlConnection);
            
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
                }
            }            
            
            connection.Open();
            var reader = await command.ExecuteReaderAsync();
            try
            {
                if (await reader.ReadAsync())
                {
                    var result = reader[0] as string;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
