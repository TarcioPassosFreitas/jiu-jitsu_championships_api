using CommonUtility.Entity;

namespace JiuJitsuMaster.Core.Aggregates;

public class UserCreatePassword : BaseEntity
{
    public string Token { get; set; } = null!;
    public DateTimeOffset Expire { get; set; }
    public long UserId { get; set; }
    public long AtleteId { get; set; }

    public User User { get; set; } = null!;
    public Athlete Athlete { get; set; } = null!;
}