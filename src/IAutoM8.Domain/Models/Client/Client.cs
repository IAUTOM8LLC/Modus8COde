using System;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Client
{
    public class Client
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string OfficePhone { get; set; }
        public string MobilePhone { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public Guid BusinessOwnerGuid { get; set; }

        #region Navigation props
        public User.User BusinessOwner { get; set; }
        #endregion

        #region Navigation collections
        public List<Project.Project> Projects { get; set; } = new List<Project.Project>();
        #endregion
    }
}
