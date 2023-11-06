using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations;

internal class FightEntityConfiguration : BaseEntityConfiguration<Fight>
{
    public override void Configure(EntityTypeBuilder<Fight> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.ChampionshipId).IsRequired();
        builder.Property(x => x.Belt).HasConversion<int>().IsRequired();
        builder.Property(x => x.Weight).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsInternalTeamFight).IsRequired(false);
        builder.Property(f => f.Round).IsRequired();

        builder
            .HasOne(x => x.Championship)
            .WithMany(x => x.Fights)
            .HasForeignKey(x => x.ChampionshipId)
            .IsRequired();
    }
}