using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Projects.Dto
{
    public class AssignUsersToProjectDto
    {
        public IEnumerable<string> Emails { get; set; }
        public IEnumerable<Guid> Ids { get; set; }
        public int ProjectId { get; set; }
    }
}
