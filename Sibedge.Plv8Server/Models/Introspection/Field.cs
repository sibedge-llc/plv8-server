namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary> GraphQL introspection field </summary>
    public class Field : FieldBase
    {
        /// <summary> Args </summary>
        public IList<InputField> Args { get; set; } = new List<InputField>();

        /// <summary> Name </summary>
        public bool IsDeprecated { get; set; }

        /// <summary> DeprecationReason </summary>
        [JsonPropertyName("deprecation reason")]
        public string DeprecationReason { get; set; }

        /// <summary> Raw type (for id fields) </summary>
        [JsonIgnore]
        public Type RawType { get; set; }
    }
}
