namespace Sibedge.Plv8Server
{
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using Models;

    /// <summary> Base service for executing stored functions (not only plv8) </summary>
    public abstract class V8FuncServiceBase
    {
        private IDbConnection _connection;

        /// <summary> ctor </summary>
        public V8FuncServiceBase(IDbConnection connection)
        {
            _connection = connection;
        }

        public Task<string> ExecuteFunction(string funcName, params object[] args)
        {
            var argNames = string.Join(',',
                args.Select((x, i) =>
                    x is FunctionArgument funcArgValue
                        ? $"@arg{i}::{funcArgValue.SqlType}"
                        : $"@arg{i}"));

            var sql = $"SELECT * FROM {funcName}({argNames});";

            var argsDict = args.Select((val, i) => new {val, i})
                .ToDictionary(x => $"@arg{x.i}",
                    x => x.val is FunctionArgument funcArgValue ? funcArgValue.Value : x.val);

            return _connection.QueryFirstAsync<string>(sql, new DynamicParameters(argsDict));
        }
    }
}
