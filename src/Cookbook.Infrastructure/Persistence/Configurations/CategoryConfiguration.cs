using Cookbook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cookbook.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Navigation(x => x.Recipes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Recipes)
            .WithMany(x => x.Categories)
            .UsingEntity<Dictionary<string, object>>(
                "RecipeCategories",
                right => right
                    .HasOne<Recipe>()
                    .WithMany()
                    .HasForeignKey("RecipeId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Category>()
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("RecipeCategories");

                    join.HasKey("RecipeId", "CategoryId");

                    join.HasIndex("CategoryId");
                });
    }
}