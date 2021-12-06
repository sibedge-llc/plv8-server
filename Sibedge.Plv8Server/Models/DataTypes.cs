namespace Sibedge.Plv8Server.Models
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary> GraphQL data types </summary>
    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
    public static class DataTypes
    {
        /// <summary> Text type </summary>
        public const string Text = "text";

        /// <summary> Integer type </summary>
        public const string Integer = "integer";

        /// <summary> Boolean type </summary>
        public const string Boolean = "boolean";
    }
}
