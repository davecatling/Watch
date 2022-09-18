using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
                    throw new ArgumentException("Invalid session");
                var user = await Entities.GetUserBySessionAsync(authHeader[7..]);
                if (user == null)
                    throw new ArgumentException("Invalid session");
                string channelNumber = req.Query["channelNumber"];
                var hasAccess = await Entities.HasAccess(channelNumber, user.RowKey);
                if (!hasAccess)
                    throw new InvalidOperationException("Access denied");
                string message = req.Query["message"];
                string to = req.Query["to"];
                var messageEntity = new MessageEntity()
                {
                    PartitionKey = channelNumber,
                    RowKey = Guid.NewGuid().ToString(),
                    Text = message,
                    To = to,
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
