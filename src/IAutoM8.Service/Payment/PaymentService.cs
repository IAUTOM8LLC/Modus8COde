using AutoMapper;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Payment.Dto;
using IAutoM8.Service.Payment.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IAutoM8.Service.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepo _repo;
        private readonly IMapper _mapper;
        private readonly ClaimsPrincipal _principal;
        private readonly INotificationService _notificationService;

        public PaymentService(IRepo repo,
           IMapper mapper,
            ClaimsPrincipal principal,
            INotificationService notificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _principal = principal;
            _notificationService = notificationService;
        }

        public async Task<IList<PaymentDto>> GetPaymentRequests(int type)
        {
            var query =
                type == 2 ?
                _repo.Read<TransferRequest>().Include(t => t.Vendor).ThenInclude(t => t.Profile)
                .Where(w => w.IsDone == true &&
                (w.Status == (int)TransferRequestEnum.Processed) || w.Status == (int)TransferRequestEnum.Declined)
                : _repo.Read<TransferRequest>().Include(t => t.Vendor).ThenInclude(t => t.Profile)
                    .Where(w => w.IsDone == false && w.Status == type);


            var paymentRequest = await query.ToListAsync();

            List<PaymentDto> payment = new List<PaymentDto>();

            foreach (var req in paymentRequest)
            {
                payment.Add(new PaymentDto
                {
                    Id = req.Id,
                    IsDone = req.IsDone,
                    RequestedAmount = req.RequestedAmount,
                    RequestedAmountWithTax = req.RequestedAmountWithTax,
                    RequestTime = req.RequestTime,
                    VendorId = req.VendorId,
                    VendorName = req.Vendor.Profile.FullName,
                    IsChecked = false,
                    Status = req.Status,
                    Description = req.Description
                });
            }

            return payment;
        }

        public async Task<TransferRequest> AcceptDenyRequest(int requestId, int response, string desc)
        {
            using (var trx = _repo.Transaction())
            {
                int status = response == 1 ? (int)TransferRequestEnum.Processed : (int)TransferRequestEnum.Declined;

                var paymentRequest = await trx.Track<TransferRequest>()
                    .Include(t => t.Vendor)
                        .ThenInclude(t => t.Credits)                    
                    .Where(w => w.Id == requestId)
                    .FirstOrDefaultAsync();

                paymentRequest.Status = status;
                paymentRequest.IsDone = true;
                paymentRequest.Description = desc;

                //Updated Vendor accounts Credits
                if (paymentRequest.Vendor.Credits != null)
                {
                    paymentRequest.Vendor.Credits.LastUpdate = paymentRequest.RequestTime;
                    paymentRequest.Vendor.Credits.TotalCredits += paymentRequest.RequestedAmountWithTax;
                }
                else
                {
                    paymentRequest.Vendor.Credits = new Domain.Models.Credits.Credits
                    {
                        TotalCredits = paymentRequest.RequestedAmountWithTax,
                        LastUpdate = paymentRequest.RequestTime
                    };
                }


                if (response == 1)
                {
                    await _notificationService.SendPaymentRequestAcceptedAsync(trx, requestId);
                }
                else
                {
                    await _notificationService.SendPaymentRequestDeclinedAsync(trx, requestId);
                }

                var creditLogs = await trx.Track<CreditLog>()
                  .Where(w => w.VendorId == paymentRequest.VendorId && w.Type == Global.Enums.CreditsLogType.CompleteTask).ToListAsync();
                creditLogs.ForEach(notification => notification.Type = CreditsLogType.ConfirmPayment);

                await _repo.SaveChangesAsync();


                await trx.SaveAndCommitAsync();

                return paymentRequest;
            }
        }

        public async Task<List<string>> DownloadCSV(PaymentRequestDto payments)
        {
            List<PaymentDto> paymentList = payments.Payments.Where(x => x.IsChecked).ToList();
            int count = 0;

            foreach (var rec in paymentList)
            {
                var request = _repo.Track<TransferRequest>()
                     .Where(w => w.Id == rec.Id).FirstOrDefault();
                request.Status = (int)TransferRequestEnum.InProcess;
                count++;

                if (count == 40)
                {
                    break;
                }
            }
            await _repo.SaveChangesAsync();

            List<PaymentDto> downloadableRecords = count == 40 ? paymentList.Take(40).ToList() : paymentList;

            byte[] fileByte = await AllPaymentRequestCSV(downloadableRecords);

            int downloadedCount = paymentList.Count <= 40 ? paymentList.Count : 40;

            var resultList = new List<string>();
            resultList.Add(Convert.ToBase64String(fileByte));
            resultList.Add(Convert.ToString(downloadedCount));
            return resultList;

        }

        public async Task<List<string>> BatchProcess(PaymentRequestDto payments, int isAccepted)
        {
            List<PaymentDto> paymentList = payments.Payments.Where(x => x.IsChecked).ToList();

            foreach (var rec in paymentList)
            {
                var request = _repo.Track<TransferRequest>()
                    .Include(t => t.Vendor)
                        .ThenInclude(t => t.Credits)                    
                     .Where(w => w.Id == rec.Id).FirstOrDefault();
                request.Status = isAccepted == 1 ? (int)TransferRequestEnum.Processed : (int)TransferRequestEnum.Declined;
                request.IsDone = true;



                //Updated Vendor accounts Credits
                if (request.Vendor.Credits != null)
                {
                    request.Vendor.Credits.LastUpdate = request.RequestTime;
                    request.Vendor.Credits.TotalCredits += request.RequestedAmountWithTax;
                }
                else
                {
                    request.Vendor.Credits = new Domain.Models.Credits.Credits
                    {
                        TotalCredits = request.RequestedAmountWithTax,
                        LastUpdate = request.RequestTime
                    };
                }


                var creditLogs = await _repo.Track<CreditLog>()
                  .Where(w => w.VendorId == rec.VendorId && w.Type == Global.Enums.CreditsLogType.CompleteTask).ToListAsync();
                creditLogs.ForEach(notification => notification.Type = CreditsLogType.ConfirmPayment);

                await _repo.SaveChangesAsync();
            }
            await _repo.SaveChangesAsync();
            var resultList = new List<string>();
            resultList.Add(null);
            resultList.Add(Convert.ToString(paymentList.Count()));
            return resultList;

        }

        public async Task<byte[]> AllPaymentRequestCSV(List<PaymentDto> paymentRequestList)        {            string[] columnHeaders = {            "Vendor Name",
            "Requested Amount",
            "Requested Amount With Tax",
            "Requested Time"
            };            var payments = (from item in paymentRequestList
                            select new object[]
                            {                                item.VendorName,
                                item.RequestedAmount,
                                item.RequestedAmountWithTax,
                                item.RequestTime,
                          }).ToList();


            // Build the file content
            var csv = new StringBuilder();            payments.ForEach(async (line) =>            {
                csv.AppendLine(string.Join(",", line));            });
            byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", columnHeaders)}\r\n{csv.ToString()}");
            return buffer;        }

        public string WriteTsv<T>(IEnumerable<T> data)
        {
            StringBuilder output = new StringBuilder();
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in props)
            {
                output.Append(prop.DisplayName); // header
                output.Append("\t");
            }
            output.AppendLine();
            foreach (T item in data)
            {
                foreach (PropertyDescriptor prop in props)
                {
                    output.Append(prop.Converter.ConvertToString(
                         prop.GetValue(item)));
                    output.Append("\t");
                }
                output.AppendLine();
            }
            return output.ToString();
        }
    }
}
