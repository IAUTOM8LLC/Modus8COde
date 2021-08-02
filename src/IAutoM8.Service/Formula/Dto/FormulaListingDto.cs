using System;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Resources.Dto;
using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class FormulaListingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid OwnerGuid { get; set; }
        public OwnerDto Owner { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool IsLocked { get; set; }
        public bool IsStarred { get; set; }
        public string Type { get; set; }
        public int TasksNumber { get; set; }
        public int? Status { get; set; }
        public IList<string> Categories { get; set; }

        public decimal? OUTSOURCER_TAT { get; set; }
        public decimal? TOTAL_TAT { get; set; }
    }
}
