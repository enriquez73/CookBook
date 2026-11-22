using Cookbook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cookbook.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CookbookDbContext))]
[Migration("20261121190000_AddRecipeCookingAppliances")]
public sealed class AddRecipeCookingAppliances : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RecipeCookingAppliances",
            columns: table => new
            {
                RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                Appliance = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecipeCookingAppliances", x => new { x.RecipeId, x.Appliance });
                table.ForeignKey(
                    name: "FK_RecipeCookingAppliances_Recipes_RecipeId",
                    column: x => x.RecipeId,
                    principalTable: "Recipes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("""
            INSERT INTO "RecipeCookingAppliances" ("RecipeId", "Appliance")
            SELECT "Id", "CookingAppliance" FROM "Recipes";
            """);

        migrationBuilder.DropColumn(name: "CookingAppliance", table: "Recipes");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CookingAppliance",
            table: "Recipes",
            type: "character varying(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "RiceCooker");

        migrationBuilder.Sql("""
            UPDATE "Recipes" AS r
            SET "CookingAppliance" = COALESCE(
                (SELECT MIN(a."Appliance") FROM "RecipeCookingAppliances" AS a WHERE a."RecipeId" = r."Id"),
                'RiceCooker');
            """);

        migrationBuilder.DropTable(name: "RecipeCookingAppliances");
    }
}
