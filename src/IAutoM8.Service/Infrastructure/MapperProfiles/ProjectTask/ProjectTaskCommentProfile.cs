using AutoMapper;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Service.ProjectTasks.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.ProjectTask
{
    internal class ProjectTaskCommentProfile : Profile
    {
        public ProjectTaskCommentProfile()
        {
            ToDto();
            FromDto();
        }

        private void ToDto()
        {
            CreateMap<ProjectTaskComment, CommentDto>()
                .ForMember(dest => dest.PostedTime, opt => opt.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User == null ? "" : src.User.Profile.FullName));
        }

        private void FromDto()
        {
            CreateMap<AddCommentDto, ProjectTaskComment>(MemberList.None)
                .ForMember(dest => dest.ProjectTaskId, opt => opt.MapFrom(src => src.TaskId));
        }
    }
}
