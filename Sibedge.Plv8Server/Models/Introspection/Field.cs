namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary> GraphQL introspection field </summary>
    public class Field : FieldBase
    {
        /// <summary> Args </summary>
        [JsonProperty("args")]
        public IList<InputField> Args { get; set; } = new List<InputField>();

        /// <summary> Name </summary>
        [JsonProperty("isDeprecated")]
        public bool IsDeprecated { get; set; }

        /// <summary> DeprecationReason </summary>
        [JsonProperty("deprecation reason")]
        public string DeprecationReason { get; set; }

        /// <summary> Raw type (for id fields) </summary>
        [JsonIgnore]
        public Type RawType { get; set; }
    }
}
