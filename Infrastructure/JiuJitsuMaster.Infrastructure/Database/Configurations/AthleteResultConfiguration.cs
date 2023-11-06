using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations;

internal class AthleteResultConfiguration : BaseEntityConfiguration<AthleteResult>
{
    public override void Configure(EntityTypeBuilder<AthleteResult> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.AthleteId).IsRequired();
        builder.Property(x => x.ResultId).IsRequired();

        builder
            .HasOne(x => x.Athlete)
            .WithMany(x => x.AthleteResults)
            .HasForeignKey(x => x.AthleteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Result)
            .WithMany(x => x.AthleteResults)
            .HasForeignKey(x => x.ResultId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}