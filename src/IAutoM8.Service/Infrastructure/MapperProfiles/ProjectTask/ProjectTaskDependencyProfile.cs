using AutoMapper;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Service.ProjectTasks.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.ProjectTask
{
    internal class ProjectTaskDependencyProfile : Profile
    {
        public ProjectTaskDependencyProfile()
        {
            ToDto();
            FromDto();
        }

        private void ToDto()
        {
            CreateMap<ProjectTaskDependency, TaskDependencyDto>();
        }

        private void FromDto()
        {
            CreateMap<TaskDependencyDto, ProjectTaskDependency>()

                // Ignore
                .ForMember(dest => dest.ParentTask, src => src.Ignore())
                .ForMember(dest => dest.ChildTask, src => src.Ignore());
        }
    }
}
