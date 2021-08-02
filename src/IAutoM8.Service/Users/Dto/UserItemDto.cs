using IAutoM8.Domain.Models.Project.Task;
using System;

namespace IAutoM8.Service.Users.Dto
{
    public class UserItemDto
    {
        public Guid Id { get; set; }
        public Guid? OwnerId { get; set; }
        public string Email { get; set; }
        public bool IsVendor { get; set; }

        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string TaskName { get; set; }
        public string ProjectName { get; set; }
    }
}
