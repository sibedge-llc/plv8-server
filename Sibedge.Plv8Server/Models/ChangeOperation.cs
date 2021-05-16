namespace Sibedge.Plv8Server.Models
{
    using System.ComponentModel;

    /// <summary> Change data operations </summary>
    public enum ChangeOperation
    {
        /// <summary> Insert operation </summary>
        [Description("")]
        Insert,

        /// <summary> Update operation </summary>
        [Description("update")]
        Update,

        /// <summary> Delete operation </summary>
        [Description("delete")]
        Delete,
    }
}
