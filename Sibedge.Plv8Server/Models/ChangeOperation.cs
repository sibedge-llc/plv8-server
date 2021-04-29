namespace Sibedge.Plv8Server.Models
{
    using System.ComponentModel;

    /// <summary> Change data operations </summary>
    public enum ChangeOperation
    {
        [Description("")]
        Insert,

        [Description("update")]
        Update,

        [Description("delete")]
        Delete
    }
}
