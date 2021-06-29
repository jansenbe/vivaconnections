using Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleMvcApp.Controllers
{
    public class AccountController : Controller
    {
        private IConfiguration Configuration { get; }

        public AccountController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be whitelisted in the 
                // **Allowed Logout URLs** settings for the client.
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// This is just a helper action to enable you to easily see all claims related to a user. It helps when debugging your
        /// application to see the in claims populated from the Auth0 ID Token
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Claims()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> Orders()
        {

            if (User.Identity.IsAuthenticated)
            {
                /*
                string accessToken = await HttpContext.GetTokenAsync("access_token");

                // if you need to check the Access Token expiration time, use this value
                // provided on the authorization response and stored.
                // do not attempt to inspect/decode the access token
                DateTime accessTokenExpiresAt = DateTime.Parse(
                    await HttpContext.GetTokenAsync("expires_at"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

                string idToken = await HttpContext.GetTokenAsync("accessToken");

                // Now you can use them. For more info on when and how to use the
                // Access Token and ID Token, see https://auth0.com/docs/tokens
                */


                var client = new RestClient("https://bertonline.eu.auth0.com/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", $"{{\"client_id\":\"{Configuration["Auth0:ClientId"]}\",\"client_secret\":\"{Configuration["Auth0:ClientSecret"]}\",\"audience\":\"{Configuration["Auth0:Audience"]}\",\"grant_type\":\"client_credentials\"}}", ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request);

                var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
                string accessToken = json.GetProperty("access_token").GetString();

                //string accessToken = await HttpContext.GetTokenAsync("access_token");

                client = new RestClient($"{Configuration["Products:API"]}");
                request = new RestRequest(Method.GET);
                request.AddHeader("authorization", $"Bearer {accessToken}");
                response = await client.ExecuteAsync(request);
                
                var orders = JsonSerializer.Deserialize<List<Order>>(response.Content, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return View(orders);
            }

            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
