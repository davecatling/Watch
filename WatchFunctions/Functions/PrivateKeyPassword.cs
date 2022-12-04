using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WatchFunctions.Model;
using System.Text;

namespace WatchFunctions.Functions
{
    public static class PrivateKeyPassword
    {
        [FunctionName("PrivateKeyPassword")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string channelNumber = req.Query["channelNumber"];
                string handle = req.Query["handle"];
                // Validate handle has access to channel
                var user = await Entities.GetEntityAsync<UserEntity>("users", "user", handle);
                if (user == null)
                    throw new ArgumentException("Validation failure");
                var hasAccess = await Entities.HasAccess(channelNumber, user.RowKey);
                if (!hasAccess)
                    throw new InvalidOperationException("Validation failure");
                // Return the PKCS file encyption key (salted hash of login password)
                return new OkObjectResult(new UTF8Encoding().GetString(user.Password));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Private key password request failed: {ex.Message}");
            }
        }
    }
}
