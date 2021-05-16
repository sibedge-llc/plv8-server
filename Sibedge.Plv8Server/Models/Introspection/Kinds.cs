namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary> GraphQL introspection kinds </summary>
    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
    public static class Kinds
    {
        /// <summary> Interface kind </summary>
        public const string Interface = "INTERFACE";

        /// <summary> InputObject kind </summary>
        public const string InputObject = "INPUT_OBJECT";

        /// <summary> Enum kind </summary>
        public const string Enum = "ENUM";

        /// <summary> Scalar kind </summary>
        public const string Scalar = "SCALAR";

        /// <summary> Object kind </summary>
        public const string Object = "OBJECT";

        /// <summary> List kind </summary>
        public const string List = "LIST";

        /// <summary> NonNull kind </summary>
        public const string NonNull = "NON_NULL";

        /// <summary> Union kind </summary>
        public const string Union = "UNION";
    }
}
