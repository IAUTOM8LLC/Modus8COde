using System;
using System.Collections.Generic;
using System.Text;
using IAutoM8.Global.Enums;

namespace IAutoM8.Domain.Models.Project.Task
{
    public class ProjectTaskUser
    {
        public int ProjectTaskId { get; set; }
        public Guid UserId { get; set; }
        public ProjectTaskUserType ProjectTaskUserType { get; set; }

        public virtual ProjectTask ProjectTask { get; set; }
        public virtual User.User User { get; set; }
    }
}
