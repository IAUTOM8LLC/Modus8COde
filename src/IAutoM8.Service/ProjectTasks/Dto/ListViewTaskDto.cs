using System;
using System.Collections.Generic;
using System.Text;
using IAutoM8.Global.Enums;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class ListViewTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public bool HasVendor { get; set; }
        public bool HasAcceptedVendor { get; set; }
        public Guid? ProccessingUserId { get; set; }
        public string ProccessingUserRole { get; set; }
        public string ProccessingUserName { get; set; }
        public string ReviewingUserName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsRecurrent { get; set; }
        public bool IsConditional { get; set; }
        public bool IsInterval { get; set; }
        public List<Guid> AssignedUserIds { get; set; }
        public List<Guid> ReviewingUserIds { get; set; }
        public string TeamName { get; set; }
        public string SkillName { get; set; }
        public string FormulaName { get; set; }
        public int? CompletionTime { get; set; }
        public int? TurnAroundTime { get; set; }
        public StatisticType WorkType { get; set; }
        public bool IsVendor { get; set; }
        public int IsCancel { get; set; }
        public decimal Price { get; set; }
        //public decimal ETA { get; set; }
        ////public bool IS_PAST80_ETA { get; set; }

        //public decimal? ETA_10 { get; set; }
        //public DateTime? target_date_10 { get; set; }
        //public bool IS_PAST80_ETA_10 { get; set; }
        //public string ETA_10_REMAINING_inrealtime { get; set; }
        //public Int32 ETA_10_REMAINING_SECONDS { get; set; }

        public decimal DEADLINE { get; set; }
        public DateTime? TARGET_DATE { get; set; }
        public bool IS_PAST80_DEADLINE { get; set; }
        public Int32 DEADLINE_REMAINING_SECONDS { get; set; }
        public Int32 NUDGECOUNT { get; set; }
    }

    public class HomeListViewTaskDto
    {
        public int Id { get; set; }
        public int FormulaId { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public int StatusEnum { get; set; }
        public string Status { get; set; }
        public string FormulaName { get; set; }
        public string ProjectName { get; set; }
        public Guid? ProccessingUserId { get; set; }
        public string ProccessingUserName { get; set; }
        public string ProcessingUserRole { get; set; }
        public string ProfileImage { get; set; }
        public int AverageTAT { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int IsRead { get; set; }
    }
}
