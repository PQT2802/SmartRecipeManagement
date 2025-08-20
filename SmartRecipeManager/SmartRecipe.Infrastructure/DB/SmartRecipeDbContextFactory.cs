using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SmartRecipe.Infrastructure.DB
{
    public class SmartRecipeDbContextFactory : IDesignTimeDbContextFactory<SmartRecipeDbContext>
    {
        public SmartRecipeDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<SmartRecipeDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new SmartRecipeDbContext(optionsBuilder.Options);
        }
    }
}
