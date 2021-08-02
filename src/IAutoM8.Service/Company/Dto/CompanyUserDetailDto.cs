using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Company.Dto
{
    public class CompanyUserDetailDto
    {
        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Team { get; set; }
        public string Skill { get; set; }
        public string Formula { get; set; }
        public int FormulaId { get; set; }
        public int FormulaTaskId { get; set; }
        public string Task { get; set; }
        public decimal Price { get; set; }
        public int Reviews { get; set; }
        public int Rating { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    public class CompanyUserData
    {
        //public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string Role { get; set; }
        public bool? Action { get; set; }
        public List<UsertaskData> UsertaskData { get; set; }
        public bool EmailConfirmed { get; set; }

    }

    public class UsertaskData
    {
        //public Guid CompanyId { get; set; }
        //public Guid Id { get; set; }
        //public int FormulaId { get; set; }
        public int FormulaTaskId { get; set; }
        public string Team { get; set; }
        public string Skill { get; set; }
        public string Formula { get; set; }
        public string Task { get; set; }
        public decimal Price { get; set; }
        public int Reviews { get; set; }
        public int Rating { get; set; }
    }
}
