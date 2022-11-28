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
using System.Text;
using System.Security.Cryptography;

namespace WatchFunctions.Functions
{
    public static class PasswordReset
    {
        [FunctionName("PasswordReset")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var dto = JsonConvert.DeserializeObject<Dtos.PasswordResetDto>(requestBody);
                // Validate handle has access to channel
                var user = await Entities.GetEntityAsync<UserEntity>("users", "user", dto.Handle);
                if (user == null)
                    throw new ArgumentException("Validation failure");
                // Decrypt and verify signature with public key
                RSACryptoServiceProvider rsa = new();
                rsa.KeySize = 2048;
                rsa.FromXmlString(user.PublicKey);
                var decryptedBytes = rsa.Decrypt(dto.Signature, true);
                var signature = new UnicodeEncoding().GetString(decryptedBytes);
                if (signature != $"{dto.Handle}{dto.ChannelNumber}")
                    throw new ArgumentException("Validation failure");
                // Set reset due value for end of timeout period
                user.PasswordResetDue = DateTime.Now.AddDays(3).ToString();               
                _ = await Entities.UpdateEntityAsync("users", user);
                return new OkObjectResult("OK");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Password reset failed: {ex.Message}");
            }
        }
    }
}
