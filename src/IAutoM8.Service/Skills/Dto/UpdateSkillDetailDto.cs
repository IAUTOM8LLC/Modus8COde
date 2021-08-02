using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Skills.Dto
{
    public class UpdateSkillDetailDto
    {
        public int Id { get; set; }
        public int? TeamId { get; set; }
        public string Name { get; set; }
        public bool IsGlobal { get; set; }
        public List<Guid> UserSkills { get; set; }
    }
}
