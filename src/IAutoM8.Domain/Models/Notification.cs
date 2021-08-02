using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsRead { get; set; }
        public Guid RecipientGuid { get; set; }
        public Guid? SenderGuid { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public int? TaskId { get; set; }
        public NotificationType NotificationType { get; set; }

        #region Navigation Props
        public virtual User.User Recipient { get; set; }
        public virtual User.User Sender { get; set; }
        public virtual ProjectTask Task { get; set; }
        #endregion
    }
}
