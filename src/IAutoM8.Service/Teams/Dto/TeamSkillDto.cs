using IAutoM8.Service.Skills.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Teams.Dto
{
    public class TeamSkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public Guid OwnerGuid { get; set; }
    }
}
