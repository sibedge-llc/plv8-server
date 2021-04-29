namespace Sibedge.Plv8Server.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public static class ControllerHelper
    {
        public static async ValueTask<IActionResult> GetFuncData(this Task<string> task, ControllerBase controller)
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
