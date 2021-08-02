using System;

namespace IAutoM8.Domain.Models.Team
{
    public class TeamUser
    {
        public int TeamId { get; set; }
        public Guid UserId { get; set; }

        #region Navigation Properties

        public virtual Team Team { get; set; }
        public virtual User.User User { get; set; }

        #endregion
    }
}
