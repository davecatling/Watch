using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WatchFunctions.Functions
{
    public static class Read
    {
        [FunctionName("Read")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var authHeader = req.Headers["Authorization"][0];
                if (!authHeader.StartsWith("Bearer "))
                    throw new ArgumentException("Invalid session");
                var user = await Entities.GetUserBySessionAsync(authHeader[7..]);
                if (user == null)
                    throw new ArgumentException("Invalid session");
                string channelNumber = req.Query["channelNumber"];
                var hasAccess = await Entities.HasAccess(channelNumber, user.RowKey);
                if (!hasAccess)
                    throw new InvalidOperationException("Access denied");
                var messages = await Entities.ReadMessages(channelNumber);
                return new OkObjectResult(messages);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Read failed: {ex.Message}");
            }
        }
    }
}
