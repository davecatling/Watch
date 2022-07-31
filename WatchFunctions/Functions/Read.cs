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
            log.LogInformation("C# HTTP trigger function processed a request.");

            var authHeader = req.Headers["Authorization"][0];
            if (!authHeader.StartsWith("Bearer "))
                return new BadRequestObjectResult("Session not found");
            var user = await Entities.GetUserBySessionAsync(authHeader.Substring(7));
            if (user == null)
                return new BadRequestObjectResult("Bad session token");
            string channelNumber = req.Query["channelNumber"];
            var messages = await Entities.ReadMessages(channelNumber);
            //var result = messages.ToList().Select(message => $"{message.Sender}: {message.Text} - {message.Timestamp}");
            return new OkObjectResult(messages);
        }
    }
}
