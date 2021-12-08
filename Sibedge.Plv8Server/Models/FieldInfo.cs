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
    }
}
