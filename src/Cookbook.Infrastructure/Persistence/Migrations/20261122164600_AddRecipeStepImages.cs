using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cookbook.Infrastructure.Persistence.Migrations;

public partial class AddRecipeStepImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ImageContentType",
            table: "RecipeStep",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ImageFileName",
            table: "RecipeStep",
            type: "character varying(300)",
            maxLength: 300,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ImageContentType", table: "RecipeStep");
        migrationBuilder.DropColumn(name: "ImageFileName", table: "RecipeStep");
    }
}
