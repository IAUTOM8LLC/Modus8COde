using IAutoM8.Domain.Models.Project.Task;
using System.Collections.Generic;

namespace IAutoM8.Service.CommonDto
{
    public class ProjectTaskComparer : IEqualityComparer<ProjectTask>
    {
        public bool Equals(ProjectTask x, ProjectTask y) => x.GetHashCode().Equals(y.GetHashCode());
        public int GetHashCode(ProjectTask obj) => obj.Id;
    }

    public class ParsedTreeDetail
    {
        public IEnumerable<UniqueLeaf> UniqueLeafs { get; set; }
        public IEnumerable<TreeRootNode> RootNodes { get; set; }
    }

    public class UniqueLeaf
    {
        public IEnumerable<ProjectTask> Leafs { get; set; }
        public int UniqueLeafId { get; set; }
    }

    public class TreeRootNode
    {
        public int UniqueLeafId { get; set; }
        public ProjectTask Item { get; set; }
        public IEnumerable<TreeNode> Children { get; set; }
    }

    public class TreeNode
    {
        public ProjectTask Item { get; set; }
        public IEnumerable<TreeNode> Childrens { get; set; }
    }
}
