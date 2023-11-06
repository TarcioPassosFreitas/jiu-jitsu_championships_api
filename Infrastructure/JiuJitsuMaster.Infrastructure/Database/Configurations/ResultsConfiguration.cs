using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations;

internal class ResultsEntityConfiguration : BaseEntityConfiguration<Results>
{
    public override void Configure(EntityTypeBuilder<Results> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.ChampionshipId).IsRequired();
        builder.Property(x => x.Belt).HasConversion<int>().IsRequired();
        builder.Property(x => x.Weight).HasConversion<int>().IsRequired();

        builder
            .HasOne(x => x.Championship)
            .WithMany(x => x.Results)
            .HasForeignKey(x => x.ChampionshipId)
            .IsRequired();
    }
}