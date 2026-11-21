using Cookbook.Domain.Entities;
using Cookbook.Domain.SmartEnums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cookbook.Infrastructure.Persistence.Configurations;

public sealed class IngredientConfiguration
    : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable(DbConstants.IngredientTableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.DefaultMeasurementUnit)
            .HasConversion(
                unit => unit.Id,
                id => MeasurementUnit.FromId(id))
            .IsRequired();

        builder.OwnsOne(x => x.Nutrition, nutrition =>
        {
            nutrition.Property(x => x.Calories)
                .HasColumnName("Calories")
                .HasPrecision(8, 2);

            nutrition.Property(x => x.Protein)
                .HasColumnName("Protein")
                .HasPrecision(8, 2);

            nutrition.Property(x => x.Carbohydrates)
                .HasColumnName("Carbohydrates")
                .HasPrecision(8, 2);

            nutrition.Property(x => x.Fat)
                .HasColumnName("Fat")
                .HasPrecision(8, 2);

            nutrition.Property(x => x.Fiber)
                .HasColumnName("Fiber")
                .HasPrecision(8, 2);

            nutrition.Property(x => x.Sugar)
                .HasColumnName("Sugar")
                .HasPrecision(8, 2);

            nutrition.Property(x => x.SaturatedFat)
                .HasColumnName("SaturatedFat")
                .HasPrecision(8, 2);

            nutrition.Property(x => x.Salt)
                .HasColumnName("Salt")
                .HasPrecision(8, 2);
        });

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100);
    }
}