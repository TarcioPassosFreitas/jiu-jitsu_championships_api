using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.UserAPI.Models.Atletes;

public class AthleteRequestModel
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTimeOffset BirthDate { get; set; }
    public string CPF { get; set; } = null!;
    public Gender Gender { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Team { get; set; } = null!;
    public BeltType Belt { get; set; }
    public WeightType Weight { get; set; }
}