namespace Sibedge.Plv8Server.Models
{
    /// <summary> Database field information </summary>
    public class FieldInfo
    {
        /// <summary> Table name </summary>
        public string TableName { get; set; }

        /// <summary> Column name </summary>
        public string ColumnName { get; set; }

        /// <summary> Data type </summary>
        public string DataType { get; set; }

        /// <summary> Is nullable </summary>
        public bool IsNullable { get; set; }

        /// <summary> Is always auto-generated (read only) </summary>
        public bool IsGenerated { get; set; }

        /// <summary> Is primary key </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary> Has default value </summary>
        public bool HasDefaultValue { get; set; }

        /// <summary> Is table function </summary>
        public bool IsFunction { get; set; }
    }
}
