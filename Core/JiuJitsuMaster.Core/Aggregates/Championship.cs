using CommonUtility.Entity;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.Aggregates;

public class Championship : BaseEntity
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
    public string PublicEntry { get; set; } = null!;
    public ChampionshipType Type { get; set; }
    public ChampionshipPhase Phase { get; set; }
    public ChampionshipStatus Status { get; set; }
    public bool? Highlights { get; set; }
    public int? HighlightOrder { get; set; }

    public List<Fight> Fights { get; set; } = new();
    public List<Results> Results { get; set; } = new();
    public List<Certificate> Certificates { get; set; } = new();
    public List<AthleteChampionship> AthleteChampionships { get; set; } = new();
}