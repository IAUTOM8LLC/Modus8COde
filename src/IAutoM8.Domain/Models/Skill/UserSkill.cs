using System;

namespace IAutoM8.Domain.Models.Skill
{
    public class UserSkill
    {
        public int SkillId { get; set; }
        public Guid UserId { get; set; }

        #region Navigation Properties

        public virtual Skill Skill { get; set; }
        public virtual User.User User { get; set; }

        #endregion
    }
}
