using AutoMapper;
using IAutoM8.Service.FormulaTasks.Dto;
using System.Collections.Generic;
using System.Data;
using System;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Teams.Dto;
using IAutoM8.Service.Skills.Dto;
using Microsoft.CodeAnalysis;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.Vendor.Dto;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Projects.Dto;
using IAutoM8.Service.Company.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles
{
    internal class SQLToDtoProfile : Profile
    {
        public SQLToDtoProfile()
        {

            CreateMap<IDataReader, IEnumerable<FormulaTaskOutsourceDto>>()
                .ConstructUsing(BuildList<FormulaTaskOutsourceDto>);
            CreateMap<IDataRecord, FormulaTaskOutsourceDto>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (Guid)src["Id"]))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src["FullName"] == DBNull.Value ? null : (string)src["FullName"]))
                .ForMember(dest => dest.Date,
                    opt => opt.MapFrom(src =>
                        src["Date"] == DBNull.Value ? null : (DateTime?)src["Date"]))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src =>
                        src["Status"] == DBNull.Value ? null : ((FormulaRequestStatus?)(byte)src["Status"]).ToString()))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src["Price"] == DBNull.Value ? null : ((decimal?)src["Price"])))
                .ForMember(dest => dest.AvgResponding,
                    opt => opt.MapFrom(src =>
                        src["AvgResponding"] == DBNull.Value ? null : (double?)src["AvgResponding"]))
                .ForMember(dest => dest.AvgRating,
                    opt => opt.MapFrom(src =>
                        src["AvgRating"] == DBNull.Value ? null : (double?)src["AvgRating"]))
                .ForMember(dest => dest.AvgWorking,
                    opt => opt.MapFrom(src =>
                        src["AvgWorking"] == DBNull.Value ? null : (double?)src["AvgWorking"]))
                .ForMember(dest => dest.AvgMessaging,
                    opt => opt.MapFrom(src =>
                        src["AvgMessaging"] == DBNull.Value ? null : (double?)src["AvgMessaging"]))
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src =>
                        src["Role"] == DBNull.Value ? null : (string)src["Role"]))
                .ForMember(dest => dest.OwnerId,
                    opt => opt.MapFrom(src =>
                        src["OwnerId"] == DBNull.Value ? null : ((Guid)src["OwnerId"]).ToString())); ;


            CreateMap<IDataReader, IEnumerable<TeamDto>>()
                .ConstructUsing(BuildList<TeamDto>);

            CreateMap<IDataRecord, TeamDto>(MemberList.None)
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src["Id"] == DBNull.Value ? null : (int?)src["Id"]))
                .ForMember(dest => dest.TeamName,
                    opt => opt.MapFrom(src => src["TeamName"] == DBNull.Value ? null : (string)src["TeamName"]))
                .ForMember(dest => dest.TeamIsGlobal,
                    opt => opt.MapFrom(src => src["TeamIsGlobal"] == DBNull.Value ? null : (bool?)src["TeamIsGlobal"]))
                .ForMember(dest => dest.TeamStatus,
                    opt => opt.MapFrom(src => src["TeamStatus"] == DBNull.Value ? null : (int?)src["TeamStatus"]))
                .ForMember(dest => dest.SkillId,
                    opt => opt.MapFrom(src => src["SkillId"] == DBNull.Value ? null : (int?)src["SkillId"]))
                .ForMember(dest => dest.SkillName,
                    opt => opt.MapFrom(src => src["SkillName"] == DBNull.Value ? null : (string)src["SkillName"]))
                .ForMember(dest => dest.SkillIsGlobal,
                    opt => opt.MapFrom(src => src["SkillIsGlobal"] == DBNull.Value ? null : (bool?)src["SkillIsGlobal"]))
                .ForMember(dest => dest.SkillStatus,
                    opt => opt.MapFrom(src => src["SkillStatus"] == DBNull.Value ? null : (int?)src["SkillStatus"]))
                .ForMember(dest => dest.OutsourcerName,
                    opt => opt.MapFrom(src => src["OutsourcerName"] == DBNull.Value ? null : (string)src["OutsourcerName"]))
                .ForMember(dest => dest.FormulaID,
                    opt => opt.MapFrom(src => src["FormulaID"] == DBNull.Value ? null : (int?)src["FormulaID"]))
                .ForMember(dest => dest.FormulaName,
                    opt => opt.MapFrom(src => src["FormulaName"] == DBNull.Value ? null : (string)src["FormulaName"]))
                .ForMember(dest => dest.FormulaIsGlobal,
                    opt => opt.MapFrom(src => src["FormulaIsGlobal"] == DBNull.Value ? null : (bool?)src["FormulaIsGlobal"]))
                .ForMember(dest => dest.FormulaCreatedDate,
                    opt => opt.MapFrom(src => src["FormulaCreatedDate"] == DBNull.Value ? null : (DateTime?)src["FormulaCreatedDate"]))
                .ForMember(dest => dest.FormulaUpdatedDate,
                    opt => opt.MapFrom(src => src["FormulaUpdatedDate"] == DBNull.Value ? null : (DateTime?)src["FormulaUpdatedDate"]))
                .ForMember(dest => dest.FormulaStatus,
                    opt => opt.MapFrom(src => src["FormulaStatus"] == DBNull.Value ? null : (int?)src["FormulaStatus"]))
                .ForMember(dest => dest.TaskID,
                    opt => opt.MapFrom(src => src["TaskID"] == DBNull.Value ? null : (int?)src["TaskID"]))
                .ForMember(dest => dest.TaskName,
                    opt => opt.MapFrom(src => src["TaskName"] == DBNull.Value ? null : (string)src["TaskName"]))
                .ForMember(dest => dest.TaskIsGlobal,
                    opt => opt.MapFrom(src => src["TaskIsGlobal"] == DBNull.Value ? null : (bool?)src["TaskIsGlobal"]));


            CreateMap<IDataReader, IEnumerable<SkillDto>>()
               .ConstructUsing(BuildList<SkillDto>);

            CreateMap<IDataRecord, SkillDto>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
                src["Id"] == DBNull.Value ? null : (int?)src["Id"]))

                 .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src =>
                src["DateCreated"] == DBNull.Value ? null : (DateTime?)src["DateCreated"]))

                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src =>
                src["LastUpdated"] == DBNull.Value ? null : (DateTime?)src["LastUpdated"]))

                  .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                src["Name"] == DBNull.Value ? null : (string)src["Name"]));

            // Vendor Performance Data
            CreateMap<IDataReader, IEnumerable<VendorPerformanceDto>>().ConstructUsing(BuildList<VendorPerformanceDto>);

            CreateMap<IDataRecord, VendorPerformanceDto>(MemberList.None)
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src =>
                        src["formulataskid"] == DBNull.Value ? null : (int?)src["formulataskid"]))
                .ForMember(dest => dest.Team,
                    opt => opt.MapFrom(src =>
                        src["teamname"] == DBNull.Value ? null : (string)src["teamname"]))
                .ForMember(dest => dest.Skill,
                    opt => opt.MapFrom(src =>
                        src["skillname"] == DBNull.Value ? null : (string)src["skillname"]))
                .ForMember(dest => dest.Task,
                    opt => opt.MapFrom(src =>
                        src["FormulaTaskName"] == DBNull.Value ? null : (string)src["FormulaTaskName"]))
                .ForMember(dest => dest.Formula,
                    opt => opt.MapFrom(src =>
                        src["formulaname"] == DBNull.Value ? null : (string)src["formulaname"]))
                .ForMember(dest => dest.Reviews,
                    opt => opt.MapFrom(src =>
                        src["reviews"] == DBNull.Value ? null : (int?)src["reviews"]))
                .ForMember(dest => dest.Rating,
                    opt => opt.MapFrom(src =>
                        src["avgrating"] == DBNull.Value ? null : (int?)src["avgrating"]))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src =>
                        src["price"] == DBNull.Value ? null : (decimal?)src["price"]))
                .ForMember(dest => dest.DwellTime,
                    opt => opt.MapFrom(src =>
                        src["VENDOR_DWELL"] == DBNull.Value ? null : (decimal?)src["VENDOR_DWELL"]))
                .ForMember(dest => dest.AvgDwellTime,
                    opt => opt.MapFrom(src =>
                        src["FORMULATASK_MEAN_DWELL"] == DBNull.Value ? null : (decimal?)src["FORMULATASK_MEAN_DWELL"]))
                .ForMember(dest => dest.CompletionTime,
                    opt => opt.MapFrom(src =>
                        src["VENDOR_CT"] == DBNull.Value ? null : (decimal?)src["VENDOR_CT"]))
                .ForMember(dest => dest.AvgCompletionTime,
                    opt => opt.MapFrom(src =>
                        src["FORMULATASK_MEAN_CT"] == DBNull.Value ? null : (decimal?)src["FORMULATASK_MEAN_CT"]))
                .ForMember(dest => dest.TurnaroundTime,
                    opt => opt.MapFrom(src =>
                        src["Vendor_AVG_TAT"] == DBNull.Value ? null : (decimal?)src["Vendor_AVG_TAT"]))
                .ForMember(dest => dest.AvgTurnaroundTime,
                    opt => opt.MapFrom(src =>
                        src["FORMULATASK_MEAN_TAT"] == DBNull.Value ? null : (decimal?)src["FORMULATASK_MEAN_TAT"]));

            // Vendor UpSkills
            CreateMap<IDataReader, IEnumerable<VendorUpSkillDto>>().ConstructUsing(BuildList<VendorUpSkillDto>);

            CreateMap<IDataRecord, VendorUpSkillDto>(MemberList.None)
                .ForMember(dest => dest.FormulaId,
                    opt => opt.MapFrom(src =>
                        src["FormulaId"] == DBNull.Value ? null : (int?)src["FormulaId"]))
                .ForMember(dest => dest.FormulaTaskId,
                    opt => opt.MapFrom(src =>
                        src["FormulaTaskId"] == DBNull.Value ? null : (int?)src["FormulaTaskId"]))
                .ForMember(dest => dest.Team,
                    opt => opt.MapFrom(src =>
                        src["TeamName"] == DBNull.Value ? null : (string)src["TeamName"]))
                .ForMember(dest => dest.Skill,
                    opt => opt.MapFrom(src =>
                        src["SkillName"] == DBNull.Value ? null : (string)src["SkillName"]))
                .ForMember(dest => dest.Formula,
                    opt => opt.MapFrom(src =>
                        src["FormulaName"] == DBNull.Value ? null : (string)src["FormulaName"]));

            ///
            CreateMap<IDataReader, IEnumerable<SnapshotDetailDto>>()
                .ConstructUsing(BuildList<SnapshotDetailDto>);
            CreateMap<IDataRecord, SnapshotDetailDto>(MemberList.None)
                .ForMember(dest => dest.VENDOR_ID, opt => opt.MapFrom(src => (Guid)src["VENDOR_ID"]))
                .ForMember(dest => dest.Vendor_FullName,
                    opt => opt.MapFrom(src =>
                        src["Vendor_FullName"] == DBNull.Value ? string.Empty : (string)src["Vendor_FullName"]))
                .ForMember(dest => dest.Invites,
                    opt => opt.MapFrom(src =>
                        src["INVITES"] == DBNull.Value ? default(int) : (int)src["INVITES"]))
                .ForMember(dest => dest.Active,
                    opt => opt.MapFrom(src =>
                        src["ACTIVE"] == DBNull.Value ? default(int) : (int)src["ACTIVE"]))
                .ForMember(dest => dest.AtRisk,
                    opt => opt.MapFrom(src =>
                        src["ATRISK"] == DBNull.Value ? default(int) : (int)src["ATRISK"]))
                .ForMember(dest => dest.Overdue,
                    opt => opt.MapFrom(src => src["OVERDUE"] == DBNull.Value ? default(int) : (int)src["OVERDUE"]))
                .ForMember(dest => dest.QueueRevenue,
                    opt => opt.MapFrom(src =>
                        src["QUEUEREVENUE"] == DBNull.Value ? null : (decimal?)src["QUEUEREVENUE"]))
                .ForMember(dest => dest.Lost,
                    opt => opt.MapFrom(src =>
                        src["LOST"] == DBNull.Value ? default(int) : (int)src["LOST"]))
                .ForMember(dest => dest.TotalCompleted,
                    opt => opt.MapFrom(src =>
                        src["TOTALCOMPLETED"] == DBNull.Value ? default(int) : (int)src["TOTALCOMPLETED"]))
                .ForMember(dest => dest.TotalRevenue,
                    opt => opt.MapFrom(src =>
                        src["TOTALREVENUE"] == DBNull.Value ? null : (decimal?)src["TOTALREVENUE"]));


            ///
            CreateMap<IDataReader, IEnumerable<VendorJobInvitesDto>>()
                .ConstructUsing(BuildList<VendorJobInvitesDto>);

            CreateMap<IDataRecord, VendorJobInvitesDto>(MemberList.None)
                .ForMember(dest => dest.TaskId,
                    opt => opt.MapFrom(src => src["TASKID"] == DBNull.Value ? null : (int?)src["TASKID"]))
                .ForMember(dest => dest.FormulaName,
                    opt => opt.MapFrom(src =>
                        src["FormulaName"] == DBNull.Value ? string.Empty : (string)src["FormulaName"]))
                .ForMember(dest => dest.TaskName,
                    opt => opt.MapFrom(src =>
                        src["TaskName"] == DBNull.Value ? string.Empty : (string)src["TaskName"]))
                .ForMember(dest => dest.SkillName,
                    opt => opt.MapFrom(src =>
                        src["SkillName"] == DBNull.Value ? string.Empty : (string)src["SkillName"]))
                .ForMember(dest => dest.TeamName,
                    opt => opt.MapFrom(src =>
                        src["TeamName"] == DBNull.Value ? string.Empty : (string)src["TeamName"]))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src =>
                        src["StartDate"] == DBNull.Value ? null : (DateTime?)src["StartDate"]))
                .ForMember(dest => dest.DueDate,
                    opt => opt.MapFrom(src =>
                        src["DueDate"] == DBNull.Value ? null : (DateTime?)src["DueDate"]))
                .ForMember(dest => dest.DurationHours,
                    opt => opt.MapFrom(src =>
                        src["DurationHours"] == DBNull.Value ? null : (decimal?)src["DurationHours"]))
                .ForMember(dest => dest.TimeLeftHours,
                    opt => opt.MapFrom(src =>
                        src["TimeLeftHours"] == DBNull.Value ? null : (int?)src["TimeLeftHours"]))
                .ForMember(dest => dest.ProjectTaskVendorId,
                    opt => opt.MapFrom(src =>
                        src["ProjectTaskVendorId"] == DBNull.Value ? null : (int?)src["ProjectTaskVendorId"]))
                .ForMember(dest => dest.SentOn,
                    opt => opt.MapFrom(src =>
                        src["SentOn"] == DBNull.Value ? null : (DateTime?)src["SentOn"]));


            // Vendor Tasks List
            CreateMap<IDataReader, IEnumerable<ListViewTaskDto>>()
                .ConstructUsing(BuildList<ListViewTaskDto>);

            CreateMap<IDataRecord, ListViewTaskDto>(MemberList.None)
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src["STATUS"] == DBNull.Value ? String.Empty : (string)src["STATUS"]))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src["PROJECTID"] == DBNull.Value ? null : (int?)src["PROJECTID"]))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src["PROJECTNAME"] == DBNull.Value ? String.Empty : (string)src["PROJECTNAME"]))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src["TASKID"] == DBNull.Value ? null : (int?)src["TASKID"]))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src["TASKNAME"] == DBNull.Value ? String.Empty : (string)src["TASKNAME"]))
                .ForMember(dest => dest.FormulaName, opt => opt.MapFrom(src => src["FORMULANAME"] == DBNull.Value ? String.Empty : (string)src["FORMULANAME"]))
                .ForMember(dest => dest.SkillName, opt => opt.MapFrom(src => src["SKILLNAME"] == DBNull.Value ? String.Empty : (string)src["SKILLNAME"]))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src["TEAMNAME"] == DBNull.Value ? String.Empty : (string)src["TEAMNAME"]))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src["StartDate"] == DBNull.Value ? null : (DateTime?)src["StartDate"]))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src["DUEDATE"] == DBNull.Value ? null : (DateTime?)src["DUEDATE"]))
                .ForMember(dest => dest.CompletionTime, opt => opt.MapFrom(src => src["CT"] == DBNull.Value ? null : (int?)src["CT"]))
                .ForMember(dest => dest.IsCancel, opt => opt.MapFrom(src => src["IsCancel"] == DBNull.Value ? null : (int?)src["IsCancel"]))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src["Price"] == DBNull.Value ? null : (decimal?)src["Price"]))
                .ForMember(dest => dest.IsVendor, opt => opt.MapFrom(src => true))


            //.ForMember(dest => dest.ETA, opt => opt.MapFrom(src => src["ETA"] == DBNull.Value ? null : (decimal?)src["ETA"]))
            //.ForMember(dest => dest.IS_PAST80_ETA_10, opt => opt.MapFrom(src => src["IS_PAST80_ETA_10"] == DBNull.Value ? null : (int?)src["IS_PAST80_ETA_10"]))

            //.ForMember(dest => dest.ETA_10, opt => opt.MapFrom(src => src["ETA_10"] == DBNull.Value ? null : (decimal?)src["ETA_10"]))
            //.ForMember(dest => dest.target_date_10, opt => opt.MapFrom(src => src["target_date_10"] == DBNull.Value ? null : (DateTime?)src["target_date_10"]))
            ////.ForMember(dest => dest.IS_PAST80_ETA_10, opt => opt.MapFrom(src => src["IS_PAST80_ETA_10"] == DBNull.Value ? null : (int?)src["IS_PAST80_ETA_10"]))
            //.ForMember(dest => dest.ETA_10_REMAINING_inrealtime, opt => opt.MapFrom(src => src["ETA_10_REMAINING_inrealtime"] == DBNull.Value ? String.Empty : (string)src["ETA_10_REMAINING_inrealtime"]))
            //.ForMember(dest => dest.ETA_10_REMAINING_SECONDS, opt => opt.MapFrom(src => src["ETA_10_REMAINING_SECONDS"] == DBNull.Value
            //? null : (System.Int32?)src["ETA_10_REMAINING_SECONDS"]));

            .ForMember(dest => dest.DEADLINE, opt => opt.MapFrom(src => src["DEADLINE"] == DBNull.Value ? null : (decimal?)src["DEADLINE"]))
            .ForMember(dest => dest.TARGET_DATE, opt => opt.MapFrom(src => src["TARGET_DATE"] == DBNull.Value ? null : (DateTime?)src["TARGET_DATE"]))
            .ForMember(dest => dest.IS_PAST80_DEADLINE, opt => opt.MapFrom(src => src["IS_PAST80_DEADLINE"] == DBNull.Value ? null : (int?)src["IS_PAST80_DEADLINE"]))
            .ForMember(dest => dest.DEADLINE_REMAINING_SECONDS, opt => opt.MapFrom(src => src["DEADLINE_REMAINING_SECONDS"] == DBNull.Value
            ? null : (System.Int32?)src["DEADLINE_REMAINING_SECONDS"]))
            .ForMember(dest => dest.NUDGECOUNT, opt => opt.MapFrom(src => src["NUDGECOUNT"] == DBNull.Value
            ? null : (System.Int32?)src["NUDGECOUNT"]));

            CreateMap<IDataReader, IEnumerable<PublishStatus>>()
                .ConstructUsing(BuildList<PublishStatus>);
            CreateMap<IDataRecord, PublishStatus>(MemberList.None)
                .ForMember(dest => dest.Result, opt => opt.MapFrom(src => (int)src["Result"]));

            CreateMap<IDataReader, IEnumerable<CopyFormula>>()
               .ConstructUsing(BuildList<CopyFormula>);
            CreateMap<IDataRecord, CopyFormula>(MemberList.None)
                .ForMember(dest => dest.OldTaskId, opt => opt.MapFrom(src => (int)src["OldTaskId"]))
                .ForMember(dest => dest.NewTaskId, opt => opt.MapFrom(src => (int)src["NewTaskId"]))
                .ForMember(dest => dest.OldFormulaId, opt => opt.MapFrom(src => (int)src["OldFormulaId"]))
                .ForMember(dest => dest.NewFormulaID, opt => opt.MapFrom(src => (int)src["NewFormulaID"]));

            CreateMap<IDataReader, IEnumerable<PublishFormula>>()
                .ConstructUsing(BuildList<PublishFormula>);
            CreateMap<IDataRecord, PublishFormula>(MemberList.None)
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src["Type"]))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src["Name"]))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src["Status"]))
                .ForMember(dest => dest.NewFormulaId, opt => opt.MapFrom(src => src["NewFormulaId"]))
                .ForMember(dest => dest.NewFormulaTaskId, opt => opt.MapFrom(src => src["NewFormulaTaskId"]))
                .ForMember(dest => dest.OldFormulaTaskId, opt => opt.MapFrom(src => src["OldFormulaTaskId"]));

            // All Users Tasks
            CreateMap<IDataReader, IEnumerable<HomeListViewTaskDto>>()
                .ConstructUsing(BuildList<HomeListViewTaskDto>);

            CreateMap<IDataRecord, HomeListViewTaskDto>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src["PROJECTTASKID"] == DBNull.Value ? null : (int?)src["PROJECTTASKID"]))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src["PROJECTTASK"] == DBNull.Value ? String.Empty : (string)src["PROJECTTASK"]))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src["Status"] == DBNull.Value ? String.Empty : (string)src["Status"]))
                .ForMember(dest => dest.FormulaName, opt => opt.MapFrom(src => src["FORMULA"] == DBNull.Value ? String.Empty : (string)src["FORMULA"]))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src["PROJECTID"] == DBNull.Value ? null : (int?)src["PROJECTID"]))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src["PROJECT"] == DBNull.Value ? String.Empty : (string)src["PROJECT"]))
                .ForMember(dest => dest.ProccessingUserId, opt => opt.MapFrom(src => src["ASSIGNED"] == DBNull.Value ? null : (Guid?)src["ASSIGNED"]))
                .ForMember(dest => dest.ProccessingUserName, opt => opt.MapFrom(src => src["ASSIGNED_NAME"] == DBNull.Value ? String.Empty : (string)src["ASSIGNED_NAME"]))
                .ForMember(dest => dest.ProcessingUserRole, opt => opt.MapFrom(src => src["ASSIGNED_ROLE"] == DBNull.Value ? String.Empty : (string)src["ASSIGNED_ROLE"]))
                .ForMember(dest => dest.AverageTAT, opt => opt.MapFrom(src => src["AVGTAT"] == DBNull.Value ? null : (int?)src["AVGTAT"]))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src["START"] == DBNull.Value ? null : (DateTime?)src["START"]))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src["ETA"] == DBNull.Value ? null : (DateTime?)src["ETA"]))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src["IS_READ"] == DBNull.Value ? null : (int?)src["IS_READ"]))
                .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src["UserProfilePicPath"] == DBNull.Value ? String.Empty : (string)src["UserProfilePicPath"]))
                .ForMember(dest => dest.StatusEnum, opt => opt.MapFrom(src => src["STATUS_ENUM"] == DBNull.Value ? null : (int?)src["STATUS_ENUM"]));

            // Sorted FormulaTasks
            CreateMap<IDataReader, IEnumerable<FormulaTaskSortOrderDto>>().ConstructUsing(BuildList<FormulaTaskSortOrderDto>);
            CreateMap<IDataRecord, FormulaTaskSortOrderDto>(MemberList.None)
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src["Taskid"] == DBNull.Value ? null : (int?)src["Taskid"]));

            // Get the Formula Mean Tat
            CreateMap<IDataReader, IEnumerable<FormulaMeanTatDto>>().ConstructUsing(BuildList<FormulaMeanTatDto>);
            CreateMap<IDataRecord, FormulaMeanTatDto>(MemberList.None)
                .ForMember(dest => dest.FormulaId, opt => opt.MapFrom(src => src["FORMULAID"] == DBNull.Value ? null : (int?)src["FORMULAID"]))
                .ForMember(dest => dest.MeanTat, opt => opt.MapFrom(src => src["MEAN_TAT_HRS"] == DBNull.Value ? null : (string)src["MEAN_TAT_HRS"]));

            // Get AutoSelectedVendors based on selected optionType
            CreateMap<IDataReader, IEnumerable<VendorRankByRatingDto>>().ConstructUsing(BuildList<VendorRankByRatingDto>);
            CreateMap<IDataRecord, VendorRankByRatingDto>(MemberList.None)
                .ForMember(dest => dest.FormulaId, opt => opt.MapFrom(src => src["FORMULAID"] == DBNull.Value ? null : (int?)src["FORMULAID"]))
                .ForMember(dest => dest.FormulaTaskId, opt => opt.MapFrom(src => src["FT_ID"] == DBNull.Value ? null : (int?)src["FT_ID"]))
                .ForMember(dest => dest.VendorGuid, opt => opt.MapFrom(src => src["OUTSOURCER_GUID"] == DBNull.Value ? null : (Guid?)src["OUTSOURCER_GUID"]))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src["OUTSOURCER_RATING"] == DBNull.Value ? null : (int?)src["OUTSOURCER_RATING"]))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src["OUTSOURCER_PRICE"] == DBNull.Value ? null : (decimal?)src["OUTSOURCER_PRICE"]))
                .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src["RNK"] == DBNull.Value ? null : (long?)src["RNK"]));

            // Get ShareNotesDownstream based on the selected projectTaskId
            CreateMap<IDataReader, IEnumerable<SharedProjectTaskDto>>().ConstructUsing(BuildList<SharedProjectTaskDto>);
            CreateMap<IDataRecord, SharedProjectTaskDto>(MemberList.None)
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src["PROJECTTASKID"] == DBNull.Value ? null : (int?)src["PROJECTTASKID"]));

            // Get FormulaTaskBids
            CreateMap<IDataReader, IEnumerable<VendorTaskBidDto>>()
                    .ConstructUsing(BuildList<VendorTaskBidDto>);
            CreateMap<IDataRecord, VendorTaskBidDto>(MemberList.None)
                .ForMember(dest => dest.BidId, opt => opt.MapFrom(src => src["ftv_id"] == DBNull.Value ? null : (int?)src["ftv_id"]))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src["ft_id"] == DBNull.Value ? null : (int?)src["ft_id"]))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src["ft_name"] == DBNull.Value ? null : (string)src["ft_name"]))
                .ForMember(dest => dest.FormulaName, opt => opt.MapFrom(src => src["f_name"] == DBNull.Value ? null : (string)src["f_name"]))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src["T_NAME"] == DBNull.Value ? null : (string)src["T_NAME"]))
                .ForMember(dest => dest.SkillName, opt => opt.MapFrom(src => src["S_NAME"] == DBNull.Value ? null : (string)src["S_NAME"]))
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src["FTV_CREATED"] == DBNull.Value ? null : (DateTime?)src["FTV_CREATED"]));



            // Get the Formula Mean Tat
            CreateMap<IDataReader, IEnumerable<AllFormulaMeanTatDto>>().ConstructUsing(BuildList<AllFormulaMeanTatDto>);
            CreateMap<IDataRecord, AllFormulaMeanTatDto>(MemberList.None)
                .ForMember(dest => dest.FORMULAID, opt => opt.MapFrom(src => src["FORMULAID"] == DBNull.Value ? null : (int?)src["FORMULAID"]))
                .ForMember(dest => dest.ISGLOBAL, opt => opt.MapFrom(src => src["ISGLOBAL"] == DBNull.Value ? null : (int?)src["ISGLOBAL"]))

                .ForMember(dest => dest.OWNERGUID, opt => opt.MapFrom(src => src["OWNERGUID"] == DBNull.Value ? null : (Guid?)src["OWNERGUID"]))
                .ForMember(dest => dest.OUTSOURCER_TAT, opt => opt.MapFrom(src => src["OUTSOURCER_TAT"] == DBNull.Value ? null : (int?)src["OUTSOURCER_TAT"]))
                .ForMember(dest => dest.TOTAL_TAT, opt => opt.MapFrom(src => src["TOTAL_TAT"] == DBNull.Value ? null : (int?)src["TOTAL_TAT"]));

            // Company Performance Data
            CreateMap<IDataReader, IEnumerable<CompanyPerformanceDto>>().ConstructUsing(BuildList<CompanyPerformanceDto>);

            CreateMap<IDataRecord, CompanyPerformanceDto>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (Guid)src["Company_Id"]))
                .ForMember(dest => dest.Team,
                    opt => opt.MapFrom(src =>
                        src["teamname"] == DBNull.Value ? null : (string)src["teamname"]))
                .ForMember(dest => dest.Skill,
                    opt => opt.MapFrom(src =>
                        src["skillname"] == DBNull.Value ? null : (string)src["skillname"]))
                .ForMember(dest => dest.Formula,
                    opt => opt.MapFrom(src =>
                        src["formulaname"] == DBNull.Value ? null : (string)src["formulaname"]))
                .ForMember(dest => dest.FormulaId,
                    opt => opt.MapFrom(src =>
                        src["formulaid"] == DBNull.Value ? null : (int?)src["formulaid"]))
                .ForMember(dest => dest.FormulaTaskId,
                    opt => opt.MapFrom(src =>
                        src["formulataskid"] == DBNull.Value ? null : (int?)src["formulataskid"]))
                .ForMember(dest => dest.Task,
                    opt => opt.MapFrom(src =>
                        src["FormulaTaskName"] == DBNull.Value ? null : (string)src["FormulaTaskName"]))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src =>
                        src["COMPANYPRICE"] == DBNull.Value ? null : (decimal?)src["COMPANYPRICE"]))
                .ForMember(dest => dest.Reviews,
                    opt => opt.MapFrom(src =>
                        src["reviews"] == DBNull.Value ? null : (int?)src["reviews"]))
                .ForMember(dest => dest.Rating,
                    opt => opt.MapFrom(src =>
                        src["avgrating"] == DBNull.Value ? null : (int?)src["avgrating"]))
                .ForMember(dest => dest.NoOfWorkers,
                    opt => opt.MapFrom(src =>
                        src["NoOfWorkers"] == DBNull.Value ? null : (int?)src["NoOfWorkers"]))
                .ForMember(dest => dest.DwellTime,
                    opt => opt.MapFrom(src =>
                        src["VENDOR_DWELL"] == DBNull.Value ? null : (decimal?)src["VENDOR_DWELL"]))
                .ForMember(dest => dest.AvgDwellTime,
                    opt => opt.MapFrom(src =>
                        src["FORMULATASK_MEAN_DWELL"] == DBNull.Value ? null : (decimal?)src["FORMULATASK_MEAN_DWELL"]))
                .ForMember(dest => dest.CompletionTime,
                    opt => opt.MapFrom(src =>
                        src["VENDOR_CT"] == DBNull.Value ? null : (decimal?)src["VENDOR_CT"]))
                .ForMember(dest => dest.AvgCompletionTime,
                    opt => opt.MapFrom(src =>
                        src["FORMULATASK_MEAN_CT"] == DBNull.Value ? null : (decimal?)src["FORMULATASK_MEAN_CT"]))
                .ForMember(dest => dest.TurnaroundTime,
                    opt => opt.MapFrom(src =>
                        src["Vendor_AVG_TAT"] == DBNull.Value ? null : (decimal?)src["Vendor_AVG_TAT"]))
                .ForMember(dest => dest.AvgTurnaroundTime,
                    opt => opt.MapFrom(src =>
                        src["FORMULATASK_MEAN_TAT"] == DBNull.Value ? null : (decimal?)src["FORMULATASK_MEAN_TAT"]));


            //// Company User Price
            //CreateMap<IDataReader, IEnumerable<CompanyUserPriceDto>>().ConstructUsing(BuildList<CompanyUserPriceDto>);

            //CreateMap<IDataRecord, CompanyUserPriceDto>(MemberList.None)
            //    .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => (Guid)src["Company_Id"]))
            //    .ForMember(dest => dest.CompanyWorkerId, opt => opt.MapFrom(src => (Guid)src["VENDOR_ID"]))
            //    .ForMember(dest => dest.CompanyWorkerFullName,
            //        opt => opt.MapFrom(src =>
            //            src["VENDOR_FULLNAME"] == DBNull.Value ? null : (string)src["VENDOR_FULLNAME"]))
            //    .ForMember(dest => dest.Formula,
            //        opt => opt.MapFrom(src =>
            //            src["formulaname"] == DBNull.Value ? null : (string)src["formulaname"]))
            //    .ForMember(dest => dest.FormulaId,
            //        opt => opt.MapFrom(src =>
            //            src["formulaid"] == DBNull.Value ? null : (int?)src["formulaid"]))
            //    .ForMember(dest => dest.FormulaTaskId,
            //        opt => opt.MapFrom(src =>
            //            src["formulataskid"] == DBNull.Value ? null : (int?)src["formulataskid"]))
            //    .ForMember(dest => dest.Task,
            //        opt => opt.MapFrom(src =>
            //            src["FormulaTaskName"] == DBNull.Value ? null : (string)src["FormulaTaskName"]))
            //    .ForMember(dest => dest.CompanyPrice,
            //        opt => opt.MapFrom(src =>
            //            src["COMPANYPRICE"] == DBNull.Value ? null : (decimal?)src["COMPANYPRICE"]))
            //    .ForMember(dest => dest.CompanyWorkerPrice,
            //        opt => opt.MapFrom(src =>
            //            src["COMPANYWORKERPRICE"] == DBNull.Value ? null : (decimal?)src["COMPANYWORKERPRICE"]))
            //    .ForMember(dest => dest.Reviews,
            //        opt => opt.MapFrom(src =>
            //            src["reviews"] == DBNull.Value ? null : (int?)src["reviews"]))
            //    .ForMember(dest => dest.Rating,
            //        opt => opt.MapFrom(src =>
            //            src["avgrating"] == DBNull.Value ? null : (int?)src["avgrating"]))
            //    .ForMember(dest => dest.NoOfWorkers,
            //        opt => opt.MapFrom(src =>
            //            src["NoOfWorkers"] == DBNull.Value ? null : (int?)src["NoOfWorkers"]));



            // Company User Detail
            CreateMap<IDataReader, IEnumerable<CompanyUserDetailDto>>().ConstructUsing(BuildList<CompanyUserDetailDto>);

            CreateMap<IDataRecord, CompanyUserDetailDto>(MemberList.None)
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => (Guid)src["Company_Id"]))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (Guid)src["VENDOR_ID"]))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src["VENDOR_FULLNAME"] == DBNull.Value ? null : (string)src["VENDOR_FULLNAME"]))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src =>
                        src["VENDOR_EMAIL"] == DBNull.Value ? null : (string)src["VENDOR_EMAIL"]))
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src =>
                        src["VENDOR_ROLE"] == DBNull.Value ? null : (string)src["VENDOR_ROLE"]))
                .ForMember(dest => dest.Team,
                    opt => opt.MapFrom(src =>
                        src["teamname"] == DBNull.Value ? null : (string)src["teamname"]))
                .ForMember(dest => dest.Skill,
                    opt => opt.MapFrom(src =>
                        src["skillname"] == DBNull.Value ? null : (string)src["skillname"]))
                .ForMember(dest => dest.Formula,
                    opt => opt.MapFrom(src =>
                        src["formulaname"] == DBNull.Value ? null : (string)src["formulaname"]))
                .ForMember(dest => dest.FormulaId,
                    opt => opt.MapFrom(src =>
                        src["formulaid"] == DBNull.Value ? null : (int?)src["formulaid"]))
                .ForMember(dest => dest.FormulaTaskId,
                    opt => opt.MapFrom(src =>
                        src["formulataskid"] == DBNull.Value ? null : (int?)src["formulataskid"]))
                .ForMember(dest => dest.Task,
                    opt => opt.MapFrom(src =>
                        src["FormulaTaskName"] == DBNull.Value ? null : (string)src["FormulaTaskName"]))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src =>
                        src["COMPANYPRICE"] == DBNull.Value ? null : (decimal?)src["COMPANYPRICE"]))
                .ForMember(dest => dest.Reviews,
                    opt => opt.MapFrom(src =>
                        src["reviews"] == DBNull.Value ? null : (int?)src["reviews"]))
                .ForMember(dest => dest.Rating,
                    opt => opt.MapFrom(src =>
                        src["avgrating"] == DBNull.Value ? null : (int?)src["avgrating"]))
                .ForMember(dest => dest.EmailConfirmed,
                    opt => opt.MapFrom(src =>
                        src["isEmailConfirmed"] == DBNull.Value ? null : (bool?)src["isEmailConfirmed"]));

            // Company User Price
            CreateMap<IDataReader, IEnumerable<CompanyUserPriceDto>>().ConstructUsing(BuildList<CompanyUserPriceDto>);

            CreateMap<IDataRecord, CompanyUserPriceDto>(MemberList.None)
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => (Guid)src["Company_Id"]))
                .ForMember(dest => dest.CompanyWorkerId, opt => opt.MapFrom(src => (Guid)src["COMPANYWORKER_ID"]))
                .ForMember(dest => dest.CompanyWorkerFullName,
                    opt => opt.MapFrom(src =>
                        src["COMPANYWORKER_FULLNAME"] == DBNull.Value ? null : (string)src["COMPANYWORKER_FULLNAME"]))
                .ForMember(dest => dest.Formula,
                    opt => opt.MapFrom(src =>
                        src["formulaname"] == DBNull.Value ? null : (string)src["formulaname"]))
                .ForMember(dest => dest.FormulaId,
                    opt => opt.MapFrom(src =>
                        src["formulaid"] == DBNull.Value ? null : (int?)src["formulaid"]))
                .ForMember(dest => dest.FormulaTaskId,
                    opt => opt.MapFrom(src =>
                        src["formulataskid"] == DBNull.Value ? null : (int?)src["formulataskid"]))
                .ForMember(dest => dest.Task,
                    opt => opt.MapFrom(src =>
                        src["FormulaTaskName"] == DBNull.Value ? null : (string)src["FormulaTaskName"]))
                .ForMember(dest => dest.CompanyPrice,
                    opt => opt.MapFrom(src =>
                        src["COMPANYPRICE"] == DBNull.Value ? null : (decimal?)src["COMPANYPRICE"]))
                .ForMember(dest => dest.CompanyWorkerPrice,
                    opt => opt.MapFrom(src =>
                        src["COMPANYWORKERPRICE"] == DBNull.Value ? null : (decimal?)src["COMPANYWORKERPRICE"]))
                .ForMember(dest => dest.Reviews,
                    opt => opt.MapFrom(src =>
                        src["reviews"] == DBNull.Value ? null : (int?)src["reviews"]))
                .ForMember(dest => dest.Rating,
                    opt => opt.MapFrom(src =>
                        src["avgrating"] == DBNull.Value ? null : (int?)src["avgrating"]))
                .ForMember(dest => dest.NoOfWorkers,
                    opt => opt.MapFrom(src =>
                        src["NoOfWorkers"] == DBNull.Value ? null : (int?)src["NoOfWorkers"]));

            // Get CompanyTaskBids
            CreateMap<IDataReader, IEnumerable<CompanyTaskBidDto>>()
                    .ConstructUsing(BuildList<CompanyTaskBidDto>);
            CreateMap<IDataRecord, CompanyTaskBidDto>(MemberList.None)
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => (Guid)src["COMPANYWORKER_ID"]))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src["COMPANYWORKER_FULLNAME"] == DBNull.Value ? null : (string)src["COMPANYWORKER_FULLNAME"]))
                .ForMember(dest => dest.BidId, opt => opt.MapFrom(src => src["ftv_id"] == DBNull.Value ? null : (int?)src["ftv_id"]))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src["ft_id"] == DBNull.Value ? null : (int?)src["ft_id"]))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src["ft_name"] == DBNull.Value ? null : (string)src["ft_name"]))
                .ForMember(dest => dest.FormulaName, opt => opt.MapFrom(src => src["f_name"] == DBNull.Value ? null : (string)src["f_name"]))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src["T_NAME"] == DBNull.Value ? null : (string)src["T_NAME"]))
                .ForMember(dest => dest.SkillName, opt => opt.MapFrom(src => src["S_NAME"] == DBNull.Value ? null : (string)src["S_NAME"]))
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src["FTV_CREATED"] == DBNull.Value ? null : (DateTime?)src["FTV_CREATED"]));




        }

        private IEnumerable<T> BuildList<T>(IDataReader arg, ResolutionContext cont)
        {
            var list = new List<T>();
            while (arg.Read())
            {
                list.Add(cont.Mapper.Map<T>(arg));
            }
            return list;
        }
    }
}
