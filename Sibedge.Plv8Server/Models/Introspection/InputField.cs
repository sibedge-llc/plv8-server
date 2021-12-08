namespace Sibedge.Plv8Server.Models.Introspection
{
    using Newtonsoft.Json;

    /// <summary> GraphQL introspection input field </summary>
    public class InputField : FieldBase
    {
        /// <summary> Default value </summary>
        [JsonProperty("defaultValue")]
        public object DefaultValue { get; set; }
    }
}
