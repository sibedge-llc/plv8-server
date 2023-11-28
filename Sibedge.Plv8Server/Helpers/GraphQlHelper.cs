namespace Sibedge.Plv8Server.Helpers
{
    using System.Linq;
    using System.Text.Json;

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

        /// <summary> Get string value for GraphQl variable </summary>
        /// <param name="element"> Variable element </param>
        /// <returns> String value for direct insertion into query </returns>
        public static string GetArgumentStringValue(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var enumerator = element.EnumerateObject().GetEnumerator();
                var objectItems = enumerator.Select(x => $"{x.Name}: {x.Value.GetArgumentStringValue()}").ToList();

                var ret = $"{{{string.Join(',', objectItems)}}}";
                return ret;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                var enumerator = element.EnumerateArray().GetEnumerator();
                var arrayItems = enumerator.Select(GetArgumentStringValue).ToList();

                var ret = $"[{string.Join(',', arrayItems)}]";
                return ret;
            }

            return element.ValueKind switch
            {
                JsonValueKind.Null => "null",
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.String => $"\"{element}\"",
                _ => element.ToString(),
            };
        }
    }
}
