namespace IAutoM8.Domain.Models.Team
{
    public class TeamSkill
    {
        public int SkillId { get; set; }
        public int TeamId { get; set; }

        #region Navigation Properties

        public virtual Skill.Skill Skill { get; set; }
        public virtual Team Team { get; set; }

        #endregion
    }
}
