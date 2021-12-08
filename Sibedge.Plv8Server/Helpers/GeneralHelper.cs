namespace Sibedge.Plv8Server.Helpers
{
    using System.ComponentModel;
    using System.Reflection;
    using System.Text.Json;
    using Models;

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

        /// <summary> Serializes authorization data </summary>
        /// <param name="authData"> Authorization data </param>
        /// <returns> JSON </returns>
        public static string Serialize(this AuthData authData)
        {
            if (authData != null)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                return $"'{JsonSerializer.Serialize(authData, options)}'";
            }
            else
            {
                return null;
            }
        }
    }
}
