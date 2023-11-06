using AutoMapper;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.UserAPI.Models.Atletes;
using JiuJitsuMaster.UserAPI.Models.Championship;
using JiuJitsuMaster.UserAPI.Models.Fight;

namespace JiuJitsuMaster.UserAPI.Mappers;

public class DomainToModel : Profile
{
    public DomainToModel()
    {
        CreateMap<Athlete, AthleteRequestModel>().ReverseMap();

        CreateMap<Championship, ChampionshipResponseModel>().ReverseMap();

        CreateMap<Fight, FightResponseModel>().ReverseMap();
    }
}