using System;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.User;
using IAutoM8.Global.Enums;

namespace IAutoM8.Domain.Models
{
    public class NotificationSetting
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public bool Enabled { get; set; }
        public Guid BussinessId { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? UserId { get; set; }
        public int? TaskId { get; set; }

        #region Navigation Props
        public virtual Business.Business Bussiness { get; set; }
        public virtual Role Role { get; set; }
        public virtual User.User User { get; set; }
        public virtual ProjectTask Task { get; set; }
        #endregion
    }
}
