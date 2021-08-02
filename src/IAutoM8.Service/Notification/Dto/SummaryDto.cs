using System;
using IAutoM8.Global.Enums;

namespace IAutoM8.Service.Notification.Dto
{
    internal class SummaryDto
    {
        public Guid UserId { get; set; }
        public ActivityType Type { get; set; }
        public string Title { get; set; }
        public Guid? ExecuterId { get; set; }
        public DateTime Time { get; set; }
    }
}
