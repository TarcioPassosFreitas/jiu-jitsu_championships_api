using CommonUtility.Entity;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.Aggregates;

public class Results : BaseEntity
{
    public long ChampionshipId { get; set; }
    public BeltType Belt { get; set; }
    public WeightType Weight { get; set; }

    public Championship Championship { get; set; } = null!;
    public List<AthleteResult> AthleteResults { get; set; } = new();
}