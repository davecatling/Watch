using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WatchFunctions.Model;
using System.IO;
using Newtonsoft.Json;

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
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var message = JsonConvert.DeserializeObject<Dtos.MessageDto>(requestBody);
                var hasAccess = await Entities.HasAccess(message.ChannelNumber, user.RowKey);
                if (!hasAccess)
                    throw new InvalidOperationException("Access denied");
                string to = req.Query["to"];
                if (to != null && to != "ALL")
                {
                    hasAccess = await Entities.HasAccess(message.ChannelNumber, to);
                    if (!hasAccess)
                        throw new InvalidOperationException("Recipient invalid");
                }
                var messageEntity = new MessageEntity()
                {
                    PartitionKey = message.ChannelNumber,
                    RowKey = Guid.NewGuid().ToString(),
                    TextBytes = message.TextBytes,
                    To = message.To ?? "ALL",
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
