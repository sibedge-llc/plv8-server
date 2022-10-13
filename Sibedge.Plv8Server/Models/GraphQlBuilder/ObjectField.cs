namespace Sibedge.Plv8Server.Models.GraphQlBuilder
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Helpers;

    /// <summary> GraphQl query object field </summary>
    public class ObjectField : IField
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <summary> Child fields </summary>
        public IList<IField> Fields { get; set; }

        /// <summary> Filter </summary>
        public IDictionary<string, FieldFilter> Filter { get; set; }

        /// <summary> Skip items number </summary>
        public long? Skip { get; set; }

        /// <summary> Take items size </summary>
        public long? Take { get; set; }

        /// <summary> Order by field </summary>
        public string OrderBy { get; set; }

        /// <inheritdoc />
        public string ToString(int level)
        {
            var sb = new StringBuilder();
            sb.AppendWhitespace(level);
            sb.Append(this.Name);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            bool hasArguments = this.Skip.HasValue || this.Take.HasValue || this.Filter?.Any() == true
                || !string.IsNullOrWhiteSpace(this.OrderBy);
            if (hasArguments)
            {
                bool hasSomething = false;
                sb.Append(" (");

                if (this.Filter?.Any() == true)
                {
                    sb.Append("filter: ");
                    var filterJson = JsonSerializer.Serialize(this.Filter, options);
                    var filterElement = JsonSerializer.Deserialize<JsonElement>(filterJson);
                    sb.Append(GraphQlService.GetArgumentStringValue(filterElement));
                    hasSomething = true;
                }

                if (!string.IsNullOrWhiteSpace(this.OrderBy))
                {
                    sb.Append(hasSomething ? ", " : string.Empty);
                    sb.Append("orderBy: ").Append(this.OrderBy);
                    hasSomething = true;
                }

                if (this.Skip.HasValue)
                {
                    sb.Append(hasSomething ? ", " : string.Empty);
                    sb.Append("skip: ").Append(this.Skip);
                    hasSomething = true;
                }

                if (this.Take.HasValue)
                {
                    sb.Append(hasSomething ? ", " : string.Empty);
                    sb.Append("take: ").Append(this.Take);
                }

                sb.Append(" )");
            }

            sb.Append(" {");
            sb.AppendLine();

            foreach (var field in this.Fields)
            {
                sb.Append(field.ToString(level + 1));
                sb.AppendLine();
            }

            sb.AppendWhitespace(level);
            sb.Append("}");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
