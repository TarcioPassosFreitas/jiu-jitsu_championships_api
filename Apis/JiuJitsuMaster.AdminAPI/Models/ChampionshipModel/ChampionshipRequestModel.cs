using JiuJitsuMaster.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace JiuJitsuMaster.AdminAPI.Models.ChampionshipModel;

public class ChampionshipRequestModel
{
    [Required] public string Code { get; set; } = null!;
    [Required] public string Title { get; set; } = null!;
    [Required] public string City { get; set; } = null!;
    [Required] public string State { get; set; } = null!;
    [Required] public DateTime EventDate { get; set; }
    [Required] public string AboutEvent { get; set; } = null!;
    [Required] public string Gym { get; set; } = null!;
    [Required] public string GeneralInfo { get; set; } = null!;
    public string? PublicEntry { get; set; }
    [Required] public ChampionshipType Type { get; set; }
    [Required] public ChampionshipPhase Phase { get; set; }
    [Required] public ChampionshipStatus Status { get; set; }
    [Required] public IFormFile ImageFiles { get; set; } = null!;
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}