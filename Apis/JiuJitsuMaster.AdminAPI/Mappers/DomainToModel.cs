using AutoMapper;
using JiuJitsuMaster.AdminAPI.Models.Athlete;
using JiuJitsuMaster.AdminAPI.Models.ChampionshipModel;
using JiuJitsuMaster.AdminAPI.Models.UserModel;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.AdminAPI.Mappers;

public class DomainToModel : Profile
{
    public DomainToModel()
    {
        CreateMap<User, RegisterRequestModel>().ReverseMap();

        CreateMap<User, UserUpdateRequestModel>().ReverseMap();

        CreateMap<User, UserResponseModel>().ReverseMap();

        CreateMap<Championship, ChampionshipRequestModel>().ReverseMap();

        CreateMap<Championship, ChampionshipResponseModel>().ReverseMap();

        CreateMap<Championship, ChampionshipUpdateRequestModel>().ReverseMap();

        CreateMap<Athlete, AthleteListResponseModel>().ReverseMap();
    }
}