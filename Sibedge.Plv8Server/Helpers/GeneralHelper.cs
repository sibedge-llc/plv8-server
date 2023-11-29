namespace Sibedge.Plv8Server.Helpers
{
    using System.ComponentModel;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary> General helper </summary>
    public static class GeneralHelper
    {
        /// <summary> Returns Description attribute value </summary>
        /// <typeparam name="T"> Type </typeparam>
        /// <param name="source"> Field </param>
        /// <returns> Attribute value </returns>
        public static string GetDescription<T>(this T source)
        {
            var fi = source.GetType().GetField(source.ToString());

            var attributes = fi.GetCustomAttribute<DescriptionAttribute>(false);

            return attributes?.Description ?? source.ToString();
        }

        /// <summary> Serializes any data </summary>
        /// <param name="data"> Data to serialize </param>
        /// <returns> JSON </returns>
        public static string Serialize(this object data)
        {
            if (data != null)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                };

                return JsonSerializer.Serialize(data, options);
            }
            else
            {
                return null;
            }
        }

        /// <summary> Appends whitespace </summary>
        /// <param name="sb"> StringBuilder </param>
        /// <param name="level"> Hierarchy level </param>
        public static void AppendWhitespace(this StringBuilder sb, int level)
        {
            sb.Append(' ', level * 2);
        }
    }
}
