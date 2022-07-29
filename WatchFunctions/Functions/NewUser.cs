using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using WatchFunctions.Model;

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
                log.LogInformation("NewUser function processed a request.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var newUser = JsonConvert.DeserializeObject<Dtos.NewUserDto>(requestBody);

                var existingUser = await Entities.GetEntityAsync<UserEntity>("users", "user", newUser.Handle);
                if (existingUser != null)
                    return new BadRequestObjectResult("Handle already in use.");

                var newEntity = new Model.UserEntity()
                {
                    PartitionKey = "user",
                    RowKey = newUser.Handle,
                    Email = newUser.Email
                };
                newEntity.Salt = HashAndSalt.GenerateSalt();
                newEntity.Password = HashAndSalt.GetHash(newUser.Password, newEntity.Salt);
                _ = await Entities.SaveEntityAsync("users", newEntity);
                string responseMessage = $"Hello, {newUser.Handle}. Your details have been added.";
                return new OkObjectResult(responseMessage);
            }
            catch (Exception)
            {
                return new BadRequestObjectResult("Adding user failed.");
            }
        }
    }
}
