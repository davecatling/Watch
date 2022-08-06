using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WatchFunctions.Model;

namespace WatchFunctions.Functions
{
    public static class Write
    {
        [FunctionName("Write")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var authHeader = req.Headers["Authorization"][0];
                if (!authHeader.StartsWith("Bearer "))
                    return new BadRequestObjectResult("Session not found");
                var user = await Entities.GetUserBySessionAsync(authHeader[7..]);
                if (user == null)
                    return new BadRequestObjectResult("Bad session token");
                string channelNumber = req.Query["channelNumber"];
                string message = req.Query["message"];
                var messageEntity = new MessageEntity()
                {
                    PartitionKey = channelNumber,
                    RowKey = Guid.NewGuid().ToString(),
                    Text = message,
                    Sender = user.RowKey
                };
                _ = await Entities.SaveEntityAsync("messages", messageEntity);
                return new OkObjectResult("OK");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Writing message failed: {ex.Message}");
            }
        }
    }
}
