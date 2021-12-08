namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary> Introspection schema </summary>
    public class IntrospectionSchema
    {
        /// <summary> Directives </summary>
        [JsonProperty("directives")]
        public List<object> Directives { get; set; }

        /// <summary> Mutation type </summary>
        [JsonProperty("mutationType")]
        public NamedItem MutationType { get; set; }

        /// <summary> Subscription type </summary>
        [JsonProperty("subscriptionType")]
        public NamedItem SubscriptionType { get; set; }

        /// <summary> Query type </summary>
        [JsonProperty("queryType")]
        public NamedItem QueryType { get; set; }

        /// <summary> Types </summary>
        [JsonProperty("types")]
        public List<Element> Types { get; set; }
    }
}
