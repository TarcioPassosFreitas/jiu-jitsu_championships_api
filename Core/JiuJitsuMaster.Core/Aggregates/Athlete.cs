using CommonUtility.Entity;
using JiuJitsuMaster.Core.Enums;
namespace JiuJitsuMaster.Core.Aggregates;

public class Athlete : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTimeOffset BirthDate { get; set; }
    public string CPF { get; set; } = null!;
    public Gender Gender { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? RefreshToken { get; set; }
    public string Team { get; set; } = null!;
    public BeltType Belt { get; set; }
    public WeightType Weight { get; set; }
    public long? UserId { get; set; }

    public User User { get; set; } = null!;
    public List<AthleteFight> AthleteFights { get; set; } = new();
    public List<AthleteResult> AthleteResults { get; set; } = new();
    public List<Certificate> Certificates { get; set; } = new();
    public List<UserRecoverPassword> UserRecoverPasswords { get; } = new();
    public List<UserCreatePassword> UserCreatePasswords { get; } = new();
    public List<AthleteChampionship> AthleteChampionships { get; set; } = new();
}