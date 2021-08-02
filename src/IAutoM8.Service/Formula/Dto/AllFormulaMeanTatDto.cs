using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Formula.Dto
{
    public class AllFormulaMeanTatDto
    {
        public int FORMULAID { get; set; }
        public int ISGLOBAL { get; set; }
        public Guid OWNERGUID { get; set; }
        public int OUTSOURCER_TAT { get; set; }
        public int TOTAL_TAT { get; set; }        

    }

    public class AllFormulaMeanTatSingleDto
    {
        public int FORMULAID { get; set; }
        public int ISGLOBAL { get; set; }
        public Guid OWNERGUID { get; set; }
        public decimal? OUTSOURCER_TAT { get; set; }
        public decimal? TOTAL_TAT { get; set; }        

    }
}
