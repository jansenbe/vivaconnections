using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

[assembly: FunctionsStartup(typeof(services.Startup))]
namespace services
{
    /// <summary>
    /// Class that initializes this Azure functions host
    /// </summary>
    public class Startup : FunctionsStartup
    {
        // Override the Configure method to load configuration values
        // from the .NET user secret store
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //#region Configuration
            //var config = new ConfigurationBuilder()
            //    //.AddUserSecrets(Assembly.GetExecutingAssembly(), false)
            //    .AddEnvironmentVariables()
            //    .Build();

            //// Make the loaded config available via dependency injection
            //builder.Services.AddSingleton<IConfiguration>(config);
            //var configuration = builder.GetContext().Configuration;
            //#endregion

            string SqlConnection = Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection");
            builder.Services.AddDbContext<TokenCacheContext>(
                options => options.UseSqlServer(SqlConnection));
        }
    }
}