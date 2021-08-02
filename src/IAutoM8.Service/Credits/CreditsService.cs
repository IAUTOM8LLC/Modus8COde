using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Braintree;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Extensions;
using IAutoM8.Global.Options;
using IAutoM8.Repository;
using IAutoM8.Service.Braintree.Interfaces;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Credits.Dto;
using IAutoM8.Service.CreditsService.Dto;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.SendGrid.Interfaces;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Vendor.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationEntity = IAutoM8.Domain.Models.Notification;
namespace IAutoM8.Service.CreditsService
{
    class CreditsService : ICreditsService
    {
        private readonly IBraintreeGateway braintreeGateway;
        private readonly IRepo _repo;
        private readonly UserManager<User> _userManager;
        private readonly ClaimsPrincipal _principal;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly BraintreeSettings _braintreeSettings;

        public CreditsService(IOptions<BraintreeSettings> braintreeOptionSettings,
            IRepo repo,
            UserManager<User> userManager,
            IDateTimeService dateTimeService,
            INotificationService notificationService,
            IMapper mapper,
            ClaimsPrincipal principal)
        {
            _braintreeSettings = braintreeOptionSettings.Value;
            braintreeGateway = new BraintreeGateway(
                    _braintreeSettings.BraintreeEnvironment,
                    _braintreeSettings.BraintreeMerchantId,
                    _braintreeSettings.BraintreePublicKey,
                    _braintreeSettings.BraintreePrivateKey);

            _repo = repo;
            _dateTimeService = dateTimeService;
            _userManager = userManager;
            _principal = principal;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<string> GetToken()
        {

            var ownerId = _principal.GetOwnerId();
            var owner = await _repo.Read<User>()
                   .Include(up => up.Credits)
               .FirstOrDefaultAsync(x => x.Id == ownerId);

            if (owner.Credits != null && !string.IsNullOrEmpty(owner.Credits.BraintreeCustomerId))
            {
                return await braintreeGateway.ClientToken.GenerateAsync(new ClientTokenRequest
                {
                    CustomerId = owner.Credits.BraintreeCustomerId
                });
            }

            return await braintreeGateway.ClientToken.GenerateAsync();


        }

        public async Task<CreditsDto> TransferRequest(BraintreeDto braintreeDto)
        {
            if (!_principal.IsOwner() && !_principal.IsAdmin())
            {
                throw new ValidationException("You don't have rights to do that");
            }
            using (var trx = _repo.Transaction())
            {
                var owner = await trx.Track<User>()
                        .Include(t => t.Profile)
                        .Include(up => up.Credits)
                    .FirstOrDefaultAsync(x => x.Id == _principal.GetOwnerId());

                string braintreeCustomerId = "";
                var transferRequest = new TransactionRequest
                {
                    Amount = await CalculateOwnerAmountWithTax(braintreeDto.Amount),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    },
                    MerchantAccountId = _braintreeSettings.BraintreeMerchantAccountId
                };
                if (string.IsNullOrEmpty(owner.Credits?.BraintreeCustomerId ?? null))
                {
                    var customerRequest = new CustomerRequest
                    {
                        FirstName = owner.Profile.FullName,
                        PaymentMethodNonce = braintreeDto.Nonce
                    };

                    Result<Customer> customerRequestResult = braintreeGateway.Customer.Create(customerRequest);

                    if (customerRequestResult.IsSuccess())
                    {
                        Customer customer = customerRequestResult.Target;

                        braintreeCustomerId = customer.Id;

                        string cardToken = customer.PaymentMethods[0].Token;
                        transferRequest.PaymentMethodToken = cardToken;
                    }
                    else
                    {
                        string errorMessages = "";
                        foreach (ValidationError error in customerRequestResult.Errors.DeepAll())
                        {
                            errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                        }
                        throw new ValidationException(errorMessages);
                    }
                }
                else
                {
                    transferRequest.PaymentMethodNonce = braintreeDto.Nonce;
                }

                Result<Transaction> result = await braintreeGateway.Transaction.SaleAsync(transferRequest);
                if (result.IsSuccess())
                {
                    Transaction transaction = result.Target;

                    if (owner.Credits == null)
                    {
                        owner.Credits = new Domain.Models.Credits.Credits
                        {
                            TotalCredits = braintreeDto.Amount,
                            LastUpdate = DateTime.UtcNow,
                            BraintreeCustomerId = braintreeCustomerId
                        };
                    }
                    else
                    {
                        owner.Credits.TotalCredits += braintreeDto.Amount;
                        owner.Credits.LastUpdate = DateTime.UtcNow;
                        if (string.IsNullOrEmpty(owner.Credits.BraintreeCustomerId))
                        {
                            owner.Credits.BraintreeCustomerId = braintreeCustomerId;
                        }
                    }

                    trx.Add(new CreditLog
                    {
                        Amount = braintreeDto.Amount,
                        AmountWithTax = await CalculateOwnerAmountWithTax(braintreeDto.Amount),
                        HistoryTime = _dateTimeService.NowUtc,
                        ManagerId = _principal.GetUserId(),
                        Type = Global.Enums.CreditsLogType.Charge
                    });
                }
                else
                {
                    string errorMessages = "";
                    foreach (ValidationError error in result.Errors.DeepAll())
                    {
                        errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                    }
                    throw new ValidationException(errorMessages);
                }

                await trx.SaveAndCommitAsync();

                return await GetOwnerCredits();
            }
        }

