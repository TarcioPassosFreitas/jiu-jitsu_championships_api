using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations;

internal class AthleteFightConfiguration : BaseEntityConfiguration<AthleteFight>
{
    public override void Configure(EntityTypeBuilder<AthleteFight> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.AthleteId).IsRequired();
        builder.Property(x => x.FightId).IsRequired();
        builder.Property(f => f.Winner).IsRequired(false);
        builder.Property(f => f.IsFinished).HasDefaultValue(false);


        builder
            .HasOne(x => x.Athlete)
            .WithMany(x => x.AthleteFights)
            .HasForeignKey(x => x.AthleteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Fight)
            .WithMany(x => x.AthleteFights)
            .HasForeignKey(x => x.FightId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}