using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Service.Company.Dto;
using IAutoM8.Service.Vendor.Dto;

namespace IAutoM8.Service.Company.Interfaces
{
    public interface ICompanyService
    {
        Task<IList<CompanyPerformanceDto>> GetPerformanceDataForCompany(Guid userId);
        Task<IList<CompanyUserDetailDto>> GetCompanyUserDetails(Guid userGuid);
        Task<IList<CompanyUserPriceDto>> GetCompanyUserPrice(Guid userGuid, int formulaTaskId);
        Task UpdatePriceForFormulaTask(int formulaTaskId, List<FormulaTaskVendor> formulaTaskVendors);
        Task<IList<CompanyTaskBidDto>> GetCompanyTaskBids(Guid userId);
        Task CompanyUserDeletePriceForFormulaTask(Guid userId, int formulaTaskId);
        Task CompanyPerformanceDeletePriceForFormulaTask(Guid companyUserId, Guid userId, int formulaTaskId);
    }
}
