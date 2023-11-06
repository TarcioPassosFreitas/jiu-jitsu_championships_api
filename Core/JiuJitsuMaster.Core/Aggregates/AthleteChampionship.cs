using CommonUtility.Entity;

namespace JiuJitsuMaster.Core.Aggregates;

public class AthleteChampionship : BaseEntity
{
    public long AthleteId { get; set; }
    public long ChampionshipId { get; set; }

    public Athlete Athlete { get; set; } = null!;
    public Championship Championship { get; set; } = null!;
}