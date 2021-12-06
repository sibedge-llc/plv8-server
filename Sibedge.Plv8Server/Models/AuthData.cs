namespace Sibedge.Plv8Server.Models
{
    /// <summary> Authorization data </summary>
    public class AuthData
    {
        /// <summary> User id for registered user </summary>
        public int? UserId { get; set; }

        /// <summary> Anonymous user </summary>
        public bool IsAnonymous => !this.UserId.HasValue;
    }
}
