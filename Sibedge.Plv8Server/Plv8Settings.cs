namespace Sibedge.Plv8Server
{
    using System.Collections.Generic;

    public class Plv8Settings
    {
        public string IdField { get; set; }

        public string IdPostfix { get; set; }

        public string AggPostfix { get; set; }

        public string Schema { get; set; }

        public IList<string> DefaultKeys { get; set; }
    }
}
