using IAutoM8.Service.Users.Dto;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Skills.Dto
{
    public class SkillDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Guid> UserSkills { get; set; }
    }
}
