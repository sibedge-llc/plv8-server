namespace Sibedge.Plv8Server.Models
{
    using System.Collections.Generic;
    using GraphQlBuilder;

    /// <summary> Authorization data </summary>
    public class AuthData
    {
        /// <summary> User id for registered user </summary>
        public int? UserId { get; set; }

        /// <summary> Anonymous user </summary>
        public bool IsAnonymous => !this.UserId.HasValue;

        /// <summary> User Id </summary>
        public Dictionary<string, FieldFilter> Condition { get; set; }
    }
}
