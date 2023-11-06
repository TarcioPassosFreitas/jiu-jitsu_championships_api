using CommonUtility.Entity;

namespace JiuJitsuMaster.Core.Aggregates;

public class AthleteFight : BaseEntity
{
    public long AthleteId { get; set; }
    public long FightId { get; set; }
    public bool? Winner { get; set; }
    public bool IsFinished { get; set; }

    public Athlete Athlete { get; set; } = null!;
    public Fight Fight { get; set; } = null!;
}