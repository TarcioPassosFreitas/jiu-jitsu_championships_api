using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations;

internal class UserEntityConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Email).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Password).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.RefreshToken).HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.Role).HasConversion<int>().HasDefaultValue(UserRole.User);
        builder.Property(e => e.Status).HasConversion<int>().HasDefaultValue(UserStatus.Activated);

        builder
            .HasMany(x => x.UserRecoverPasswords)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .IsRequired();

        builder
            .HasMany(x => x.UserCreatePasswords)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .IsRequired();

        builder
            .HasOne(x => x.Athlete)
            .WithOne(x => x.User)
            .HasForeignKey<User>(x => x.AthleteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Email).IsUnique();
    }
}
