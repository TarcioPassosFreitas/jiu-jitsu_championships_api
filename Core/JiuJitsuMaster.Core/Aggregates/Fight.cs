using CommonUtility.Entity;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.Aggregates;

public class Fight : BaseEntity
{
    public long ChampionshipId { get; set; }
    public BeltType Belt { get; set; }
    public WeightType Weight { get; set; }
    public bool? IsInternalTeamFight { get; set; }
    public int Round { get; set; }

    public Championship Championship { get; set; } = null!;
    public List<AthleteFight> AthleteFights { get; set; } = new();
}