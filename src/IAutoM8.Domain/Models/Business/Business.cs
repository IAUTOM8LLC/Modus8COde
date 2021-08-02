using System;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Business
{
    public class Business
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Occupation { get; set; }
        public DateTime ToDoSummaryTime { get; set; }

        #region Navigation props
        public virtual User.User User { get; set; }
        #endregion

        #region Navigation collections
        public virtual List<NotificationSetting> NotificationSettings { get; set; }
        #endregion
    }
}
