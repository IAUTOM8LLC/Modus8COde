using AutoMapper;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Teams.Dto;
using System.Linq;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.Team
{
    internal class TeamProfile : Profile
    {
        public TeamProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<UpdateTeamDetailDto, Domain.Models.Team.Team>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TeamName))
            .ForMember(dest => dest.IsGlobal, opt => opt.MapFrom(src => src.IsGlobal))
            .ForMember(dest => dest.OwnerGuid, opt => opt.Ignore())
            .ForMember(dest => dest.TeamSkills, opt => opt.Ignore())
            .ForMember(dest => dest.TeamUsers, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewingTasks, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTasks, opt => opt.Ignore())
            .ForAllOtherMembers(opt => opt.Ignore());

        }

        private void ToDto()
        {
            CreateMap<Domain.Models.Team.Team, TeamSkillDto>();

            CreateMap<Domain.Models.Team.Team, TeamDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Name))
            .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<Domain.Models.Team.Team, UpdateTeamDetailDto>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TeamSkills, opt => opt.MapFrom(src => src.TeamSkills.Select(s => s.SkillId)))
                .ForSourceMember(src=>src.Owner,opt=>opt.Ignore())
                .ForSourceMember(src=>src.TeamUsers, opt=>opt.Ignore())
                .ForSourceMember(src=>src.AssignedTasks, opt=>opt.Ignore())
                .ForSourceMember(src=>src.ReviewingTasks, opt=>opt.Ignore())
                .ForSourceMember(src=>src.DateCreated, opt=>opt.Ignore())
                .ForSourceMember(src=>src.LastUpdated, opt=>opt.Ignore())
                .ForSourceMember(src=>src.OwnerGuid, opt=>opt.Ignore())
                ;
            //.ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Id))
            //.ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Name))
            //.ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
