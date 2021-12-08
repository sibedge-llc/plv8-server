namespace Sibedge.Plv8Server.Models.Introspection
{
    using Newtonsoft.Json;

    /// <summary> Type </summary>
    public class Type
    {
        /// <summary> Default ctor </summary>
        public Type()
        {
        }

        /// <summary> Ctor </summary>
        public Type(string kind, string name, Type type = null)
        {
            this.Kind = kind;
            this.Name = name;
            this.OfType = type;
        }

        public static Type CreateNonNull(string kind, string name)
        {
            return new Type(Kinds.NonNull, null, new Type(kind, name, null));
        }

        public static Type CreateList(string kind, string name)
        {
            return new Type(Kinds.List, null, CreateNonNull(kind, name));
        }

        public static Type CreateNonNullList(string kind, string name)
        {
            return new Type(Kinds.NonNull, null, CreateList(kind, name));
        }

        /// <summary> Kind </summary>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary> Name </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary> Of type </summary>
        [JsonProperty("ofType")]
        public Type OfType { get; set; }
    }
}
