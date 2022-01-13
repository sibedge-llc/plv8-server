namespace Sibedge.Plv8Server
{
    using System.Collections.Generic;

    /// <summary> Plv8 settings </summary>
    public class Plv8Settings
    {
        /// <summary> Name of Id field </summary>
        public string IdField { get; set; }

        /// <summary> Id-Postfix in field names (foreign keys) </summary>
        public string IdPostfix { get; set; }

        /// <summary> Postfix in aggregate collection names </summary>
        public string AggPostfix { get; set; }

        /// <summary> Database schema </summary>
        public string Schema { get; set; }

        /// <summary> Default primary key field names list </summary>
        public IList<string> DefaultKeys { get; set; }

        /// <summary> Settings for write audit </summary>
        public AuditSettings Audit { get; set; }
    }
}
