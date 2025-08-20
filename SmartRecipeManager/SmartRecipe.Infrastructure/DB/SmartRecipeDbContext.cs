using Microsoft.EntityFrameworkCore;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Domain.Enum;

namespace SmartRecipe.Infrastructure.DB
{
    public class SmartRecipeDbContext : DbContext
    {
        // Remove parameterless constructor to fix DI issues
        public SmartRecipeDbContext(DbContextOptions<SmartRecipeDbContext> options) : base(options)
        {
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<NutritionInfo> NutritionInfo { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<MediaFile> MediaFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartRecipeDbContext).Assembly);

            // Configure decimal precision for NutritionInfo
            modelBuilder.Entity<NutritionInfo>(entity =>
            {
                entity.Property(e => e.Calories)
                    .HasPrecision(7, 2); // Up to 99999.99 calories

                entity.Property(e => e.Protein)
                    .HasPrecision(6, 2); // Up to 9999.99 grams

                entity.Property(e => e.Fat)
                    .HasPrecision(6, 2); // Up to 9999.99 grams

                entity.Property(e => e.Carbs)
                    .HasPrecision(6, 2); // Up to 9999.99 grams
            });

            // Fixed seed data with static values (no dynamic Guid.NewGuid() or DateTime.UtcNow)
            var adminId = new Guid("fecb7ca0-067c-4700-bb94-104e4bc9640d");
            var contributorId = new Guid("10ca4c8b-470c-42e5-a957-8cca562ac76c");
            var recipe1Id = new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6");
            var recipe2Id = new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46");

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminId,
                    UserName = "admin",
                    Email = "admin@smartrecipe.com",
                    PasswordHash = "String123!",
                    Role = UserRole.Admin,
                    ProfileImageUrl = "https://drive.google.com/file/d/admin_profile_image"
                },
                new User
                {
                    Id = contributorId,
                    UserName = "chef_john",
                    Email = "john@smartrecipe.com",
                    PasswordHash = "String123!",
                    Role = UserRole.Contributor,
                    ProfileImageUrl = "https://drive.google.com/file/d/john_profile_image"
                }
            );

            // Seed Recipes with static dates
            modelBuilder.Entity<Recipe>().HasData(
                new Recipe
                {
                    Id = recipe1Id,
                    Title = "Spaghetti Bolognese",
                    Description = "A classic Italian pasta dish with rich meat sauce.",
                    Category = RecipeCategory.MainCourse,
                    PrepTimeMinutes = 15,
                    CookTimeMinutes = 30,
                    Servings = 4,
                    CreatedAt = new DateTime(2025, 8, 4, 10, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 8, 9, 15, 30, 0, DateTimeKind.Utc),
                    CreatedByUserId = contributorId
                },
                new Recipe
                {
                    Id = recipe2Id,
                    Title = "Chocolate Lava Cake",
                    Description = "Warm, gooey chocolate cake with a molten center.",
                    Category = RecipeCategory.Dessert,
                    PrepTimeMinutes = 20,
                    CookTimeMinutes = 12,
                    Servings = 2,
                    CreatedAt = new DateTime(2025, 8, 9, 14, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 8, 14, 12, 0, 0, DateTimeKind.Utc),
                    CreatedByUserId = contributorId
                }
            );

            // Seed Ingredients with static GUIDs
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Spaghetti", Quantity = "200g", RecipeId = recipe1Id },
                new Ingredient { Id = new Guid("11111111-1111-1111-1111-111111111112"), Name = "Ground Beef", Quantity = "500g", RecipeId = recipe1Id },
                new Ingredient { Id = new Guid("11111111-1111-1111-1111-111111111113"), Name = "Tomato Sauce", Quantity = "400ml", RecipeId = recipe1Id },
                new Ingredient { Id = new Guid("22222222-2222-2222-2222-222222222221"), Name = "Chocolate", Quantity = "200g", RecipeId = recipe2Id },
                new Ingredient { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "Butter", Quantity = "100g", RecipeId = recipe2Id },
                new Ingredient { Id = new Guid("22222222-2222-2222-2222-222222222223"), Name = "Sugar", Quantity = "150g", RecipeId = recipe2Id }
            );

            // Seed Steps with static GUIDs
            modelBuilder.Entity<Step>().HasData(
                new Step { Id = new Guid("33333333-3333-3333-3333-333333333331"), Description = "Boil spaghetti until al dente.", Order = 1, RecipeId = recipe1Id },
                new Step { Id = new Guid("33333333-3333-3333-3333-333333333332"), Description = "Cook ground beef with tomato sauce.", Order = 2, RecipeId = recipe1Id },
                new Step { Id = new Guid("44444444-4444-4444-4444-444444444441"), Description = "Melt chocolate and butter together.", Order = 1, RecipeId = recipe2Id },
                new Step { Id = new Guid("44444444-4444-4444-4444-444444444442"), Description = "Mix with sugar and bake for 12 minutes.", Order = 2, RecipeId = recipe2Id }
            );

            // Seed NutritionInfo with static GUIDs and proper decimal values
            modelBuilder.Entity<NutritionInfo>().HasData(
                new NutritionInfo
                {
                    Id = new Guid("55555555-5555-5555-5555-555555555551"),
                    Calories = 800.00m,
                    Protein = 35.50m,
                    Fat = 25.25m,
                    Carbs = 90.75m,
                    RecipeId = recipe1Id
                },
                new NutritionInfo
                {
                    Id = new Guid("55555555-5555-5555-5555-555555555552"),
                    Calories = 600.00m,
                    Protein = 10.25m,
                    Fat = 30.50m,
                    Carbs = 70.75m,
                    RecipeId = recipe2Id
                }
            );

            // Seed MediaFiles with static GUIDs and dates
            modelBuilder.Entity<MediaFile>().HasData(
                new MediaFile
                {
                    Id = new Guid("66666666-6666-6666-6666-666666666661"),
                    FileName = "spaghetti_bolognese.jpg",
                    FileUrl = "https://drive.google.com/file/d/spaghetti_image",
                    FileType = MediaType.Image,
                    UploadedAt = new DateTime(2025, 8, 4, 10, 30, 0, DateTimeKind.Utc),
                    RecipeId = recipe1Id
                },
                new MediaFile
                {
                    Id = new Guid("66666666-6666-6666-6666-666666666662"),
                    FileName = "lava_cake_video.mp4",
                    FileUrl = "https://drive.google.com/file/d/lava_cake_video",
                    FileType = MediaType.Video,
                    UploadedAt = new DateTime(2025, 8, 9, 14, 30, 0, DateTimeKind.Utc),
                    RecipeId = recipe2Id
                }
            );

            // Seed Comments with static GUIDs and dates
            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777771"),
                    Content = "Delicious recipe, easy to follow!",
                    CreatedAt = new DateTime(2025, 8, 5, 16, 0, 0, DateTimeKind.Utc),
                    RecipeId = recipe1Id,
                    UserId = adminId
                },
                new Comment
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777772"),
                    Content = "Perfect dessert, loved the gooey center!",
                    CreatedAt = new DateTime(2025, 8, 10, 18, 30, 0, DateTimeKind.Utc),
                    RecipeId = recipe2Id,
                    UserId = adminId
                }
            );

            // Seed Likes with static GUIDs
            modelBuilder.Entity<Like>().HasData(
                new Like { Id = new Guid("88888888-8888-8888-8888-888888888881"), RecipeId = recipe1Id, UserId = adminId },
                new Like { Id = new Guid("88888888-8888-8888-8888-888888888882"), RecipeId = recipe2Id, UserId = adminId }
            );
        }

        // Add this method for migration support
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // This is for design-time migrations only
                optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=SmartRecipeManager;Persist Security Info=True;User ID=sa;Password=12345;Encrypt=false");
            }
        }
    }
}