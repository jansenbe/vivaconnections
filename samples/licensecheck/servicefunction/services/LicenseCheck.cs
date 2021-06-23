using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace services
{
    public static class LicenseCheck
    {
        [FunctionName("LicenseCheck")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ClaimsPrincipal claimIdentity)
        {
            log.LogInformation("License check service called");

            log.LogInformation("Information from used access token:");
            log.LogInformation("User ID: " + claimIdentity.Identity.Name);
            log.LogInformation("Claim Type : Claim Value");
            foreach (Claim claim in claimIdentity.Claims)
            {
                log.LogInformation(claim.Type + " : " + claim.Value + "\n");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation("Information from received payload:");
            log.LogInformation(requestBody);

            var random = new Random();
            var randomBool = random.Next(2) == 1;

            var jsonResponse = JsonSerializer.Serialize(new LicenseInfo()
            {
                IsLicensed = randomBool,
                AccessToken = $"Randomstringactingastoken{new Random().Next(1, 10000)}"
            });

            log.LogInformation(jsonResponse);

            return new OkObjectResult(jsonResponse);
        }
    }
}
