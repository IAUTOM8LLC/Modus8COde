using System;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Resources.Dto;
using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class FormulaDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormulaOverview { get; set; }
        public Guid OwnerGuid { get; set; }
        public OwnerDto Owner { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool IsSharingAllowedByOriginal { get; set; }
        public string Type { get; set; }
        public bool IsLocked { get; set; }
        public bool IsStarred { get; set; }
        public int Status { get; set; }
        public FormulaShareStatusDto FormulaShareStatus { get; set; }
        public int TasksNumber { get; set; }
        public List<ResourceDto> Resources { get; set; } = new List<ResourceDto>();
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
