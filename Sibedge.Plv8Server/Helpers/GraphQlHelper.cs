namespace Sibedge.Plv8Server.Helpers
{
    /// <summary> Helper for GraphQL </summary>
    public static class GraphQlHelper
    {
        /// <summary> Converts database type name to GraphQL type name </summary>
        /// <param name="dbName"> Database type name </param>
        /// <returns> GraphQL type name </returns>
        public static string ToTypeName(this string dbName)
        {
            return dbName
                .Replace(' ', '_')
                .Replace('-', '_');
        }
    }
}
