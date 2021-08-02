using AutoMapper;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Service.Projects.Dto;
using System.Linq;

namespace IAutoM8.Service.Infrastructure.MapperProfiles
{
    internal class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            ToDto();
            FromDto();
        }

        private void ToDto()
        {
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.Resources, opt => opt.MapFrom(src => src.ResourceProject.Select(s => s.Resource)))
                .ForMember(dest => dest.Managers,
                    opt => opt.MapFrom(src => src.UserProjects.Select(s => s.User).ToList()))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(dest => dest.Owner))
                .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client != null ? src.Client.CompanyName : null));

            CreateMap<Domain.Models.User.User, ProjectUserDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Profile.FullName));

            CreateMap<Domain.Models.User.UserProject, ProjectUserDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Profile.FullName))
                .ForSourceMember(src => src.ProjectId, opt => opt.Ignore());
        }

        private void FromDto()
        {
            CreateMap<AddProjectDto, Project>(MemberList.None);

            CreateMap<ProjectDto, Project>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ParentProjectId, opt => opt.MapFrom(src => src.ParentProjectId))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
