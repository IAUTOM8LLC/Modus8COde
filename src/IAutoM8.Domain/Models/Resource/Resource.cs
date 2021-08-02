using IAutoM8.Global.Enums;
using System;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Resource
{
    public class Resource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Mime { get; set; }
        public int Size { get; set; }
        public ResourceType Type { get; set; }
        public ResourceOriginType OriginType { get; set; }
        public string TimeStamp { get; set; }

        public virtual ICollection<ResourceFormula> ResourceFormula { get; set; } = new List<ResourceFormula>();
        public virtual ICollection<ResourceFormulaTask> ResourceFormulaTask { get; set; } = new List<ResourceFormulaTask>();
        public virtual ICollection<ResourceProject> ResourceProject { get; set; } = new List<ResourceProject>();
        public virtual ICollection<ResourceProjectTask> ResourceProjectTask { get; set; } = new List<ResourceProjectTask>();
    }
}
