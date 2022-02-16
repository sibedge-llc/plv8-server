namespace Sibedge.Plv8Server.Models.Introspection
{
    /// <summary> GraphQL introspection item with name </summary>
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
        public string Name { get; set; }
    }
}
