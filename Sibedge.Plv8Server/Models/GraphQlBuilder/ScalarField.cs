namespace Sibedge.Plv8Server.Models.GraphQlBuilder
{
    using System.Text;
    using Helpers;

    /// <summary> GraphQl query scalar field </summary>
    public class ScalarField : IField
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string ToString(int level)
        {
            var sb = new StringBuilder();
            sb.AppendWhitespace(level);
            sb.Append(this.Name);

            return sb.ToString();
        }
    }
}
