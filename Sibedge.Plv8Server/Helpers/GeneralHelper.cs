namespace Sibedge.Plv8Server.Helpers
{
    using System.ComponentModel;
    using System.Reflection;

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
    }
}
