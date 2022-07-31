using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            if (!req.Headers.ContainsKey("SessionToken"))
                return new BadRequestObjectResult("No session token");

            var sessionToken = req.Headers["SessionToken"];
            
            var user = await Entities.GetUserBySessionAsync(sessionToken);
            if (user == null)
                return new BadRequestObjectResult("Bad session token");

            string channelNumber = req.Query["channelNumber"];

            var messages = await Entities.ReadMessages(channelNumber);

            return new OkObjectResult(messages);
        }
    }
}
