namespace Sibedge.Plv8Server.Models
{
    /// <summary> Information about DB foreign keys </summary>
    public class ForeignKeyInfo
    {
        /// <summary> Table name </summary>
        public string TableName { get; set; }

        /// <summary> Column name </summary>
        public string ColumnName { get; set; }

        /// <summary> Foreign table name </summary>
        public string ForeignTableName { get; set; }

        /// <summary> Foreign column name </summary>
        public string ForeignColumnName { get; set; }

        /// <summary> Is Array </summary>
        public bool IsArray { get; set; }
    }
}
