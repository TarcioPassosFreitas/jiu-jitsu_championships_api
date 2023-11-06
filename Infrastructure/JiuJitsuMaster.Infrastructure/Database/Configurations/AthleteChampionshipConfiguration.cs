using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations;

internal class AthleteChampionshipConfiguration : BaseEntityConfiguration<AthleteChampionship>
{
    public override void Configure(EntityTypeBuilder<AthleteChampionship> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.AthleteId).IsRequired();
        builder.Property(x => x.ChampionshipId).IsRequired();

        builder
            .HasOne(x => x.Athlete)
            .WithMany(x => x.AthleteChampionships)
            .HasForeignKey(x => x.AthleteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Championship)
            .WithMany(x => x.AthleteChampionships)
            .HasForeignKey(x => x.ChampionshipId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}