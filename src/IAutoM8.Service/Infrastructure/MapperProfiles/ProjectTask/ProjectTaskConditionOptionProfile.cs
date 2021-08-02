using AutoMapper;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Service.ProjectTasks.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.ProjectTask
{
    internal class ProjectTaskConditionOptionProfile : Profile
    {
        public ProjectTaskConditionOptionProfile()
        {
            ToDto();
            FromDto();
        }

        private void ToDto()
        {
            CreateMap<ProjectTaskConditionOption, TaskConditionOptionDto>()
                .ForMember(dest => dest.AssignedTaskId,
                    opt => opt.MapFrom(src => src.AssignedTaskId ?? 0))
                .ForMember(dest => dest.ConditionalTaskId,
                    opt => opt.MapFrom(x => x.Condition.Task.Id));
        }

        private void FromDto()
        {
            CreateMap<TaskConditionOptionDto, ProjectTaskConditionOption>(MemberList.None)
                .ForMember(dest => dest.AssignedTaskId,
                    opt => opt.MapFrom(src => src.AssignedTaskId == 0 ? (int?) null : src.AssignedTaskId))
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }

    }
}
