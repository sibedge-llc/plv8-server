namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Field : FieldBase
    {
        /// <summary> Args </summary>
        [JsonProperty("args")]
        public List<InputField> Args { get; set; } = new List<InputField>();

        /// <summary> Name </summary>
        [JsonProperty("isDeprecated")]
        public bool IsDeprecated { get; set; }

        /// <summary> DeprecationReason </summary>
        [JsonProperty("deprecation reason")]
        public string DeprecationReason { get; set; }
    }
}
