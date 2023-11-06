using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.AdminAPI.Models.ChampionshipModel;

public class ChampionshipResponseModel
{
     public string Code { get; set; } = null!;
     public string Title { get; set; } = null!;
     public string Image { get; set; } = null!;
     public string City { get; set; } = null!;
     public string State { get; set; } = null!;
     public DateTimeOffset EventDate { get; set; }
     public string AboutEvent { get; set; } = null!;
     public string Gym { get; set; } = null!;
     public string GeneralInfo { get; set; } = null!;
     public string? PublicEntry { get; set; }
     public ChampionshipType Type { get; set; }
     public ChampionshipPhase Phase { get; set; }
     public ChampionshipStatus Status { get; set; }
}