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
        internal static async Task<string> ReadJson(this IDbConnection connection, string querySql,
            IDictionary<string, object> parameters)
        {
            var npqsqlConnection = connection as NpgsqlConnection;
            await npqsqlConnection.OpenIfNeeded();

            using var command = new NpgsqlCommand(querySql, npqsqlConnection);
            command.AddParameters(parameters);

            using var reader = await command.ExecuteReaderAsync();
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

        /// <summary> Reads JSON value from PostgreSQL </summary>
        internal static async Task RunCommand(this IDbConnection connection, string commandSql,
            IDictionary<string, object> parameters)
        {
            var npqsqlConnection = connection as NpgsqlConnection;
            await npqsqlConnection.OpenIfNeeded();

            using var command = new NpgsqlCommand(commandSql, npqsqlConnection);
            command.AddParameters(parameters);

            await command.ExecuteNonQueryAsync();
        }

        private static async ValueTask OpenIfNeeded(this NpgsqlConnection connection)
        {
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                await connection.OpenAsync();
            }
        }

        private static void AddParameters(this NpgsqlCommand command, IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
                }
            }
        }
    }
}
