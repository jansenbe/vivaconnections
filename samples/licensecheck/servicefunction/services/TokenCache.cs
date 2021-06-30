using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;

namespace services
{
    public class TokenCacheContext : DbContext
    {
        public TokenCacheContext(DbContextOptions<TokenCacheContext> options) : base(options)
        {
        }

        public DbSet<TokenCache> TokenCaches { get; set; }

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

    public class BloggingContextFactory : IDesignTimeDbContextFactory<TokenCacheContext>
    {
        public TokenCacheContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TokenCacheContext>();
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection"));

            return new TokenCacheContext(optionsBuilder.Options);
        }
    }
}
