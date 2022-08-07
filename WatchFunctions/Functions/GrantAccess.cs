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
    public static class GrantAccess
    {
        [FunctionName("GrantAccess")]
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
                    throw new ArgumentException("Bad session token");
                string channelNumber = req.Query["channelNumber"];
                string handle = req.Query["handle"];
                if (user.RowKey == handle)
                    throw new InvalidOperationException("Invalid grant request");
                var hasAccess = await Entities.HasAccess(channelNumber, user.RowKey);
                if (!hasAccess)
                    throw new InvalidOperationException("Access denied");
                var newAccess = new AccessEntity()
                {
                    PartitionKey = channelNumber,
                    RowKey = Guid.NewGuid().ToString(),
                    Handle = handle,
                    Grantor = user.RowKey
                };
                _ = await Entities.SaveEntityAsync("access", newAccess);
                var newMessage = new MessageEntity()
                {
                    PartitionKey = channelNumber,
                    RowKey = Guid.NewGuid().ToString(),
                    Sender = "SYSTEM",
                    Text = $"{user.RowKey} granted access to {handle}"
                };
                _ = await Entities.SaveEntityAsync("messages", newMessage);
                return new OkObjectResult("OK");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Granting access failed: {ex.Message}");
            }
        }
    }
}
