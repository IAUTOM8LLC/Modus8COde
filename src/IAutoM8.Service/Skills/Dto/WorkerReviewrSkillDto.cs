using System.Collections.Generic;

namespace IAutoM8.Service.Skills.Dto
{
    public class WorkerReviewrSkillDto
    {
        public List<SkillDto> WorkerSkills { get; set; }
        public List<SkillDto> ReviewerSkills { get; set; }
    }
}
