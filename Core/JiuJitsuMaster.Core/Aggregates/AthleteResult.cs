using CommonUtility.Entity;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.Aggregates;

public class AthleteResult : BaseEntity
{
    public long AthleteId { get; set; }
    public long ResultId { get; set; }
    public PlacementType Placement { get; set; }

    public Athlete Athlete { get; set; } = null!;
    public Results Result { get; set; } = null!;
}