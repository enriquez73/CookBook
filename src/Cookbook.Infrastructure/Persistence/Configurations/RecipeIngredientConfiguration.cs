using Cookbook.Domain.Entities;
using Cookbook.Domain.SmartEnums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cookbook.Infrastructure.Persistence.Configurations;

public sealed class RecipeIngredientConfiguration
    : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable(DbConstants.RecipeIngredientTableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Quantity)
            .HasPrecision(8, 3)
            .IsRequired();

        builder.Property(x => x.MeasurementUnit)
            .HasConversion(
                unit => unit.Id,
                id => MeasurementUnit.FromId(id))
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.Recipe)
            .WithMany(x => x.Ingredients)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Ingredient)
            .WithMany()
            .HasForeignKey(x => x.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
            {
                x.RecipeId,
                x.IngredientId
            })
            .IsUnique();
    }
}