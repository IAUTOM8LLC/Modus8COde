using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class ProcessingUserDto
    {
        public int TaskId { get; set; }
        public string SkillName { get; set; }
        public Guid ProcessingUserId { get; set; }
    }
}
