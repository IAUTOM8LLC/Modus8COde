using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Service.FormulaTasks.Dto;
using IAutoM8.Service.ProjectTasks.Dto;
using System;
using System.Linq;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.FormulaTask
{
    internal class FormulaTaskProfile : Profile
    {
        public FormulaTaskProfile()
        {
            ToDto();
            FromDto();
            
            CreateMap<Domain.Models.Formula.Task.FormulaTask, Domain.Models.Formula.Task.FormulaTask>(MemberList.None)
                .ForMember(dest => dest.AssignedConditionOptions, opts => opts.Ignore())
                .ForMember(dest => dest.AssignedSkill, opts => opts.Ignore())
                .ForMember(dest => dest.AssignedSkillId, opts => opts.Ignore())
                .ForMember(dest => dest.AssignedTeam, opts => opts.Ignore())
                .ForMember(dest => dest.AssignedTeamId, opts => opts.Ignore())
                .ForMember(dest => dest.ChildTasks, opts => opts.Ignore())
                .ForMember(dest => dest.Condition, opts => opts.Ignore())
                .ForMember(dest => dest.DateCreated, opts => opts.ResolveUsing(
                    (source, dest, member, context) =>
                    (DateTime)context.Items["DateCreated"]))
                .ForMember(dest => dest.FormulaProject, opts => opts.Ignore())
                .ForMember(dest => dest.FormulaProjectId, opts => opts.ResolveUsing(
                    (source, dest, member, context) =>
                    (int)context.Items["FormulaProjectId"]))
                .ForMember(dest => dest.Id, opts => opts.Ignore())
                .ForMember(dest => dest.InternalFormulaProject, opts => opts.Ignore())
                .ForMember(dest => dest.InternalFormulaProjectId, opts => opts.ResolveUsing(
                    (source, dest, member, context) =>
                    (int?)context.Items["InternalFormulaProjectId"]))
                .ForMember(dest => dest.IsShareResources, opts => opts.Ignore())
                .ForMember(dest => dest.LastUpdated, opts => opts.Ignore())
                .ForMember(dest => dest.Owner, opts => opts.Ignore())
                .ForMember(dest => dest.OwnerGuid, opts => opts.ResolveUsing(
                    (source, dest, member, context) =>
                    (Guid)context.Items["OwnerGuid"]))
                .ForMember(dest => dest.ParentTasks, opts => opts.Ignore())
                .ForMember(dest => dest.ResourceFormulaTask, opts => opts.Ignore())
                .ForMember(dest => dest.ReviewingSkill, opts => opts.Ignore())
                .ForMember(dest => dest.ReviewingSkillId, opts => opts.Ignore())
                .ForMember(dest => dest.ReviewingTeam, opts => opts.Ignore())
                .ForMember(dest => dest.ReviewingTeamId, opts => opts.Ignore())
                .ForMember(dest => dest.RecurrenceOptions, opts => opts.Ignore())
                .ForMember(dest => dest.RecurrenceOptionsId, opts => opts.Ignore())
                .ForMember(dest => dest.TaskConditionId, opts => opts.Ignore())
                .ForMember(dest => dest.DescNotificationFlag, opt => opt.Ignore());
            CreateMap<Domain.Models.Vendor.FormulaTaskVendor, Domain.Models.Vendor.FormulaTaskVendor>(MemberList.None)
                .ForMember(dest => dest.FormulaTaskId, opts => opts.Ignore())
                .ForMember(dest => dest.FormulaTask, opts => opts.Ignore())
                .ForMember(dest => dest.Id, opts => opts.Ignore());
        }

        private void FromDto()
        {
            CreateMap<UpdateFormulaTaskDto, Domain.Models.Formula.Task.FormulaTask>(MemberList.None)
                .ForMember(dest => dest.RecurrenceOptions,
                    opt => opt.MapFrom(src => src.IsRecurrent ? src.RecurrenceOptions : null))
                .ForMember(dest => dest.Condition,
                    src => src.MapFrom(dto => dto.IsConditional ? dto.Condition : null))
                .ForMember(dest => dest.IsInterval, opts => opts.MapFrom(src => src.IsInterval))
                .ForMember(dest => dest.IsTrainingLocked, opts => opts.MapFrom(src => src.IsTrainingLocked))
                .ForMember(dest => dest.TaskConditionId, opts => opts.Ignore())
                .ForMember(dest => dest.RecurrenceOptionsId, opts => opts.Ignore())
                .ForMember(dest => dest.AssignedConditionOptions, opts => opts.Ignore())
                .ForMember(dest => dest.ParentTasks, opts => opts.Ignore())
                .ForMember(dest => dest.ChildTasks, opts => opts.Ignore())
                .ForMember(dest => dest.DateCreated, opts => opts.Ignore())
                .ForMember(dest => dest.LastUpdated, opts => opts.Ignore())
                .ForMember(dest => dest.AssignedFormulaTeam, opts => opts.Ignore())
                .ForSourceMember(src => src.AddTodoCheckList, opts => opts.Ignore())                
                .ForSourceMember(src => src.AddReviewerCheckList, opts => opts.Ignore());

            CreateMap<FormulaTaskDto, Domain.Models.Formula.Task.FormulaTask>(MemberList.None)
                .IncludeBase<UpdateFormulaTaskDto, Domain.Models.Formula.Task.FormulaTask>()
                .ForSourceMember(src => src.AddTodoCheckList, opts => opts.Ignore())
                .ForSourceMember(src => src.AddReviewerCheckList, opts => opts.Ignore());

            CreateMap<AddFormulaTaskDto, Domain.Models.Formula.Task.FormulaTask>(MemberList.None);

            CreateMap<AddFormulaNotesDto, FormulaNote>(MemberList.None)
                .ForMember(dest => dest.DateCreated, opts => opts.Ignore());

            CreateMap<AddFormulaTaskDisableStatusDto, FormulaTaskDisableStatus>(MemberList.None)
                .ForMember(dest => dest.DateCreated, opts => opts.Ignore())
                .ForMember(dest => dest.LastUpdated, opts => opts.Ignore())
                .ForMember(dest => dest.IsDisabled, opts => opts.Ignore());
        }

        private void ToDto()
        {
            CreateMap<Domain.Models.Formula.Task.FormulaTask, UpdateFormulaTaskDto>()
                .ForMember(dest => dest.IsConditional, opt => opt.MapFrom(src => src.TaskConditionId.HasValue))
                .ForMember(dest => dest.IsRecurrent, opt => opt.MapFrom(src => src.RecurrenceOptionsId.HasValue))
                .ForMember(dest => dest.IsInterval, opt => opt.MapFrom(src => src.IsInterval))
                .ForMember(dest => dest.IsTrainingLocked, opt => opt.MapFrom(src => src.IsTrainingLocked))
                .ForMember(dest => dest.AddTodoCheckList, opt => opt.Ignore())
                .ForMember(dest => dest.AddReviewerCheckList, opt => opt.Ignore());

            CreateMap<Domain.Models.Formula.Task.FormulaTask, FormulaTaskDto>()
                .IncludeBase<Domain.Models.Formula.Task.FormulaTask, UpdateFormulaTaskDto>()
                .ForMember(dest => dest.Resources, opt => opt.Ignore())
                .ForMember(dest => dest.AddTodoCheckList, opt => opt.Ignore())
                .ForMember(dest => dest.AddReviewerCheckList, opt => opt.Ignore())
                .ForMember(dest => dest.FormulaTaskChecklists, opt => opt.Ignore())
                .ForMember(dest => dest.ShowTrainingTab, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTasks,
                    opt => opt.MapFrom(src => src.ParentTasks.Select(x => x.ParentTaskId)))
                .ForMember(dest => dest.ChildTasks,
                    opt => opt.MapFrom(src => src.ChildTasks.Select(x => x.ChildTaskId)))
                .ForMember(dest => dest.ParentTaskId, opts => opts.Ignore());

            CreateMap<Domain.Models.Formula.Task.FormulaTaskChecklist, FormulaTaskChecklistDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForSourceMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForSourceMember(dest => dest.LastUpdated, opt => opt.Ignore());

            CreateMap<FormulaNote, FormulaNotesDto>();
        }
    }
}
