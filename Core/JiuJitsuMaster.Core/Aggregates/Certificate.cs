using CommonUtility.Entity;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.Aggregates;

public class Certificate : BaseEntity
{
    public CertificateType Type { get; set; }
    public long AthleteId { get; set; }
    public long ChampionshipId { get; set; }

    public Athlete Athlete { get; set; } = null!;
    public Championship Championship { get; set; } = null!;
}