namespace Sibedge.Plv8Server
{
    using System.Data;

    /// <summary> Base class for CRUD services </summary>
    public class DataServiceBase
    {        
        /// <summary> Initializes a new instance of the <see cref="DataServiceBase"/> class. </summary>
        protected DataServiceBase(IDbConnection connection, Plv8Settings settings)
        {
            this.Connection = connection;
            this.Settings = settings;
        }

        /// <summary> Database connection </summary>
        protected IDbConnection Connection { get; set; }

        /// <summary> Settings </summary>
        protected Plv8Settings Settings { get; set; }
    }
}
