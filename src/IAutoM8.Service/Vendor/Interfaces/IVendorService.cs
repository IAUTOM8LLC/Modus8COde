using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.Vendor.Dto;

namespace IAutoM8.Service.Vendor.Interfaces
{
    public interface IVendorService
    {
        Task<IList<VendorUpSkillDto>> GetVendorUpSkills(Guid userId);
        Task<IList<VendorPerformanceDto>> GetPerformanceData(Guid userId);
        Task<IList<VendorTaskBidDto>> GetVendorsTaskBids(Guid userId);
        Task<FormulaTaskVendorDto> GetFormulaTaskCertification(Guid userId, int formulaTaskId);
        Task<FormulaTaskVendorDto> GetFormulaTaskVendorNotification(int notificationId);
        Task<ProjectTaskVendorDto> GetProjectTaskVendorNotification(int notificationId);
        Task<IList<SelectedVendorsByTaskDto>> GetSelectedVendorsByFormulaId(int formulaId);
        Task<IList<Guid>> GetSelectedVendorsByTaskId(int formulaId, int formulaTaskId, string optionType);
        Task DeletePriceForFormulaTask(Guid userId, int formulaTaskId);
        Task UpdatePriceForFormulaTask(Guid userId, int formulaTaskId, decimal price);
        Task UpdateProjectTaskVendorNotificationStatus(ProjectTaskVendorDto vendorNotificationDto);
        Task UpdateFormulaTaskVendorNotificationStatus(FormulaTaskVendorDto vendorNotificationDto);
        Task UpdateFormulaTaskVendorCertificationStatus(FormulaTaskVendorDto vendorNotificationDto, Guid userId);
        Task<SnapshotDetailDto> GetSnapshotDetail(Guid userId);
        Task SyncVendorData(Guid userId);
        Task SendETANotification(int taskId);
        //Task<IList<CompanyPerformanceDto>> GetPerformanceDataForCompany(Guid userId);
        //Task<IList<CompanyUserDetailDto>> GetCompanyUserDetails(Guid userGuid);
    }
}
