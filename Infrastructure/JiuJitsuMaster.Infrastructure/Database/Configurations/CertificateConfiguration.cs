using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations
{
    internal class CertificateConfiguration : BaseEntityConfiguration<Certificate>
    {
        public override void Configure(EntityTypeBuilder<Certificate> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Type).HasConversion<int>().IsRequired();
            builder.Property(x => x.AthleteId).IsRequired();
            builder.Property(x => x.ChampionshipId).IsRequired();

            builder
                .HasOne(x => x.Athlete)
                .WithMany(x => x.Certificates)
                .HasForeignKey(x => x.AthleteId)
                .IsRequired();

            builder
                .HasOne(x => x.Championship)
                .WithMany(x => x.Certificates)
                .HasForeignKey(x => x.ChampionshipId)
                .IsRequired();
        }
    }
}