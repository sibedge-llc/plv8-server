namespace Sibedge.Plv8Server.Models.Introspection
{
    using Newtonsoft.Json;

    /// <summary> Item with name </summary>
    public class NamedItem
    {
        /// <summary> Default ctor </summary>
        public NamedItem()
        {
        }

        /// <summary> Ctor </summary>
        public NamedItem(string name)
        {
            this.Name = name;
        }

        /// <summary> Name </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
