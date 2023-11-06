using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations
{
    internal class AthleteConfiguration : BaseEntityConfiguration<Athlete>
    {
        public override void Configure(EntityTypeBuilder<Athlete> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.BirthDate).IsRequired();
            builder.Property(x => x.CPF).HasMaxLength(11).IsRequired();
            builder.Property(x => x.Gender).HasConversion<int>().IsRequired();
            builder.Property(x => x.Email).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Password).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Team).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Belt).HasConversion<int>().IsRequired();
            builder.Property(x => x.Weight).HasConversion<int>().IsRequired();

            builder
            .HasMany(x => x.UserRecoverPasswords)
            .WithOne(x => x.Athlete)
            .HasForeignKey(x => x.AtleteId)
            .IsRequired();

            builder
                .HasMany(x => x.UserCreatePasswords)
                .WithOne(x => x.Athlete)
                .HasForeignKey(x => x.AtleteId)
                .IsRequired();

            builder
                .HasMany(x => x.Certificates)
                .WithOne(x => x.Athlete)
                .HasForeignKey(x => x.AthleteId)
                .IsRequired();

            builder
              .HasOne(x => x.User)
              .WithOne(x => x.Athlete)
              .HasForeignKey<Athlete>(x => x.UserId)
              .IsRequired(false)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.CPF).IsUnique();
            builder.HasIndex(x => x.Email).IsUnique();
        }
    }
}