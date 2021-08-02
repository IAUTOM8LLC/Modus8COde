using AutoMapper;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Notification.Models;
using IAutoM8.Service.ProjectTasks.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.ProjectTask
{
    internal class ProjectTaskProfile : Profile
    {
        public ProjectTaskProfile()
        {
            ToDto();
            FromDto();
        }

        private void ToDto()
        {
            CreateMap<Domain.Models.Project.Task.ProjectTask, UpdateTaskDto>()
                .ForMember(dest => dest.IsConditional,
                    opt => opt.MapFrom(src => src.TaskConditionId.HasValue))
                .ForMember(dest => dest.IsRecurrent,
                    opt => opt.MapFrom(src => src.RecurrenceOptionsId.HasValue))
                .ForMember(dest => dest.IsInterval, opt => opt.MapFrom(src => src.IsInterval))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => src.RecurrenceOptionsId.HasValue
                        ? src.RecurrenceOptions.NextOccurenceDate.Value
                        : src.StartDate))
                .ForMember(dest => dest.DueDate,
                    opt => opt.MapFrom(src => src.RecurrenceOptionsId.HasValue
                        ? src.RecurrenceOptions.NextOccurenceDate.Value.AddMinutes(src.Duration ?? 0)
                        : (src.StartDate.HasValue
                            ? src.StartDate.Value.AddMinutes(src.Duration ?? 0)
                            : (DateTime?)null)))

                .ForMember(dest => dest.AssignedUserIds, opt => opt.MapFrom(src =>
                    src.ProjectTaskUsers.Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Assigned)
                        .Select(t => t.UserId)))
                .ForMember(dest => dest.ReviewingUserIds, opt => opt.MapFrom(src =>
                    src.ProjectTaskUsers.Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing)
                        .Select(t => t.UserId)))

                .ForMember(dest => dest.FormulaName, opt => opt.MapFrom(src =>
                    src.FormulaTaskId.HasValue ? src.FormulaTask.FormulaProject.Name : null))

                // Ignore
                .ForMember(dest => dest.UseDateRangeAsDuration, opt => opt.Ignore())
                .ForMember(dest => dest.AddTodoCheckList, opt => opt.Ignore())
                .ForMember(dest => dest.EditCheckLists, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCheckLists, opt => opt.Ignore())
                .ForMember(dest => dest.AddReviewerCheckList, opt => opt.Ignore());

            CreateMap<List<Domain.Models.Project.Task.ProjectTask>, DailyToDoSummary>()
                .ForMember(dest => dest.UpcomingTasks,
                    opt => opt.MapFrom(src => src.Where(t => t.Status == TaskStatusType.New)
                    .Select(t => new TaskSummaryDetail { Title = t.Title, Time = t.StartDate.Value })))
                .ForMember(dest => dest.ToDoTasks,
                    opt => opt.MapFrom(src => src.Where(t => t.Status == TaskStatusType.InProgress)
                    .Select(t => new TaskSummaryDetail { Title = t.Title, Time = t.StartDate.Value })))
                .ForMember(dest => dest.NeedsReviewTasks,
                    opt => opt.MapFrom(src => src.Where(t => t.Status == TaskStatusType.NeedsReview)
                    .Select(t => new TaskSummaryDetail { Title = t.Title, Time = t.StartDate.Value })));


            CreateMap<Domain.Models.Project.Task.ProjectTask, TaskDto>()
                .IncludeBase<Domain.Models.Project.Task.ProjectTask, UpdateTaskDto>()
                .ForMember(dest => dest.ParentTasks,
                    opt => opt.MapFrom(src => src.ParentTasks.Select(x => x.ParentTaskId)))
                .ForMember(dest => dest.ChildTasks,
                    opt => opt.MapFrom(src => src.ChildTasks.Select(x => x.ChildTaskId)))
                .ForMember(dest => dest.ConditionalParentTasks,
                    opt => opt.MapFrom(x => x.AssignedConditionOptions.Select(co => co.Condition.Task.Id)))
                .ForMember(dest => dest.Outsources,
                    opt => opt.MapFrom(x => x.ProjectTaskVendors))
                .ForMember(dest => dest.Resources, opt => opt.Ignore())
                .ForMember(dest => dest.CanBeProccessed, opt => opt.Ignore())
                .ForMember(dest => dest.CanBeReviewed, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewingUserId, opt => opt.MapFrom(src => src.ReviewingUserGuid))
                .ForMember(dest => dest.CompletedDate,
                    opt => opt.MapFrom(src => (src.TaskHistories.LastOrDefault(t => t.Type == ActivityType.Completed || t.Type == ActivityType.AcceptReview) != null ?
                        src.TaskHistories.LastOrDefault(t => t.Type == ActivityType.Completed || t.Type == ActivityType.AcceptReview).HistoryTime : (DateTime?)null)))
                .ForMember(dest => dest.ProccessingUserId, opt => opt.MapFrom(src => src.ProccessingUserGuid))
                .ForMember(dest => dest.ProccessingUserName, opt => opt.MapFrom(src => src.ProccessingUser.Profile.FullName))
                .ForMember(dest => dest.ReviewingUserName, opt => opt.MapFrom(src => src.ReviewingUser.Profile.FullName))
                .ForMember(dest => dest.AssignedUserIds, opt => opt.MapFrom(src =>
                    src.ProjectTaskUsers.Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Assigned)
                        .Select(t => t.UserId)))
                .ForMember(dest => dest.ReviewingUserIds, opt => opt.MapFrom(src =>
                    src.ProjectTaskUsers.Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing)
                        .Select(t => t.UserId)))
                .ForMember(dest => dest.HasVendor, opt => opt.MapFrom(src => src.ProjectTaskVendors.Any()))
                .ForMember(dest => dest.HasAcceptedVendor, opt => opt.MapFrom(src => src.ProjectTaskVendors.Any(t => t.Status == ProjectRequestStatus.Accepted)))
                .ForMember(dest => dest.HasAssignedVendor, opt => opt.MapFrom(src => src.ProjectTaskVendors.Any(s => s.Status != ProjectRequestStatus.None)))
                .ForMember(dest => dest.IsDisabled, opt => opt.MapFrom(src => src.IsDisabled.HasValue ? src.IsDisabled.Value : false))
                // Added here, to be removed
                .ForMember(dest => dest.ReviewRating, opts => opts.Ignore())
                .ForMember(dest => dest.AddTodoCheckList, opt => opt.Ignore())
                .ForMember(dest => dest.EditCheckLists, opts => opts.Ignore())
                .ForMember(dest => dest.TaskCheckLists, opts => opts.Ignore())
                .ForMember(dest => dest.ShowTrainingTab, opts => opts.Ignore())
                .ForMember(dest => dest.AddReviewerCheckList, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore());



            CreateMap<Domain.Models.Vendor.ProjectTaskVendor, ProjectTaskOutsourceDto>()
                .ForMember(dest => dest.Date,
                    opts => opts.MapFrom(src => (src.Status == ProjectRequestStatus.Send
                        || src.Status == ProjectRequestStatus.Accepted
                        || src.Status == ProjectRequestStatus.Declined
                        || src.Status == ProjectRequestStatus.CancelAfterNudge
                        || src.Status == ProjectRequestStatus.Lost) ? src.LastModified : null))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.VendorGuid))
                .ForMember(dest => dest.FullName, opts => opts.MapFrom(src => src.Vendor.Profile.FullName))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.AvgRating, opts => opts.Ignore())
                .ForMember(dest => dest.AvgWorking, opts => opts.Ignore())
                .ForMember(dest => dest.AvgResponding, opts => opts.Ignore())
                // Added here, to be removed
                .ForMember(dest => dest.AvgMessaging, opts => opts.Ignore());

            CreateMap<Domain.Models.Project.Task.ProjectTask, ListViewTaskDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
                .ForMember(dest => dest.IsConditional,
                    opt => opt.MapFrom(src => src.TaskConditionId.HasValue))
                .ForMember(dest => dest.HasVendor, opt => opt.MapFrom(src => src.ProjectTaskVendors.Any()))
                .ForMember(dest => dest.HasAcceptedVendor, opt => opt.MapFrom(src => src.ProjectTaskVendors.Any(t => t.Status == ProjectRequestStatus.Accepted)))
                .ForMember(dest => dest.ProccessingUserName, opt => opt.MapFrom(src => src.ProccessingUser.Profile.FullName))
                .ForMember(dest => dest.ReviewingUserName, opt => opt.MapFrom(src => src.ReviewingUser.Profile.FullName))
                .ForMember(dest => dest.AssignedUserIds, opt => opt.MapFrom(src =>
                    src.ProjectTaskUsers.Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Assigned)
                        .Select(t => t.UserId)))
                .ForMember(dest => dest.ReviewingUserIds, opt => opt.MapFrom(src =>
                    src.ProjectTaskUsers.Where(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing)
                        .Select(t => t.UserId)))
                .ForMember(dest => dest.IsRecurrent,
                    opt => opt.MapFrom(src => src.RecurrenceOptionsId.HasValue))
                .ForMember(dest => dest.DueDate,
                    opt => opt.MapFrom(src => src.RecurrenceOptionsId.HasValue
                        ? src.RecurrenceOptions.NextOccurenceDate.Value.AddMinutes(src.Duration ?? 0)
                        : (src.StartDate.HasValue
                            ? src.StartDate.Value.AddMinutes(src.Duration ?? 0)
                            : (DateTime?)null)))
                .ForMember(dest => dest.FormulaName, opt => opt.MapFrom(src => src.FormulaTask.FormulaProject.Name))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.AssignedProjectTeam.Name))
                .ForMember(dest => dest.SkillName, opt => opt.MapFrom(src => src.AssignedSkill.Name))
                .ForMember(dest => dest.CompletionTime, opt => opt.Ignore())
                .ForMember(dest => dest.TurnAroundTime, opt => opt.Ignore())
                .ForMember(dest => dest.WorkType, opt => opt.Ignore())
                .ForMember(dest => dest.IsVendor, opt => opt.Ignore())
                .ForMember(dest => dest.IsCancel, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.ProccessingUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ProccessingUserRole, opt => opt.Ignore())
            //.ForMember(dest => dest.ETA, opt => opt.Ignore())
            //.ForMember(dest => dest.IS_PAST80_ETA_10, opt => opt.Ignore())


            //.ForMember(dest => dest.ETA_10, opt => opt.Ignore())
            //.ForMember(dest => dest.target_date_10, opt => opt.Ignore())
            //.ForMember(dest => dest.ETA_10_REMAINING_inrealtime, opt => opt.Ignore())
            //.ForMember(dest => dest.ETA_10_REMAINING_SECONDS, opt => opt.Ignore());


            .ForMember(dest => dest.DEADLINE, opt => opt.Ignore())
            .ForMember(dest => dest.TARGET_DATE, opt => opt.Ignore())
            .ForMember(dest => dest.IS_PAST80_DEADLINE, opt => opt.Ignore())
            .ForMember(dest => dest.DEADLINE_REMAINING_SECONDS, opt => opt.Ignore())
            .ForMember(dest => dest.NUDGECOUNT, opt => opt.Ignore());
        }

        private void FromDto()
        {
            CreateMap<UpdateTaskDto, Domain.Models.Project.Task.ProjectTask>(MemberList.None)
                .ForMember(dest => dest.StartDate, opt => opt.Condition(src => src.Status == "New"))
                .ForMember(dest => dest.Duration, opt => opt.ResolveUsing((dto, task) =>
                {
                    if (dto.Status != "New")
                        return task.Duration;

                    if (!dto.UseDateRangeAsDuration || dto.IsRecurrent || dto.IsInterval)
                        return dto.Duration;

                    if (!dto.IsInterval && dto.DueDate.HasValue && dto.StartDate.HasValue && dto.DueDate > dto.StartDate)
                        return (int)Math.Round(dto.DueDate.Value.Subtract(dto.StartDate.Value).TotalMinutes);

                    return 1;
                }))
                .ForMember(dest => dest.DueDate, opt => opt.ResolveUsing(dto => dto.UseDateRangeAsDuration && !dto.IsInterval ? dto.DueDate : null))
                .ForMember(dest => dest.RecurrenceOptions,
                    opt => opt.ResolveUsing((source, destination) =>
                    {
                        if (source.Status != "New")
                            return destination.RecurrenceOptions;

                        if (!source.IsRecurrent)
                            return null;

                        if (destination.RecurrenceOptions == null)
                            return Mapper.Map<RecurrenceOptions>(source.RecurrenceOptions);

                        Mapper.Map(source.RecurrenceOptions, destination.RecurrenceOptions);
                        return destination.RecurrenceOptions;
                    }))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.IsConditional ? src.Condition : null))
                .ForMember(dest => dest.IsInterval, opt => opt.MapFrom(src => src.IsInterval))
                .ForMember(dest => dest.ParentTasks, opt => opt.Ignore())
                .ForMember(dest => dest.ChildTasks, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedConditionOptions, opts => opts.Ignore())
                .ForMember(dest => dest.DateCreated, opts => opts.Ignore())
                .ForMember(dest => dest.LastUpdated, opts => opts.Ignore())
                .ForMember(dest => dest.NotificationSettings, opts => opts.Ignore())
                .ForMember(dest => dest.FormulaTaskId, opts => opts.Ignore())
                .ForMember(dest => dest.AssignedProjectTeam, opts => opts.Ignore());

            CreateMap<TaskDto, Domain.Models.Project.Task.ProjectTask>(MemberList.None)
                .IncludeBase<UpdateTaskDto, Domain.Models.Project.Task.ProjectTask>()
                .ForMember(dest => dest.IsDisabled, opts => opts.Ignore());

            CreateMap<Domain.Models.Formula.Task.FormulaTask, Domain.Models.Project.Task.ProjectTask>()
                .PreserveReferences()
                // Added here tobe removed
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTeamId, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewingTeamId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RecurrenceOptionsId, opt => opt.Ignore())
                .ForMember(dest => dest.TaskConditionId, opt => opt.Ignore())
                .ForMember(dest => dest.Condition, opt => opt.Ignore())
                .ForMember(dest => dest.ChildTasks, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTasks, opt => opt.Ignore())
                //.ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.TreeDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.ProccessingUserGuid, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewingUserGuid, opt => opt.Ignore())
                .ForMember(dest => dest.FormulaId, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.ProccessingUser, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewingUser, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.CreditLogs, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
                .ForMember(dest => dest.TaskJobs, opt => opt.Ignore())
                .ForMember(dest => dest.ResourceProjectTask, opt => opt.Ignore())
                .ForMember(dest => dest.NotificationSettings, opt => opt.Ignore())
                .ForMember(dest => dest.FormulaProjectTask, opt => opt.Ignore())
                .ForMember(dest => dest.FormulaProject, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTaskVendors, opt => opt.Ignore())
                .ForMember(dest => dest.FormulaTask, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedProjectTeam, opt => opt.Ignore())
                .ForMember(dest => dest.FormulaTaskId, opt => opt.MapFrom(src => src.OriginalFormulaTaskId ?? src.Id))
                .ForMember(dest => dest.DateCreated, opt => opt
                    .ResolveUsing((source, dest, member, context) => (DateTime)context.Items["NowUtc"]))
                .ForMember(dest => dest.DueDate, opt => opt
                    .ResolveUsing((source, dest, member, context) => (DateTime)context.Items["NowUtc"]))
                .ForMember(dest => dest.ParentTaskId, opt => opt
                    .ResolveUsing((source, dest, member, context) => (int?)context.Items["ParentTaskId"]))
                .ForMember(dest => dest.OwnerGuid, opt => opt
                    .ResolveUsing((source, dest, member, context) => (Guid)context.Items["OwnerGuid"]))
                .ForMember(dest => dest.ProjectTaskUsers, opt => opt
                    .ResolveUsing((source, dest, member, context) =>
                    {
                        var skillMaps = ((IEnumerable<SkillMapDto>)context.Items["SkillMappings"]);

                        var projectTaskUsers = skillMaps.FirstOrDefault(t => t.SkillId == source.AssignedSkillId && t.FormulaTaskId == source.Id)
                            ?.UserIds.Select(t => new ProjectTaskUser
                            {
                                ProjectTaskUserType = ProjectTaskUserType.Assigned,
                                UserId = t
                            }).ToList();
                        projectTaskUsers?.AddRange(
                            skillMaps.FirstOrDefault(t =>
                                source.ReviewingSkillId != null && t.ReviewingSkillId == source.ReviewingSkillId.Value && t.FormulaTaskId == source.Id)
                            ?.ReviewingUserIds.Select(t => new ProjectTaskUser
                            {
                                ProjectTaskUserType = ProjectTaskUserType.Reviewing,
                                UserId = t
                            }) ?? new List<ProjectTaskUser>());

                        return projectTaskUsers ?? new List<ProjectTaskUser>();
                    }))
                .ForMember(dest => dest.PosX, opt => opt
                    .ResolveUsing((source, dest, member, context) =>
                        source.PosX + ((ValueTuple<int, int>)context.Items["PositionOffset"]).Item1))
                .ForMember(dest => dest.PosY, opt => opt
                    .ResolveUsing((source, dest, member, context) =>
                        source.PosY + ((ValueTuple<int, int>)context.Items["PositionOffset"]).Item2))
                .ForMember(dest => dest.TaskHistories, opt => opt
                    .ResolveUsing((source, dest, member, context) => new List<TaskHistory>
                        {
                            new TaskHistory
                            {
                                HistoryTime = (DateTime)context.Items["NowUtc"],
                                Type = ActivityType.New
                            }
                        })
                    )
                .ForMember(dest => dest.IsDisabled, opt => opt
                    .ResolveUsing((source, dest, member, context) =>
                    {
                        var skillMaps = ((IEnumerable<SkillMapDto>)context.Items["SkillMappings"]);
                        var isDisabled = skillMaps.FirstOrDefault(t => t.FormulaTaskId == source.Id)?.IsDisabled ?? false;
                        return isDisabled;
                    }))
                .ForMember(dest => dest.Status, opt => opt
                    .ResolveUsing((source, dest, member, context) =>
                    {
                        var skillMaps = ((IEnumerable<SkillMapDto>)context.Items["SkillMappings"]);
                        var isDisabled = skillMaps.FirstOrDefault(t => t.FormulaTaskId == source.Id)?.IsDisabled ?? false;
                        return isDisabled ? TaskStatusType.DisabledFromFormula : TaskStatusType.New;
                    }))
                .ForMember(dest => dest.DescNotificationFlag, opt => opt.Ignore());
            var now = new DateTime();
            CreateMap<FormulaTaskVendor, ProjectTaskVendor>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => now))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LastModified, opt => opt.MapFrom(src => now))
                .ForMember(dest => dest.ProjectTaskId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTask, opt => opt.Ignore())
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProjectRequestStatus.None));

            CreateMap<AddProjectNotesDto, Domain.Models.Project.ProjectNote>(MemberList.None)
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.IsPublished, opt => opt.Ignore());

            CreateMap<UpdateProjectNotesDto, Domain.Models.Project.ProjectNote>(MemberList.None)
                .IncludeBase<AddProjectNotesDto, Domain.Models.Project.ProjectNote>()
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore());
        }
    }
}
