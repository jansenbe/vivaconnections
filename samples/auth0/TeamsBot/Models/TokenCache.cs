using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamsBot.Models
{
    public class TokenCacheContext: DbContext
    {
        public IConfiguration Configuration { get; }

        public TokenCacheContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public DbSet<TokenCache> TokenCaches { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("SQL"));
            base.OnConfiguring(optionsBuilder);
        }
    }

    public class TokenCache
    {
        [Required]
        [Key]
        public string Upn { get; set; }
        
        [Required]
        public string AccessToken { get; set; }
        public DateTime ExpirationTime { get; set; }
    }


}
