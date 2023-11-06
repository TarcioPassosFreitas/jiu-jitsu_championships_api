using JiuJitsuMaster.AdminAPI.Models.Athlete;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.AdminAPI.Models.AthleteChampionship;

public class AthleteChampionshipResponseModel
{
    public long AthleteId { get; set; }
    public long ChampionshipId { get; set; }

    public AthleteListResponseModel Athlete { get; set; } = null!;
    public Championship Championship { get; set; } = null!;
}