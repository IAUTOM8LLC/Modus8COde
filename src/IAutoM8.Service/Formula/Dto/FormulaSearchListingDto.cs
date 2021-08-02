using IAutoM8.Service.Users.Dto;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Formula.Dto
{
    public class FormulaSearchListingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid OwnerGuid { get; set; }
        public OwnerDto Owner { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsOwned { get; set; }
        public bool IsStarred { get; set; }
        public IList<string> Categories { get; set; }
    }
}
