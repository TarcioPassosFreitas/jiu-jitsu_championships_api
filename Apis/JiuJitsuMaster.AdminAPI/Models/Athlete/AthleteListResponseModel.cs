using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.AdminAPI.Models.Athlete;

public class AthleteListResponseModel
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
}