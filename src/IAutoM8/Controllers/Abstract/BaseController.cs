using System;
using System.Linq;
using System.Security.Claims;
using IAutoM8.Global;
using Microsoft.AspNetCore.Mvc;

namespace IAutoM8.Controllers.Abstract
{
    [Route("api/[controller]")]
    public abstract class BaseController : Controller
    {
        public Guid UserGuid => Guid.Parse(User.Claims.Single(x => x.Type == ClaimTypes.Sid).Value);

        protected const string OwnerOrManager = UserRoles.Owner + "," + UserRoles.Manager;
        protected const string BussinessMemeber = UserRoles.Owner + "," + UserRoles.Manager + "," + UserRoles.Worker;
        protected const string VendorMemeber = UserRoles.Vendor;
        protected const string VendorOrBussinessMember = BussinessMemeber + "," + VendorMemeber;
        protected const string Admin = UserRoles.Admin;
        protected const string OwnerOrManagerOrAdmin = UserRoles.Owner + "," + UserRoles.Manager + "," + UserRoles.Admin;
        protected const string VendorOrBussinessMemberOrAdmin = BussinessMemeber + "," + VendorMemeber + "," + Admin;
        protected const string CompanyWorkerMember = UserRoles.CompanyWorker; // added new for company worker (WRT Sprint 10B).
        protected const string CompanyMember = UserRoles.Company; // added new for company (WRT Sprint 10B).
        protected const string VendorOrBussinessOrCompanyWorkerMember = VendorOrBussinessMember + "," + CompanyWorkerMember; // added new combination for company worker (WRT Sprint 10B).
        protected const string VendorOrBussinessOrCompanyWorkerOrCompanyMember = VendorOrBussinessMember + "," + CompanyWorkerMember + "," + CompanyMember; // added new combination for company and company worker (WRT Sprint 10B).
        protected const string AdminOrCompany = UserRoles.Admin + "," + UserRoles.Company;
    }
}
