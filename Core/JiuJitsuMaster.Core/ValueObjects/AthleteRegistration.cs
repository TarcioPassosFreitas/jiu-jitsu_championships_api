using CommonUtility.Model;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.ValueObjects;

public class AthleteRegistration : ValueObject
{
    public string AthleteName { get; private set; }
    public Gender AthleteGender { get; private set; }
    public string ChampionshipTitle { get; private set; }
    public string ChampionshipCity { get; private set; }
    public string ChampionshipState { get; private set; }

    public AthleteRegistration(
        string athleteName,
        Gender athleteGender,
        string championshipTitle,
        string championshipCity,
        string championshipState)
    {
        AthleteName = athleteName;
        AthleteGender = athleteGender;
        ChampionshipTitle = championshipTitle;
        ChampionshipCity = championshipCity;
        ChampionshipState = championshipState;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AthleteName;
        yield return AthleteGender;
        yield return ChampionshipTitle;
        yield return ChampionshipCity;
        yield return ChampionshipState;
    }
}