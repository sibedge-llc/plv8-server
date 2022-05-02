namespace Sibedge.Plv8Server
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper;
    using Models;

    /// <summary> Base class for CRUD services </summary>
    public class DataServiceBase
    {
        /// <summary> Initializes a new instance of the <see cref="DataServiceBase"/> class. </summary>
        protected DataServiceBase(IDbConnection connection, Plv8Settings settings)
        {
            this.Connection = connection;
            this.Settings = settings;
        }

        /// <summary> Database connection </summary>
        protected IDbConnection Connection { get; set; }

        /// <summary> Settings </summary>
        protected Plv8Settings Settings { get; set; }

        /// <summary> Returns information about database tables fields </summary>
        protected Task<IEnumerable<FieldInfo>> GetFieldInfo()
        {
            var sql = $@"SELECT gc.table_name AS ""TableName"", gc.column_name AS ""ColumnName"",
                          ic.data_type AS ""DataType"", ic.is_nullable='YES' AS ""IsNullable"" FROM graphql.schema_columns gc
                        LEFT JOIN information_schema.columns ic ON gc.table_name=ic.table_name AND gc.column_name=ic.column_name
                          WHERE ic.table_schema::name = '{this.Settings.Schema}'::name;";

            return this.Connection.QueryAsync<FieldInfo>(sql);
        }
    }
}
