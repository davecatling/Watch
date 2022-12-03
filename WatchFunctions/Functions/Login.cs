using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WatchFunctions.Model;
using System.Linq;
using WatchFunctions.Dtos;
using System.Text;

namespace WatchFunctions.Functions
{
    public static class Login
    {
        [FunctionName("Login")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string handle = req.Query["handle"];
                string password = req.Query["password"];
                if (handle == null || password == null)
                    throw new ArgumentException("Handle and password required");
                var user = await Entities.GetEntityAsync<UserEntity>("users", "user", handle);
                if (user == null) throw new ArgumentException("Check username and password");
                var hashedPassword = HashAndSalt.GetHash(password, user.Salt);
                if (!hashedPassword.SequenceEqual(user.Password)) throw new ArgumentException("Check username and password");
                user.LastAccess = DateTime.Now.ToString();
                user.SessionToken = Guid.NewGuid().ToString();
                await Entities.UpdateEntityAsync("users", user);
                return new OkObjectResult(new LoginDto()
                {
                    // Password value returned is the salted hash of the actual password to be used as the PKCS file encryption key
                    Password = new UTF8Encoding().GetString(hashedPassword),
                    SessionToken = user.SessionToken
                }
                );
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Login failed: {ex.Message}");
            }
        }
    }
}
