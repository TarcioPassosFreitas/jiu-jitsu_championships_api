using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsuMaster.Infrastructure.Database.Configurations;

internal class UserRecoverPasswordEntityConfiguration : BaseEntityConfiguration<UserRecoverPassword>
{
    public override void Configure(EntityTypeBuilder<UserRecoverPassword> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Expire).IsRequired();
        builder.Property(x => x.Token).HasMaxLength(255).IsRequired();
        builder.Property(x => x.UserId).IsRequired(false);
        builder.Property(x => x.AtleteId).IsRequired(false);
    }
}