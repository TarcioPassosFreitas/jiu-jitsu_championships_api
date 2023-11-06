using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.AdminAPI.Models.ChampionshipModel;

public class EnumsChampionshipRequestModel
{
    public ChampionshipType Type { get; set; }
    public ChampionshipPhase Phase { get; set; }
    public ChampionshipStatus Status { get; set; }
}