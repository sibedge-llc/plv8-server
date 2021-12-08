namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary> Any graphQL introspection element </summary>
    public class Element
    {
        /// <summary> Input fields </summary>
        [JsonProperty("inputFields")]
        public IList<InputField> InputFields { get; set; }

        /// <summary> Name </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary> Description </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary> Interfaces </summary>
        [JsonProperty("interfaces")]
        public IList<Type> Interfaces { get; set; }

        /// <summary> Enum values </summary>
        [JsonProperty("enumValues")]
        public IList<EnumValue> EnumValues { get; set; }

        /// <summary> Fields </summary>
        [JsonProperty("fields")]
        public IList<Field> Fields { get; set; }

        /// <summary> Kind </summary>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary> Possible types </summary>
        [JsonProperty("possibleTypes")]
        public IList<Type> PossibleTypes { get; set; }
    }
}
