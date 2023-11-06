using CommonUtility.Entity;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.Aggregates;

public class User : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Password { get; set; }
    public string? RefreshToken { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public long? AthleteId { get; set; }

    public Athlete Athlete { get; set; } = null!;
    public List<UserRecoverPassword> UserRecoverPasswords { get; } = new();
    public List<UserCreatePassword> UserCreatePasswords { get; } = new();
}