namespace Sibedge.Plv8Server.Models.Introspection
{
    using Newtonsoft.Json;

    /// <summary> Item with name </summary>
    public class NamedItem
    {
        /// <summary>Initializes a new instance of the <see cref="NamedItem"/> class. </summary>
        public NamedItem()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NamedItem"/> class. </summary>
        public NamedItem(string name)
        {
            this.Name = name;
        }

        /// <summary> Name </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
