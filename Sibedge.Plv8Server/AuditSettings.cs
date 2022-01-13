namespace Sibedge.Plv8Server
{
    /// <summary> Settings for write audit </summary>
    public class AuditSettings
    {
        /// <summary> Is write audit enabled </summary>
        public bool Enabled { get; set; }

        /// <summary> Database schema </summary>
        public string Schema { get; set; }

        /// <summary> Database table name </summary>
        public string TableName { get; set; }
    }
}
