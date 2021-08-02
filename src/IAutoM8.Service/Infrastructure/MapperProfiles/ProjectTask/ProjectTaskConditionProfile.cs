using AutoMapper;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Service.ProjectTasks.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.ProjectTask
{
    internal class ProjectTaskConditionProfile : Profile
    {
        public ProjectTaskConditionProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<ProjectTaskCondition, TaskConditionDto>();
        }

        private void ToDto()
        {
            CreateMap<TaskConditionDto, ProjectTaskCondition>()

                // Ignore
                .ForMember(dest => dest.Id, opts => opts.Ignore())
                .ForMember(dest => dest.Task, opts => opts.Ignore());
        }
    }
}
