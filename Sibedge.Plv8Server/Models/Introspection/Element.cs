namespace Sibedge.Plv8Server.Models.Introspection
{
    using System.Collections.Generic;

    /// <summary> Any graphQL introspection element </summary>
    public class Element
    {
        /// <summary> Input fields </summary>
        public IList<InputField> InputFields { get; set; }

        /// <summary> Name </summary>
        public string Name { get; set; }

        /// <summary> Description </summary>
        public string Description { get; set; }

        /// <summary> Interfaces </summary>
        public IList<Type> Interfaces { get; set; }

        /// <summary> Enum values </summary>
        public IList<EnumValue> EnumValues { get; set; }

        /// <summary> Fields </summary>
        public IList<Field> Fields { get; set; }

        /// <summary> Kind </summary>
        public string Kind { get; set; }

        /// <summary> Possible types </summary>
        public IList<Type> PossibleTypes { get; set; }
    }
}
