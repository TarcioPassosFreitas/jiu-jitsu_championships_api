using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations;

internal class ChampionshipConfiguration : BaseEntityConfiguration<Championship>
{
    public override void Configure(EntityTypeBuilder<Championship> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Image).HasMaxLength(255).IsRequired();
        builder.Property(x => x.City).HasMaxLength(50).IsRequired();
        builder.Property(x => x.State).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EventDate).IsRequired();
        builder.Property(x => x.AboutEvent).IsRequired();
        builder.Property(x => x.Gym).IsRequired();
        builder.Property(x => x.GeneralInfo).IsRequired();
        builder.Property(x => x.PublicEntry).IsRequired(false);
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Phase).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.Highlights).IsRequired(false);
        builder.Property(x => x.HighlightOrder).IsRequired(false);

        builder
            .HasMany(x => x.Fights)
            .WithOne(x => x.Championship)
            .HasForeignKey(x => x.ChampionshipId)
            .IsRequired();

        builder
            .HasMany(x => x.Results)
            .WithOne(x => x.Championship)
            .HasForeignKey(x => x.ChampionshipId)
            .IsRequired();

        builder
            .HasMany(x => x.Certificates)
            .WithOne(x => x.Championship)
            .HasForeignKey(x => x.ChampionshipId)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Title).IsUnique();
    }
}