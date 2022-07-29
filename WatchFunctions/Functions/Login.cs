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
using System.Linq;

namespace WatchFunctions.Functions
{
    public static class Login
    {
        [FunctionName("Login")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string handle = req.Query["handle"];
            string password = req.Query["password"];

            if (handle == null || password == null)
                return new BadRequestObjectResult("Handle and password required.");

            var user = await Entities.GetEntityAsync<UserEntity>("users", "user", handle);
            if (user == null) return new BadRequestObjectResult("Login failed.");

            var hashedPassword = HashAndSalt.GetHash(password, user.Salt);

            if (!hashedPassword.SequenceEqual(user.Password)) return new BadRequestObjectResult("Login failed.");

            return new OkObjectResult($"{user.RowKey} logged in OK.");
        }
    }
}
