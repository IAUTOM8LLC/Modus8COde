using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Notification.Interfaces;
using System.Data.SqlClient;
using System.Data;
using IAutoM8.Service.Company.Interfaces;
using IAutoM8.Service.Company.Dto;
using IAutoM8.Domain.Models.Vendor;
using Microsoft.EntityFrameworkCore;

namespace IAutoM8.Service.Company
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepo _repo;
        private readonly IMapper _mapper;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4JRepository;
        private readonly IStorageService _storageService;
        private readonly ClaimsPrincipal _principal;
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly INotificationService _notificationService;

        public CompanyService(IRepo repo,
            IMapper mapper,
            ITaskNeo4jRepository taskNeo4JRepository,
            IFormulaTaskNeo4jRepository formulaTaskNeo4JRepository,
            IDateTimeService dateTimeService,
            ITaskHistoryService taskHistoryService,
            ClaimsPrincipal principal,
            IStorageService storageService,
            INotificationService notificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _taskNeo4JRepository = taskNeo4JRepository;
            _dateTimeService = dateTimeService;
            _storageService = storageService;
            _formulaTaskNeo4JRepository = formulaTaskNeo4JRepository;
            _taskHistoryService = taskHistoryService;
            _principal = principal;
            _notificationService = notificationService;
        }

        public async Task<IList<CompanyPerformanceDto>> GetPerformanceDataForCompany(Guid userId)
        {
            var rating = StatisticType.Rating;
            var working = StatisticType.Working;
            var responding = StatisticType.Responding;

            var sqlForVendorPerformance = @"[dbo].[uspGetVendorPerformanceData_Company] @userId, @rating, @working, @responding";

            return await _repo.ExecuteSql<CompanyPerformanceDto>(_mapper, sqlForVendorPerformance,
                new List<SqlParameter> {
                    new SqlParameter{ ParameterName = "@userId", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId },
                    new SqlParameter{ ParameterName = "@rating", SqlDbType = SqlDbType.TinyInt, Value = rating },
                    new SqlParameter{ ParameterName = "@working", SqlDbType = SqlDbType.TinyInt, Value = working },
                    new SqlParameter{ ParameterName = "@responding", SqlDbType = SqlDbType.TinyInt, Value = responding },
                }).ToListAsync();
        }

        public async Task<IList<CompanyUserDetailDto>> GetCompanyUserDetails(Guid userId)
        {
            var rating = StatisticType.Rating;
            var sqlForVendorPerformance = @"[dbo].[uspGetCompanyUserDetails] @CompanyId, @rating";

            return await _repo.ExecuteSql<CompanyUserDetailDto>(_mapper, sqlForVendorPerformance,
                new List<SqlParameter> {
                    new SqlParameter{ ParameterName = "@CompanyId", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId },
                    new SqlParameter{ ParameterName = "@rating", SqlDbType = SqlDbType.TinyInt, Value = rating },
                }).ToListAsync();
        }

        public async Task<IList<CompanyUserPriceDto>> GetCompanyUserPrice(Guid userId, int formulaTaskId)
        {
            var rating = StatisticType.Rating;
            var sqlForVendorPerformance = @"[dbo].[uspGetCompanyUserPrice] @CompanyId, @rating, @FORMULATASKID";

            return await _repo.ExecuteSql<CompanyUserPriceDto>(_mapper, sqlForVendorPerformance,
            new List<SqlParameter> {
                    new SqlParameter{ ParameterName = "@CompanyId", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId },
                    new SqlParameter{ ParameterName = "@rating", SqlDbType = SqlDbType.TinyInt, Value = rating },
                    new SqlParameter{ ParameterName = "@FORMULATASKID", SqlDbType = SqlDbType.Int, Value = formulaTaskId },
            }).ToListAsync();
        }

        public async Task UpdatePriceForFormulaTask(int formulaTaskId, List<FormulaTaskVendor> formulaTaskVendors)
        {
            //var availableFormulaTaskVendorList = await _repo.Track<FormulaTaskVendor>().Include(i => i.Vendor.Roles).Where(t => t.FormulaTaskId == formulaTaskId).ToListAsync();
            var availableFormulaTaskVendorList = await _repo.Track<FormulaTaskVendor>().Where(t => t.FormulaTaskId == formulaTaskId).ToListAsync();
            var updatedFormulaTaskVendorList = formulaTaskVendors.Select(i => i.VendorGuid).ToList();
            List<FormulaTaskVendor> ftvToUpdate = new List<FormulaTaskVendor>();

            foreach (var vendorGuid in updatedFormulaTaskVendorList)
            {
                var list = availableFormulaTaskVendorList.Where(x => x.VendorGuid == vendorGuid).ToList();
                ftvToUpdate.AddRange(list);
            }

            foreach (var item in ftvToUpdate)
            {
                item.Price = formulaTaskVendors.Where(i => i.VendorGuid == item.VendorGuid).FirstOrDefault().Price;
                item.LastModified = DateTime.UtcNow;
            }
            await _repo.SaveChangesAsync();
        }

        public async Task<IList<CompanyTaskBidDto>> GetCompanyTaskBids(Guid userId)
        {
            return await _repo.ExecuteSql<CompanyTaskBidDto>(_mapper,
                "[dbo].[USP_GetFormulaBidsForCompany] @inputvendorguid",
                new List<SqlParameter> { new SqlParameter { ParameterName = "@inputvendorguid", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId } })
                .ToListAsync();
        }

        public async Task CompanyUserDeletePriceForFormulaTask(Guid userId, int formulaTaskId)
        {
            var formulaTaskVendor1 = _repo.Track<FormulaTaskVendor>().Where(t => (t.FormulaTaskId == formulaTaskId && t.VendorGuid == userId) || (t.FormulaTaskId == formulaTaskId && t.ChildCompanyWorkerID == userId)).Select(t => t).ToList();

            var formulaTaskVendorCompany = await _repo.Track<FormulaTaskVendor>()
            .FirstOrDefaultAsync(t => t.FormulaTaskId == formulaTaskId && t.VendorGuid == userId);
            _repo.Remove(formulaTaskVendorCompany);
            var formulaTaskVendor = await _repo.Track<FormulaTaskVendor>()
            .FirstOrDefaultAsync(t => t.FormulaTaskId == formulaTaskId && t.ChildCompanyWorkerID == userId);
            _repo.Remove(formulaTaskVendor);

            await _repo.SaveChangesAsync();
        }

        public async Task CompanyPerformanceDeletePriceForFormulaTask(Guid companyUserId, Guid userId, int formulaTaskId)
        {
            //var formulaTaskVendor1 = _repo.Track<FormulaTaskVendor>().Where(t => (t.FormulaTaskId == formulaTaskId &&
            //t.VendorGuid == companyUserId) || (t.FormulaTaskId == formulaTaskId && t.ChildCompanyWorkerID == userId))
            // .Select(t => t).ToList();

            //var FTV_CompanySet = await _repo.Track<FormulaTaskVendor>()
            // .FirstOrDefaultAsync(t => t.FormulaTaskId == formulaTaskId && t.VendorGuid == companyUserId);
            var FTV_CompanySet = _repo.Track<FormulaTaskVendor>().Where(t => t.FormulaTaskId == formulaTaskId && t.VendorGuid == companyUserId).ToList();
            foreach (var item in FTV_CompanySet)
            {
                var formulaTaskVendorCompany1 = await _repo.Track<FormulaTaskVendor>()
                .FirstOrDefaultAsync(t => t.FormulaTaskId == formulaTaskId && t.VendorGuid == item.ChildCompanyWorkerID);
                _repo.Remove(formulaTaskVendorCompany1);
            }
            _repo.RemoveRange(FTV_CompanySet);

            await _repo.SaveChangesAsync();
        }

    }
}
