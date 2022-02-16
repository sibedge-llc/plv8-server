namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Text.Json.Serialization;

    /// <summary> GraphQL introspection enum value </summary>
    public class EnumValue
    {
        /// <summary> Name </summary>
        public string Name { get; set; }

        /// <summary> Description </summary>
        public string Description { get; set; }

        /// <summary> Name </summary>
        public bool IsDeprecated { get; set; }

        /// <summary> DeprecationReason </summary>
        [JsonPropertyName("deprecation reason")]
        public string DeprecationReason { get; set; }
    }
}
