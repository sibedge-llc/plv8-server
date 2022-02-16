namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Collections.Generic;

    /// <summary> GraphQL introspection schema </summary>
    public class IntrospectionSchema
    {
        /// <summary> Directives </summary>
        public IList<object> Directives { get; set; }

        /// <summary> Mutation type </summary>
        public NamedItem MutationType { get; set; }

        /// <summary> Subscription type </summary>
        public NamedItem SubscriptionType { get; set; }

        /// <summary> Query type </summary>
        public NamedItem QueryType { get; set; }

        /// <summary> Types </summary>
        public IList<Element> Types { get; set; }
    }
}
