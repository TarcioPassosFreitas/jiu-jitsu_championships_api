using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.UserAPI.Models.Fight;

public class FightResponseModel
{
    public long ChampionshipId { get; set; }
    public BeltType Belt { get; set; }
    public WeightType Weight { get; set; }
    public bool? IsInternalTeamFight { get; set; }
    public int Round { get; set; }
}