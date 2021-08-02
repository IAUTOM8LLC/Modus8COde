using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Domain.Models.User
{
    public class UserProfile
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public DateTime? Dob { get; set; }
        public GenderEnum Gender { get; set; }
        public string DisplayName { get; set; }
        public string Path { get; set; }
        public int DailyAvailability { get; set; }
        public string PayoneerEmail { get; set; }
        public Guid? CompanyWorkerOwnerID { get; set; }

        #region infusionsoft
        public int? ContactId { get; set; }
        public int? AffiliateId { get; set; }
        public string AffiliateCode { get; set; }
        public string AffiliatePass { get; set; }
        #endregion
    }
}
