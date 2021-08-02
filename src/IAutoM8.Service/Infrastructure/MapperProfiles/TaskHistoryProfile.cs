using AutoMapper;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Service.ProjectTasks.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles
{
    internal class TaskHistoryProfile : Profile
    {
        public TaskHistoryProfile()
        {
            ToDto();
        }

        private void ToDto()
        {
            CreateMap<TaskHistory, TaskHistoryItemDto>()
                .ForMember(dest => dest.ConditionOption,
                    opt => opt.MapFrom(x => x.ProjectTaskConditionOption.Option))
                .ForMember(dest => dest.TaskName,
                    opt => opt.MapFrom(x => x.Task.Title))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(x => x.Type))
                .ForMember(dest => dest.ProjectId,
                    opt => opt.MapFrom(x => x.Task.ProjectId))
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(x => x.Task.Project.Name))
                .ForMember(dest => dest.Condition,
                    opt => opt.MapFrom(x => x.ProjectTaskConditionOption.Condition.Condition));
        }
    }
}
