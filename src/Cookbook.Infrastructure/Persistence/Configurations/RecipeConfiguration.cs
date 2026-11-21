using Cookbook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cookbook.Infrastructure.Persistence.Configurations;

public sealed class RecipeConfiguration
    : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable(DbConstants.RecipeTableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.Source)
            .HasMaxLength(500);

        builder.Property(x => x.Difficulty)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CookingProgram)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PreparationTimeMinutes)
            .IsRequired();

        builder.Property(x => x.CookingTimeMinutes)
            .IsRequired();

        builder.Property(x => x.Servings)
            .IsRequired();

        builder.Ignore(x => x.TotalTimeMinutes);

        builder.Ignore(x => x.PrimaryImage);

        builder.Ignore(x => x.HasImages);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100);

        builder.HasMany(x => x.Ingredients)
            .WithOne(x => x.Recipe)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Ingredients)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Steps)
            .WithOne(x => x.Recipe)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Recipe)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Categories)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RecipeCategories",
                right => right
                    .HasOne<Category>()
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Recipe>()
                    .WithMany()
                    .HasForeignKey("RecipeId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("RecipeId", "CategoryId");

                    join.HasIndex("CategoryId");

                    join.ToTable(DbConstants.RecipeCategoryTableName);
                });

        builder.Navigation(x => x.Categories)
            .UsePropertyAccessMode(PropertyAccessMode.Field);


        // builder.HasMany(x => x.Tags)
        //     .WithMany()
        //     .UsingEntity(j =>
        //     {
        //         j.ToTable("RecipeTags");
        //     });

        builder.HasMany(x => x.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RecipeTags",
                right => right
                    .HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Recipe>()
                    .WithMany()
                    .HasForeignKey("RecipeId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("RecipeId", "TagId");

                    join.HasIndex("TagId");

                    // join.ToTable("RecipeTags");
                    join.ToTable(DbConstants.RecipeTagTableName);
                });

        builder.Navigation(x => x.Tags)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // builder.HasMany(x => x.Equipments)
        //     .WithMany()
        //     .UsingEntity(j =>
        //     {
        //         j.ToTable("RecipeEquipments");
        //     });
        builder.HasMany(x => x.Equipments)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RecipeEquipments",
                right => right
                    .HasOne<Equipment>()
                    .WithMany()
                    .HasForeignKey("EquipmentId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Recipe>()
                    .WithMany()
                    .HasForeignKey("RecipeId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("RecipeId", "EquipmentId");

                    join.HasIndex("EquipmentId");

                    join.ToTable(DbConstants.RecipeEquipmentTableName);
                });

        builder.Navigation(x => x.Equipments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}