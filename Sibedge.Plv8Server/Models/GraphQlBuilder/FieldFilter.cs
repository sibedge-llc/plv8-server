namespace Sibedge.Plv8Server.Models.GraphQlBuilder
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary> Filter for fields </summary>
    public class FieldFilter
    {
        /// <summary> Equals </summary>
        [JsonPropertyName("equals")]
        public object EqualsOperator { get; set; }

        /// <summary> Equals </summary>
        [JsonPropertyName("notEquals")]
        public object NotEqualsOperator { get; set; }

        /// <summary> Less </summary>
        public object Less { get; set; }

        /// <summary> Greater </summary>
        public object Greater { get; set; }

        /// <summary> LessOrEquals </summary>
        public object LessOrEquals { get; set; }

        /// <summary> GreaterOrEquals </summary>
        public object GreaterOrEquals { get; set; }

        /// <summary> Contains </summary>
        public object Contains { get; set; }

        /// <summary> NotContains </summary>
        public object NotContains { get; set; }

        /// <summary> ArrayContains </summary>
        public object ArrayContains { get; set; }

        /// <summary> ArrayNotContains </summary>
        public object ArrayNotContains { get; set; }

        /// <summary> Starts </summary>
        public object Starts { get; set; }

        /// <summary> Ends </summary>
        public object Ends { get; set; }

        /// <summary> EqualsNoCase </summary>
        public object EqualsNoCase { get; set; }

        /// <summary> Jsquery </summary>
        public object Jsquery { get; set; }

        /// <summary> In </summary>
        public IList<object> In { get; set; }
    }
}
