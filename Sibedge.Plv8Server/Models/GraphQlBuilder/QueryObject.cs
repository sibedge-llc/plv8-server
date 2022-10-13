namespace Sibedge.Plv8Server.Models.GraphQlBuilder
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary> Object for build GraphQl query </summary>
    public class QueryObject
    {
        /// <summary> Child fields </summary>
        public IList<IField> Fields { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder("query {");
            sb.AppendLine();

            foreach (var field in this.Fields)
            {
                sb.Append(field.ToString(1));
            }

            sb.Append("}");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
