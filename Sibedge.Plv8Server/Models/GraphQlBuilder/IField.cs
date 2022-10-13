namespace Sibedge.Plv8Server.Models.GraphQlBuilder
{
    /// <summary> GraphQl query field </summary>
    public interface IField
    {
        /// <summary> Name </summary>
        string Name { get; set; }

        /// <summary> Converts value to string </summary>
        /// <param name="level"> Hierarchy level </param>
        string ToString(int level);
    }
}
