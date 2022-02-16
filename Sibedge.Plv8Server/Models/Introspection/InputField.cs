namespace Sibedge.Plv8Server.Models.Introspection
{
    /// <summary> GraphQL introspection input field </summary>
    public class InputField : FieldBase
    {
        /// <summary> Default value </summary>
        public object DefaultValue { get; set; }
    }
}
