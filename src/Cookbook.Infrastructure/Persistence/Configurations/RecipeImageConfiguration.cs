using Cookbook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cookbook.Infrastructure.Persistence.Configurations;

public sealed class RecipeImageConfiguration
    : IEntityTypeConfiguration<RecipeImage>
{
    public void Configure(EntityTypeBuilder<RecipeImage> builder)
    {
        builder.ToTable(DbConstants.RecipeImageTableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.Width)
            .IsRequired();

        builder.Property(x => x.Height)
            .IsRequired();

        builder.Property(x => x.Checksum)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsPrimary)
            .IsRequired();

        builder.HasOne(x => x.Recipe)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
            {
                x.RecipeId,
                x.SortOrder
            })
            .IsUnique();
        
        builder.HasIndex(x => new
        {
            x.RecipeId,
            x.IsPrimary
        });

        builder.HasIndex(x => x.Checksum);
    }
}