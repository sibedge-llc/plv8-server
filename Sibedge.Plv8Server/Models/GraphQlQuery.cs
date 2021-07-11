namespace Sibedge.Plv8Server.Models
{
    using System.Collections.Generic;
    using System.Text.Json;

    /// <summary> GraphQL query </summary>
    public class GraphQlQuery
    {
        /// <summary> Operation name </summary>
        public string OperationName { get; set; }

        /// <summary> Operation name </summary>
        public Dictionary<string, JsonElement> Variables { get; set; }

        /// <summary> Query body </summary>
        public string Query { get; set; }
    }
}
