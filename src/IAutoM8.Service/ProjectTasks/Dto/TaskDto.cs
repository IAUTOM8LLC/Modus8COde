using IAutoM8.Service.Resources.Dto;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class TaskDto : UpdateTaskDto
    {
        public short? ReviewRating { get; set; }
        public bool HasVendor { get; set; }
        public bool HasAcceptedVendor { get; set; }
        public bool HasAssignedVendor { get; set; }
        public int? FormulaId { get; set; }
        public int? ParentTaskId { get; set; }
        public bool CanBeProccessed { get; set; }
        public bool CanBeReviewed { get; set; }
        public string ProccessingUserName { get; set; }
        public string ReviewingUserName { get; set; }
        public Guid? ProccessingUserId { get; set; }
        public Guid? ReviewingUserId { get; set; }
        public Guid OwnerGuid { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool? DescNotificationFlag { get; set; }
        public string ReviewerTraining { get; set; }

        public List<int> ParentTasks { get; set; }
        public List<int> ChildTasks { get; set; }
        public List<int> ConditionalParentTasks { get; set; }
        public List<ResourceDto> Resources { get; set; }
        public List<ProjectNotesDto> Notes { get; set; }
        public List<ProjectTaskOutsourceDto> Outsources { get; set; }
        public bool? ShowTrainingTab { get; set; }
        public bool IsTrainingLocked { get; set; }
        public bool IsDisabled { get; set; }
    }
}
