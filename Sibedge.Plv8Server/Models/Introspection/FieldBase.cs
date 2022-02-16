namespace Sibedge.Plv8Server.Models.Introspection
{
    /// <summary> Base class for GraphQL introspection field </summary>
    public abstract class FieldBase
    {
        /// <summary> Name </summary>
        public string Name { get; set; }

        /// <summary> Description </summary>
        public string Description { get; set; }

        /// <summary> Type </summary>
        public Type Type { get; set; }
    }
}