        public async Task AddCredits(decimal amount, Guid userId)
        {
            using (var trx = _repo.Transaction())
            {
                var owner = await trx.Track<User>()
                        .Include(t => t.Profile)
                        .Include(up => up.Credits)
                    .FirstOrDefaultAsync(x => x.Id == userId);

                if (_principal.IsAdmin())
                {
                    if (owner.Credits == null)
                    {
                        owner.Credits = new Domain.Models.Credits.Credits
                        {
                            TotalCredits = amount,
                            LastUpdate = DateTime.UtcNow,
                            BraintreeCustomerId = null
                        };
                    }
                    else
                    {
                        owner.Credits.TotalCredits += amount;
                        owner.Credits.LastUpdate = DateTime.UtcNow;
                    }
                    trx.Add(new CreditLog
                    {
                        Amount = amount,
                        AmountWithTax = await CalculateOwnerAmountWithTax(amount),
                        HistoryTime = _dateTimeService.NowUtc,
                        ManagerId = _principal.GetUserId(),
                        Type = Global.Enums.CreditsLogType.Charge
                    });
                }
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task<CreditsDto> GetOwnerCredits()
        {
            if (!_principal.IsOwner() &&
                !_principal.IsManager() && !_principal.IsAdmin())
            {
                throw new ValidationException("You don't have rights to do that");
            }

            var ownerId = _principal.GetOwnerId();
            var owner = await _repo.Read<User>()
                   .Include(up => up.Credits)
               .FirstOrDefaultAsync(x => x.Id == ownerId);

            CreditsDto result;
            if (owner.Credits != null)
            {
                result = _mapper.Map<CreditsDto>(owner.Credits);

                var vendorRequests = await GetReservedRequests(ownerId);

                result.ReservedCredits = vendorRequests.reservedAmount;
                result.AvailableCredits = owner.Credits.TotalCredits - result.ReservedCredits;
                result.PrepaidTasksCount = vendorRequests.count;

                var taxes = await GetTaxes();

                result.Percentage = taxes.ownerTax.Percentage + taxes.braintreeTax.Percentage;
                result.Fee = taxes.ownerTax.Fee + taxes.braintreeTax.Fee;
            }
            else
            {
                var taxes = await GetTaxes();
                result = new CreditsDto
                {
                    AvailableCredits = 0,
                    ReservedCredits = 0,
                    LastUpdate = null,
                    PrepaidTasksCount = 0,
                    Percentage = taxes.ownerTax.Percentage + taxes.braintreeTax.Percentage,
                    Fee = taxes.ownerTax.Fee + taxes.braintreeTax.Fee
                };
            }

            return result;
        }

        public async Task<(int count, decimal reservedAmount)> GetReservedRequests(Guid ownerId)
        {
            var reservedRequests = await _repo.Read<ProjectTaskVendor>()
                    .Include(t => t.ProjectTask)
                        .ThenInclude(t => t.Project)
                    .Include(t => t.ProjectTask)
                        .ThenInclude(t => t.RecurrenceOptions)
                    .Where(w => w.ProjectTask.Project.OwnerGuid == ownerId
                    && (w.ProjectTask.Status != Global.Enums.TaskStatusType.Completed ||
                    (w.ProjectTask.RecurrenceOptions != null &&
                    w.ProjectTask.RecurrenceOptions.RecurrenceType == Global.Enums.FormulaTaskRecurrenceType.EndNever ||
                              (w.ProjectTask.RecurrenceOptions.RecurrenceType == Global.Enums.FormulaTaskRecurrenceType.EndAfterCertainAmount && w.ProjectTask.RecurrenceOptions.Occurrences < w.ProjectTask.RecurrenceOptions.MaxOccurrences) ||
                              (w.ProjectTask.RecurrenceOptions.RecurrenceType == Global.Enums.FormulaTaskRecurrenceType.EndOnDate && (w.ProjectTask.RecurrenceOptions.EndRecurrenceDate.Value >= w.ProjectTask.RecurrenceOptions.NextOccurenceDate || w.ProjectTask.RecurrenceOptions.Occurrences <= 1)))
                    )
                    && (w.Status == Global.Enums.ProjectRequestStatus.Send
                    || w.Status == Global.Enums.ProjectRequestStatus.Accepted))
                    .GroupBy(t => t.ProjectTaskId)
                    .Select(t => new { TaskId = t.Key, Price = t.Max(x => x.Price) }).ToListAsync();

            return (reservedRequests.Count, reservedRequests.Sum(t => t.Price));
        }

        public async Task<BalanceDto> GetBalance()
        {
            if (!_principal.IsVendor())
            {
                throw new ValidationException("You don't have rights to do that");
            }

            var vendor = await _repo.Read<User>()
                .Include(t => t.Credits)
                .Include(t => t.VendorCreditLogs)
                .SingleOrDefaultAsync(t => t.Id == _principal.GetUserId());

            var taskInProgressOrReview = _repo.Read<ProjectTask>()
                .Where(t => t.ProccessingUserGuid == vendor.Id &&
                (t.Status == Global.Enums.TaskStatusType.InProgress
                || t.Status == Global.Enums.TaskStatusType.NeedsReview));

            var vendorRequestTasks = await _repo.Read<ProjectTaskVendor>()
                .Where(t => t.VendorGuid == vendor.Id)
                .Join(taskInProgressOrReview,
                    t => t.ProjectTaskId,
                    j => j.Id,
                    (vendorRequest, task) => new TasksWithPriceDto
                    {
                        Id = task.Id,
                        Price = vendorRequest.Price,
                        Name = task.Title
                    })
                    .ToListAsync();

            foreach (var vendorRequest in vendorRequestTasks)
            {
                vendorRequest.Price = await CalculateOwnerVendorWithTax(vendorRequest.Price);
            }

            List<CreditLog> vendorCompletedRequestsWithHistoryTime = await GetCompletedVendorRequestsWithHistoryTime();
            decimal unpaidAmount;
            if (vendor.Credits != null)
            {
                unpaidAmount = vendorCompletedRequestsWithHistoryTime
                    .Where(t => t.HistoryTime > vendor.Credits?.LastUpdate).Sum(t => t.AmountWithTax);
            }
            else
            {
                unpaidAmount = vendorCompletedRequestsWithHistoryTime.Sum(t => t.AmountWithTax);
            }

            int finishedTaskCount = 0;
            if (vendor.Credits != null)
            {
                finishedTaskCount = vendor.VendorCreditLogs.Where(t => t.Type == Global.Enums.CreditsLogType.CompleteTask
                && t.HistoryTime < vendor.Credits.LastUpdate).Count();
            }

            var paymentRequest = _repo.Read<VendorPaymentRequest>().
                Where(x => x.OwnerGuid == vendor.Id && x.Status == (int)PaymentRequest.Requested).ToList().Sum(i => i.Amount);

            var payoneerEmailAvailable = _repo.Read<UserProfile>().
                Where(x => x.UserId == vendor.Id).FirstOrDefault().PayoneerEmail;

            return new BalanceDto
            {
                Expected = vendorRequestTasks.Sum(t => t.Price),
                FinishedTasksCount = finishedTaskCount,
                LastTransfer = vendor.Credits?.LastUpdate ?? null,
                Tasks = vendorRequestTasks,
                Total = vendor.Credits == null ? 0 : vendor.Credits.TotalCredits,
                //Unpaid = unpaidAmount - paymentRequest,
                Unpaid = unpaidAmount,
                RequestedAmount = paymentRequest,
                PayoneerEmailAvailable = payoneerEmailAvailable != null ? true : false
            };
        }

        public async Task<CreditsTaxDto> GetVendorTax()
        {
            var vendorTax = await _repo.Read<CreditsTax>().SingleOrDefaultAsync(t => t.Type == Global.Enums.CreditsTaxType.Vendor);
            return _mapper.Map<CreditsTaxDto>(vendorTax);
        }

        public async Task<TransferRequestDto> RequestTransfer()
        {
            if (!_principal.IsVendor())
            {
                throw new ValidationException("You don't have rights to do that");
            }

            var vendorId = _principal.GetUserId();
            if (_repo.Read<TransferRequest>()
            .Any(t => t.VendorId == vendorId && !t.IsDone))
            {
                throw new ValidationException("You have not finished request");
            }
            using (var trx = _repo.Transaction())
            {
                var vendor = await trx.Read<User>()
                .Include(t => t.Credits)
                .SingleOrDefaultAsync(t => t.Id == vendorId);

                List<CreditLog> vendorCompletedRequestsWithHistoryTime = await GetCompletedVendorRequestsWithHistoryTime();
                if (vendorCompletedRequestsWithHistoryTime.Count == 0)
                {
                    throw new ValidationException("There is no unpaid tasks for transfer request");
                }

                var transferRequest = new TransferRequest
                {
                    IsDone = false,
                    RequestTime = _dateTimeService.NowUtc,
                    VendorId = vendorId
                };

                if (vendor.Credits != null)
                {
                    transferRequest.RequestedAmountWithTax = vendorCompletedRequestsWithHistoryTime
                        .Where(t => t.HistoryTime > vendor.Credits.LastUpdate).Sum(t => t.AmountWithTax);
                    transferRequest.RequestedAmount = vendorCompletedRequestsWithHistoryTime
                        .Where(t => t.HistoryTime > vendor.Credits.LastUpdate).Sum(t => t.Amount);
                }
                else
                {
                    transferRequest.RequestedAmountWithTax = vendorCompletedRequestsWithHistoryTime.Sum(t => t.AmountWithTax);
                    transferRequest.RequestedAmount = vendorCompletedRequestsWithHistoryTime.Sum(t => t.Amount);
                }

                trx.Add(transferRequest);

                await trx.SaveAndCommitAsync();

                var activeTransferRequest = await trx.Read<TransferRequest>()
                    .SingleOrDefaultAsync(t => t.VendorId == vendorId && t.IsDone == false);

                await _notificationService.SendTransferRequestAsync(trx, activeTransferRequest.Id);
            }

            return await LoadActiveTransferRequest();
        }

        public async Task AcceptTransferRequest(int transferRequestId)
        {
            if (!_principal.IsOwner())
            {
                throw new ValidationException("You don't have rights to do that");
            }

            using (var trx = _repo.Transaction())
            {
                var activeTransferRequest = await trx.Track<TransferRequest>()
                    .Include(t => t.Vendor)
                        .ThenInclude(t => t.Credits)
                    .SingleOrDefaultAsync(t => t.Id == transferRequestId);

                if (activeTransferRequest == null)
                {
                    throw new ValidationException("Transfer request with specified id doesn't exist");
                }

                if (activeTransferRequest.IsDone)
                {
                    throw new ValidationException("Transfer request is already accepted");
                }

                activeTransferRequest.IsDone = true;
                if (activeTransferRequest.Vendor.Credits != null)
                {
                    activeTransferRequest.Vendor.Credits.LastUpdate = activeTransferRequest.RequestTime;
                    activeTransferRequest.Vendor.Credits.TotalCredits += activeTransferRequest.RequestedAmountWithTax;
                }
                else
                {
                    activeTransferRequest.Vendor.Credits = new Domain.Models.Credits.Credits
                    {
                        TotalCredits = activeTransferRequest.RequestedAmountWithTax,
                        LastUpdate = activeTransferRequest.RequestTime
                    };
                }
                var creditLog = new CreditLog
                {
                    Amount = activeTransferRequest.RequestedAmount,
                    AmountWithTax = activeTransferRequest.RequestedAmountWithTax,
                    HistoryTime = _dateTimeService.NowUtc,
                    ManagerId = _principal.GetUserId(),
                    VendorId = activeTransferRequest.VendorId,
                    Type = Global.Enums.CreditsLogType.ConfirmPayment
                };
                trx.Add(creditLog);
                await _notificationService.SendAcceptOutsourcePaymentAsync(trx, creditLog);

                await trx.SaveAndCommitAsync();
            }
        }

        public async Task<TransferRequestDto> LoadActiveTransferRequest()
        {
            var activeTransferRequest = await _repo.Read<TransferRequest>()
                .SingleOrDefaultAsync(t => t.VendorId == _principal.GetUserId() && t.IsDone == false);

            if (activeTransferRequest != null)
            {
                return _mapper.Map<TransferRequestDto>(activeTransferRequest);
            }

            return null;
        }

        private async Task<decimal> CalculateOwnerAmountWithTax(decimal amount)
        {
            var taxes = await GetTaxes();
            double amountWithTax = (double)amount + taxes.braintreeTax.Fee +
                taxes.ownerTax.Fee + ((taxes.braintreeTax.Percentage + taxes.ownerTax.Percentage) / 100 * (double)amount);
            return (decimal)amountWithTax;
        }

        private async Task<decimal> CalculateOwnerVendorWithTax(decimal amount)
        {
            var vendorTax = await _repo.Read<CreditsTax>().SingleOrDefaultAsync(t => t.Type == Global.Enums.CreditsTaxType.Vendor);
            double amountWithTax = (double)amount - vendorTax.Fee - (vendorTax.Percentage / 100 * (double)amount);
            return (decimal)amountWithTax;
        }

        private async Task<List<CreditLog>> GetCompletedVendorRequestsWithHistoryTime()
        {
            var vendorId = _principal.GetUserId();
            var vendorCompletedRequests = await _repo.Read<CreditLog>()
                            .Where(t => t.VendorId == vendorId &&
                            t.Type == Global.Enums.CreditsLogType.CompleteTask)
                            .ToListAsync();

            return vendorCompletedRequests;
        }

        private async Task<(CreditsTax ownerTax, CreditsTax braintreeTax, CreditsTax vendorTax)> GetTaxes()
        {
            var taxes = await _repo.Read<CreditsTax>().ToListAsync();

            var ownerTax = taxes.SingleOrDefault(t => t.Type == Global.Enums.CreditsTaxType.Owner);
            var braintreeTax = taxes.SingleOrDefault(t => t.Type == Global.Enums.CreditsTaxType.Braintree);
            var vendorTax = taxes.SingleOrDefault(t => t.Type == Global.Enums.CreditsTaxType.Vendor);

            return (ownerTax, braintreeTax, vendorTax);
        }

        public async Task<VendorPaymentRequestDto> AddVendorFundRequest(decimal amount)
        {
            var ownerGuid = _principal.GetOwnerId();
            var vendorFullName = _principal.GetFullName();

            var payment = new VendorPaymentRequest();
            payment.OwnerGuid = ownerGuid;
            payment.CreateDate = _dateTimeService.NowUtc;
            payment.Amount = amount;
            payment.Status = (int)PaymentRequest.Requested;

            Guid adminGuid = Guid.Parse("2A3B99E6-0A9F-42DD-15B5-08D7A66504B7");

            List<NotificationEntity> notification = new List<NotificationEntity>();

            //Send notification to vendor for requested Balance.
            notification.Add(new NotificationEntity
            {
                CreateDate = _dateTimeService.NowUtc,
                IsRead = false,
                Message = "You have requested amount of " + amount,
                NotificationType = Global.Enums.NotificationType.FundRequestFromVednor,
                RecipientGuid = ownerGuid,
                SenderGuid = adminGuid,
                TaskId = null
            });

            //Send notification to Admin.
            notification.Add(new NotificationEntity
            {
                CreateDate = _dateTimeService.NowUtc,
                IsRead = false,
                Message = "Vendor " + vendorFullName + " requested for amount " + amount,
                NotificationType = Global.Enums.NotificationType.FundRequestToAdmin,
                RecipientGuid = adminGuid,
                SenderGuid = null,
                TaskId = null
            });

            _repo.AddRange(notification);
            await _repo.AddAsync(payment);
            await _repo.SaveChangesAsync();

            return Mapper.Map<VendorPaymentRequestDto>(payment);

        }


    }
}
