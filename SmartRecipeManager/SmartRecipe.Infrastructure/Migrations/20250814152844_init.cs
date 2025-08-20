using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartRecipe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    PrepTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    CookTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    Servings = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFiles_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NutritionInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Calories = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    Protein = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Fat = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Carbs = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionInfo_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "PasswordHash", "ProfileImageUrl", "Role", "UserName" },
                values: new object[,]
                {
                    { new Guid("10ca4c8b-470c-42e5-a957-8cca562ac76c"), "john@smartrecipe.com", "String123!", "https://drive.google.com/file/d/john_profile_image", "Contributor", "chef_john" },
                    { new Guid("fecb7ca0-067c-4700-bb94-104e4bc9640d"), "admin@smartrecipe.com", "String123!", "https://drive.google.com/file/d/admin_profile_image", "Admin", "admin" }
                });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "Category", "CookTimeMinutes", "CreatedAt", "CreatedByUserId", "Description", "PrepTimeMinutes", "Servings", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6"), 2, 30, new DateTime(2025, 8, 4, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("10ca4c8b-470c-42e5-a957-8cca562ac76c"), "A classic Italian pasta dish with rich meat sauce.", 15, 4, "Spaghetti Bolognese", new DateTime(2025, 8, 9, 15, 30, 0, 0, DateTimeKind.Utc) },
                    { new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46"), 3, 12, new DateTime(2025, 8, 9, 14, 0, 0, 0, DateTimeKind.Utc), new Guid("10ca4c8b-470c-42e5-a957-8cca562ac76c"), "Warm, gooey chocolate cake with a molten center.", 20, 2, "Chocolate Lava Cake", new DateTime(2025, 8, 14, 12, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreatedAt", "RecipeId", "UserId" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777771"), "Delicious recipe, easy to follow!", new DateTime(2025, 8, 5, 16, 0, 0, 0, DateTimeKind.Utc), new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6"), new Guid("fecb7ca0-067c-4700-bb94-104e4bc9640d") },
                    { new Guid("77777777-7777-7777-7777-777777777772"), "Perfect dessert, loved the gooey center!", new DateTime(2025, 8, 10, 18, 30, 0, 0, DateTimeKind.Utc), new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46"), new Guid("fecb7ca0-067c-4700-bb94-104e4bc9640d") }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "Name", "Quantity", "RecipeId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Spaghetti", "200g", new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6") },
                    { new Guid("11111111-1111-1111-1111-111111111112"), "Ground Beef", "500g", new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6") },
                    { new Guid("11111111-1111-1111-1111-111111111113"), "Tomato Sauce", "400ml", new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6") },
                    { new Guid("22222222-2222-2222-2222-222222222221"), "Chocolate", "200g", new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Butter", "100g", new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46") },
                    { new Guid("22222222-2222-2222-2222-222222222223"), "Sugar", "150g", new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46") }
                });

            migrationBuilder.InsertData(
                table: "Likes",
                columns: new[] { "Id", "RecipeId", "UserId" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-888888888881"), new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6"), new Guid("fecb7ca0-067c-4700-bb94-104e4bc9640d") },
                    { new Guid("88888888-8888-8888-8888-888888888882"), new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46"), new Guid("fecb7ca0-067c-4700-bb94-104e4bc9640d") }
                });

            migrationBuilder.InsertData(
                table: "MediaFiles",
                columns: new[] { "Id", "FileName", "FileType", "FileUrl", "RecipeId", "UploadedAt" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-6666-6666-666666666661"), "spaghetti_bolognese.jpg", "Image", "https://drive.google.com/file/d/spaghetti_image", new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6"), new DateTime(2025, 8, 4, 10, 30, 0, 0, DateTimeKind.Utc) },
                    { new Guid("66666666-6666-6666-6666-666666666662"), "lava_cake_video.mp4", "Video", "https://drive.google.com/file/d/lava_cake_video", new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46"), new DateTime(2025, 8, 9, 14, 30, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "NutritionInfo",
                columns: new[] { "Id", "Calories", "Carbs", "Fat", "Protein", "RecipeId" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555551"), 800.00m, 90.75m, 25.25m, 35.50m, new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6") },
                    { new Guid("55555555-5555-5555-5555-555555555552"), 600.00m, 70.75m, 30.50m, 10.25m, new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46") }
                });

            migrationBuilder.InsertData(
                table: "Steps",
                columns: new[] { "Id", "Description", "Order", "RecipeId" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333331"), "Boil spaghetti until al dente.", 1, new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6") },
                    { new Guid("33333333-3333-3333-3333-333333333332"), "Cook ground beef with tomato sauce.", 2, new Guid("1236c0bf-8dac-4455-8706-ecf615980ed6") },
                    { new Guid("44444444-4444-4444-4444-444444444441"), "Melt chocolate and butter together.", 1, new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46") },
                    { new Guid("44444444-4444-4444-4444-444444444442"), "Mix with sugar and bake for 12 minutes.", 2, new Guid("9014ec0c-55aa-465b-ace6-5c5da9d6bf46") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_RecipeId",
                table: "Comments",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_RecipeId",
                table: "Ingredients",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_RecipeId_UserId",
                table: "Likes",
                columns: new[] { "RecipeId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_RecipeId",
                table: "MediaFiles",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionInfo_RecipeId",
                table: "NutritionInfo",
                column: "RecipeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CreatedByUserId",
                table: "Recipes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_RecipeId",
                table: "Steps",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropTable(
                name: "NutritionInfo");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
