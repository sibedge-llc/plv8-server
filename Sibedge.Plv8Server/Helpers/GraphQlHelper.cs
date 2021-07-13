namespace Sibedge.Plv8Server.Helpers
{
    public static class GraphQlHelper
    {
        public static string ToTypeName(this string str)
        {
            return str
                .Replace(' ', '_')
                .Replace('-', '_');
        }
    }
}
