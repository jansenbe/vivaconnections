// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using TeamsBot.Models;

namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : LogoutDialog
    {
        private readonly ILogger _logger;
        private IConfiguration Configuration { get; }

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _logger = logger;
            Configuration = configuration;

            AddDialog(new TokenExchangeOAuthPrompt(
                nameof(TokenExchangeOAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 1000 * 60 * 1, // User has 5 minutes to login (1000 * 60 * 5)

                    //EndOnInvalidMessage = true
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog),  new WaterfallStep[] { PromptStepAsync, LoginStepAsync, DisplayTokenPhase1Async, DisplayTokenPhase2Async }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(TokenExchangeOAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                // Pull in the data from the Microsoft Graph.
                //var client = new SimpleGraphClient(tokenResponse.Token);
                //var me = await client.GetMeAsync();
                //var title = !string.IsNullOrEmpty(me.JobTitle) ?
                //            me.JobTitle : "Unknown";

                //await stepContext.Context.SendActivityAsync($"You're logged in as {me.DisplayName} ({me.UserPrincipalName}); you job title is: {title}");

                var member = await TeamsInfo.GetMemberAsync(stepContext.Context, stepContext.Context.Activity.From.Id, cancellationToken);


                //var client = new RestClient("https://bertonline.eu.auth0.com/oauth/token");
                //var request = new RestRequest(Method.POST);
                //request.AddHeader("content-type", "application/json");
                //request.AddParameter("application/json", $"{{\"client_id\":\"{Configuration["Auth0:ClientId"]}\",\"client_secret\":\"{Configuration["Auth0:ClientSecret"]}\",\"audience\":\"{Configuration["Auth0:Audience"]}\",\"grant_type\":\"client_credentials\"}}", ParameterType.RequestBody);
                //IRestResponse response = await client.ExecuteAsync(request);

                //var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
                //string accessToken = json.GetProperty("access_token").GetString();

                //var client = new RestClient($"{Configuration["Products:API"]}");
                //var request = new RestRequest(Method.GET);
                //request.AddHeader("authorization", $"Bearer {tokenResponse.Token}");
                //var response = await client.ExecuteAsync(request);

                // Persist the access token

                using (var tokenCacheContext = new TokenCacheContext(Configuration))
                {
                    var existingCache = await tokenCacheContext.TokenCaches.FirstOrDefaultAsync(p => p.Upn == member.UserPrincipalName);
                    if (existingCache != null)
                    {
                        existingCache.AccessToken = tokenResponse.Token;
                    }
                    else
                    {
                        await tokenCacheContext.TokenCaches.AddAsync(new TokenCache() { Upn = member.UserPrincipalName, AccessToken = tokenResponse.Token });
                    }
                    await tokenCacheContext.SaveChangesAsync();
                }

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to view your token and the data retrieved with it?") }, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);

            var result = (bool)stepContext.Result;
            if (result)
            {
                // Call the prompt again because we need the token. The reasons for this are:
                // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
                // about refreshing it. We can always just call the prompt again to get the token.
                // 2. We never know how long it will take a user to respond. By the time the
                // user responds the token may have expired. The user would then be prompted to login again.
                //
                // There is no reason to store the token locally in the bot because we can always just call
                // the OAuth prompt to get the token or get a new token if needed.
                return await stepContext.BeginDialogAsync(nameof(TokenExchangeOAuthPrompt), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token {tokenResponse.Token}"), cancellationToken);

                var client = new RestClient($"{Configuration["Products:API"]}");
                var request = new RestRequest(Method.GET);
                request.AddHeader("authorization", $"Bearer {tokenResponse.Token}");
                var response = await client.ExecuteAsync(request);

                var orders = System.Text.Json.JsonSerializer.Deserialize<List<Order>>(response.Content, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                
                var cardAttachment = CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "productcard.json"), orders);
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment, "Here is your data"), cancellationToken);

            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Attachment CreateAdaptiveCardAttachment(string filePath, List<Order> orders)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);

            // Create a Template instance from the template payload
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);

            // You can use any serializable object as your data
            var myData = new
            {
                items = orders
            };

            // "Expand" the template - this generates the final Adaptive Card payload
            string cardJson = template.Expand(myData);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),                
            };
            return adaptiveCardAttachment;
        }
    }
}
