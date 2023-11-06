using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.AdminAPI.Models.ChampionshipModel;

public class ChampionshipUpdateRequestModel
{
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public DateTime? EventDate { get; set; }
    public string? AboutEvent { get; set; }
    public string? Gym { get; set; }
    public string? GeneralInfo { get; set; }
    public string? PublicEntry { get; set; }
    public ChampionshipType? Type { get; set; }
    public ChampionshipPhase? Phase { get; set; }
    public ChampionshipStatus? Status { get; set; }
    public IFormFile? ImageFiles { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}