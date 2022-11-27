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
using System.Text.RegularExpressions;

namespace WatchFunctions.Functions
{
    public static class NewUser
    {
        [FunctionName("NewUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var newUser = JsonConvert.DeserializeObject<Dtos.NewUserDto>(requestBody);
                if (newUser.Handle == "SYSTEM")
                    throw new ArgumentException("Reserved handle");
                var existingUser = await Entities.GetEntityAsync<UserEntity>("users", "user", newUser.Handle);
                if (existingUser != null)
                    throw new ArgumentException("Handle in use");
                if (!ComplexPassword(newUser.Password))
                    throw new ArgumentException("Password not complex enough");
                var newEntity = new UserEntity()
                {
                    PartitionKey = "user",
                    RowKey = newUser.Handle,
                    PublicKey = newUser.PublicKey
                };
                newEntity.Salt = HashAndSalt.GenerateSalt();
                newEntity.Password = HashAndSalt.GetHash(newUser.Password, newEntity.Salt);
                _ = await Entities.SaveEntityAsync("users", newEntity);
                string responseMessage = $"Hello, {newUser.Handle}. Your details have been added.";
                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"User creation failed: {ex.Message}");
            }
        }

        private static bool ComplexPassword(string password)
        {
            if (password == null) return false;
            if (password.Length < 8) return false;
            var regex = new Regex(@"[\!@#$%^&*()\\[\]{}\-_+=~`|:;/""'<>,./?]");
            if (!regex.IsMatch(password)) return false;
            regex = new Regex(@"[a-z]");
            if (!regex.IsMatch(password)) return false;
            regex = new Regex(@"[A-Z]");
            if (!regex.IsMatch(password)) return false;
            regex = new Regex(@"[0-9]");
            if (!regex.IsMatch(password)) return false;
            return true;
        }
    }
}
