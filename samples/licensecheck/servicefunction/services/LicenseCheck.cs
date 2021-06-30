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
using Microsoft.EntityFrameworkCore;

namespace services
{
    public class LicenseCheck
    {

        private readonly TokenCacheContext TokenCacheContext;

        public LicenseCheck(TokenCacheContext tokenCacheContext)
        {
            TokenCacheContext = tokenCacheContext;
        }

        [FunctionName("LicenseCheck")]
        public async Task<IActionResult> Run(
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

            // check if there's an access token for this user
            var auth0AccessToken = await TokenCacheContext.TokenCaches.FirstOrDefaultAsync(p => p.Upn == claimIdentity.Identity.Name.ToLower());

            string accessToken = $"";

            if (auth0AccessToken != null)
            {
                log.LogInformation($"Access token retrieved. Expires at {auth0AccessToken.ExpirationTime}, value: {auth0AccessToken.AccessToken}");
                if (auth0AccessToken.ExpirationTime > DateTime.Now)
                {
                    log.LogInformation("Access token can be used!");
                    accessToken = auth0AccessToken.AccessToken;
                }
            }

            var jsonResponse = JsonSerializer.Serialize(new LicenseInfo()
            {
                IsLicensed = randomBool,
                AccessToken = accessToken
            });

            log.LogInformation(jsonResponse);

            return new OkObjectResult(jsonResponse);
        }
    }
}
