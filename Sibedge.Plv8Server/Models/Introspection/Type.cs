namespace Sibedge.Plv8Server.Models.Introspection
{
    /// <summary> GraphQL introspection type </summary>
    public class Type
    {
        /// <summary> Initializes a new instance of the <see cref="Type"/> class. </summary>
        public Type()
        {
        }

        /// <summary> Initializes a new instance of the <see cref="Type"/> class. </summary>
        /// <param name="kind"> Type kind </param>
        /// <param name="name"> Type name </param>
        /// <param name="type"> Parent type </param>
        public Type(string kind, string name, Type type = null)
        {
            this.Kind = kind;
            this.Name = name;
            this.OfType = type;
        }

        /// <summary> Type kind </summary>
        public string Kind { get; set; }

        /// <summary> Type name </summary>
        public string Name { get; set; }

        /// <summary> Parent type </summary>
        public Type OfType { get; set; }

        /// <summary> Creates NotNull type </summary>
        public static Type CreateNonNull(string kind, string name)
        {
            return new Type(Kinds.NonNull, null, new Type(kind, name, null));
        }

        /// <summary> Creates list type </summary>
        public static Type CreateList(string kind, string name)
        {
            return new Type(Kinds.List, null, CreateNonNull(kind, name));
        }

        /// <summary> Creates NotNull list type </summary>
        public static Type CreateNonNullList(string kind, string name)
        {
            return new Type(Kinds.NonNull, null, CreateList(kind, name));
        }
    }
}
