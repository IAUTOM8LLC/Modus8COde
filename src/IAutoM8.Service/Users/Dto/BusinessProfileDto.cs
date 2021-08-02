using IAutoM8.Service.Notification.Dto;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Users.Dto
{
    public class BusinessProfileDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Occupation { get; set; }
        public DateTime ToDoSummaryTime { get; set; }
        public List<NotificationSettingDto> NotificationSettings { get; set; }

        public string AffCode { get; set; }
        public string AffPass { get; set; }
        public string AffLoginUrl { get; set; }
        public string SilverAffUrl { get; set; }
        public string GoldAffUrl { get; set; }
    }
}
