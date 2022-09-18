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
    public static class PublicKey
    {
        [FunctionName("PublicKey")]
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
                string handle = req.Query["handle"];
                var keyUser = await Entities.GetEntityAsync<UserEntity>("users", "user", handle);
                if (keyUser == null)
                    throw new ArgumentException("Invalid user");
                return new OkObjectResult(keyUser.PublicKey);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Getting public key failed: {ex.Message}");
            }
        }
    }
}
