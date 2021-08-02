using IAutoM8.Service.ProjectTasks.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Infrastructure
{
    public class ProjectNoteDtoComparer : IEqualityComparer<ProjectNotesDto>
    {
        public bool Equals(ProjectNotesDto x, ProjectNotesDto y)
        {
            if (object.ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(ProjectNotesDto obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
