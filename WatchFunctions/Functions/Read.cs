using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace WatchFunctions.Functions
{
    public static class Read
    {
        [FunctionName("Read")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var authHeader = req.Headers["Authorization"][0];
            if (!authHeader.StartsWith("Bearer "))
                return new BadRequestObjectResult("Session not found");
            var user = await Entities.GetUserBySessionAsync(authHeader[7..]);
            if (user == null)
                return new BadRequestObjectResult("Bad session token");
            string channelNumber = req.Query["channelNumber"];
            var hasAccess = await Entities.HasAccess(channelNumber, user.RowKey);
            if (!hasAccess)
                return new BadRequestObjectResult("Access denied");
            var messages = await Entities.ReadMessages(channelNumber);
            return new OkObjectResult(messages);
        }
    }
}
