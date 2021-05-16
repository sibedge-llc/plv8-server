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
        private readonly IDbConnection connection;

        /// <summary>Initializes a new instance of the <see cref="V8FuncServiceBase"/> class. </summary>
        protected V8FuncServiceBase(IDbConnection connection)
        {
            this.connection = connection;
        }

        public Task<string> ExecuteFunction(string funcName, params object[] args)
        {
            var argNames = string.Join(
                ',',
                args.Select((x, i) =>
                    x is FunctionArgument funcArgValue
                        ? $"@arg{i}::{funcArgValue.SqlType}"
                        : $"@arg{i}"));

            var sql = $"SELECT * FROM {funcName}({argNames});";

            var argsDict = args.Select((val, i) => new { val, i })
                .ToDictionary(
                    x => $"@arg{x.i}",
                    x => x.val is FunctionArgument funcArgValue ? funcArgValue.Value : x.val);

            return this.connection.QueryFirstAsync<string>(sql, new DynamicParameters(argsDict));
        }
    }
}
