using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.Braintree.Interfaces;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.ProjectTasks.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks
{
    public class TaskOutsourcesService: IProjectTaskOutsourcesService
    {
        private readonly IRepo _repo;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ClaimsPrincipal _principal;

        public TaskOutsourcesService(IRepo repo,
            UserManager<User> userManager,
            INotificationService notificationService,
            ClaimsPrincipal principal,
            IDateTimeService dateTimeService)
        {
            _repo = repo;
            _userManager = userManager;
            _notificationService = notificationService;
            _dateTimeService = dateTimeService;
            _principal = principal;
        }

        public async Task CreateRequest(OutsourceRequestDto requestDto)
        {
            if (!_principal.IsOwner() &&
                !_principal.IsManager())
            {
                throw new ValidationException("You don't have rights to do that");
            }

            var owner = await _repo.Read<User>()
                   .Include(up => up.Credits)
               .FirstOrDefaultAsync(x => x.Id == _principal.GetOwnerId());

            var vendorRequests = await _repo.Read<ProjectTaskVendor>()
                    .Include(t => t.ProjectTask)
                        .ThenInclude(t => t.Project)
                    .Where(w => w.ProjectTask.Project.OwnerGuid == _principal.GetOwnerId()
                    && w.ProjectTask.Status != Global.Enums.TaskStatusType.Completed
                    && (w.Status == Global.Enums.ProjectRequestStatus.Send
                    || w.Status == Global.Enums.ProjectRequestStatus.Accepted))
                    .GroupBy(t => t.ProjectTaskId)
                    .Select(t => new { TaskId = t.Key, Price = t.Max(x => x.Price) }).ToListAsync();

            var availableCredits = owner?.Credits.TotalCredits - vendorRequests.Sum(t => t.Price);
            decimal? maxTaskRequestPrice = 0;
            var processingUser = await _repo.Read<ProjectTask>()
                .Where(w => w.Id == requestDto.TaskId)
                .Select(s => s.ProccessingUserGuid).FirstOrDefaultAsync();
            if (processingUser.HasValue)
            {
                var proccessingUser = await _userManager.FindByIdAsync(processingUser.Value.ToString());
                
                if (await _userManager.IsInRoleAsync(proccessingUser, UserRoles.Vendor))
                    throw new ValidationException("Task currently proccessed by other vendor!");

                if (await _userManager.IsInRoleAsync(proccessingUser, UserRoles.CompanyWorker)) //added logic for new role CompanyWorker WRT Sprint 10B
                    throw new ValidationException("Task currently proccessed by other vendor!");

                if (await _userManager.IsInRoleAsync(proccessingUser, UserRoles.CompanyWorker)) //added logic for new role CompanyWorker WRT Sprint 10B
                    throw new ValidationException("Task currently proccessed by other vendor!");
            }

            using (var trx = _repo.Transaction())
            {
                var now = _dateTimeService.NowUtc;
                var curOutsources = await trx.Track<ProjectTaskVendor>()
                    .Include(i => i.ProjectTask).Where(w => w.ProjectTaskId == requestDto.TaskId)
                    .ToListAsync();
                foreach (var vendor in curOutsources)
                {
                    if (vendor.Status == ProjectRequestStatus.Send &&
                          requestDto.Outsources.Any(a => !a.IsSelected && a.Id == vendor.VendorGuid))
                    {
                        vendor.Status = ProjectRequestStatus.DeclinedByOwner;
                        vendor.LastModified = now;
                    }
                    else if((vendor.Status == ProjectRequestStatus.None || vendor.Status == ProjectRequestStatus.DeclinedByOwner) &&
                        requestDto.Outsources.Any(a => a.IsSelected && a.Id == vendor.VendorGuid))
                    {
                        vendor.ProjectTask.ProccessingUserGuid = null;
                        if (vendorRequests.SingleOrDefault(t => t.TaskId == vendor.ProjectTaskId) != null)
                        {
                            maxTaskRequestPrice = vendorRequests.SingleOrDefault(t => t.TaskId == vendor.ProjectTaskId).Price;
                        }
                        if (vendor.Price > maxTaskRequestPrice)
                        {
                            if (vendor.Price - maxTaskRequestPrice > availableCredits)
                            {
                                throw new ValidationException("You don't have enough credits to do that action");
                            }
                            else
                            {
                                availableCredits -= vendor.Price - maxTaskRequestPrice;
                                maxTaskRequestPrice = vendor.Price;
                            }
                        }
                        vendor.Status = ProjectRequestStatus.Send;
                        await _notificationService.SendProjectTaskOutsourcesAsync(trx, vendor.Id);
                        vendor.LastModified = now;
                        trx.Add(new FormulaTaskStatistic
                        {
                            Created = now,
                            FormulaTaskId = vendor.ProjectTask.FormulaTaskId.Value,
                            ProjectTaskId = vendor.ProjectTaskId,
                            VendorGuid = vendor.VendorGuid,
                            Type = StatisticType.Responding
                        });
                    }
                }
                await trx.SaveAndCommitAsync();
            }
        }
    }
}
