using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Vendor.Dto
{
    public class VendorRankByRatingDto
    {
        public int FormulaId { get; set; }
        public int FormulaTaskId { get; set; }
        public Guid VendorGuid { get; set; }
        public string OptionType { get; set; }
        public long Rank { get; set; }
        public int? Rating { get; set; }
        public decimal? Price { get; set; }

    }
}
