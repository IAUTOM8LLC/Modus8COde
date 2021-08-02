using System;

namespace IAutoM8.Domain.Models.User
{
    public class UserProject
    {
        public Guid UserId { get; set; }
        public int ProjectId { get; set; }
        public User User { get; set; }
        public Project.Project Project { get; set; }
    }
}
