namespace Sibedge.Plv8Server.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    /// <summary> Helper for controllers </summary>
    public static class ControllerHelper
    {
        /// <summary> Returns JSON data </summary>
        public static async Task<IActionResult> GetFuncData(this Task<string> task, ControllerBase controller)
        {
            try
            {
                var json = await task;
                return controller.Content(json, "application/json");
            }
            catch (Exception e)
            {
                return controller.BadRequest(e.Message);
            }
        }

        /// <summary> Returns JSON data </summary>
        public static async ValueTask<IActionResult> GetFuncData(this ValueTask<string> task, ControllerBase controller)
        {
            try
            {
                var json = await task;
                return controller.Content(json, "application/json");
            }
            catch (Exception e)
            {
                return controller.BadRequest(e.Message);
            }
        }
    }
}
