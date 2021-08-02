using System;
using System.Threading.Tasks;
using IAutoM8.Repository;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Service.Teams.Dto;

namespace IAutoM8.Service.Notification.Interfaces
{
    public interface INotificationService
    {
        Task SendTaskCommentAsync(int taskId);
        Task SendInProgressTaskAsync(ITransactionScope trx, int taskId);
        Task SendAssignToTaskAsync(ITransactionScope trx, int taskId);
        Task SendAssignToProjectAsync(Guid ownerGuid, string projectName, string userEmail);
        Task SendOverdueTaskAsync(ITransactionScope trx, int taskId);
        Task SendDeadlineAsync(ITransactionScope trx, int taskId);
        Task SendDeclineReviewTaskAsync(ITransactionScope trx, int taskId);
        Task SendApproveReviewTaskAsync(ITransactionScope trx, int taskId);
        Task SendNeedReviewTaskAsync(ITransactionScope trx, int taskId);
        Task SendSummaryAsync(Guid id);
        Task SendDailyToDoSummary(Guid id);
        Task SendTransferRequestAsync(ITransactionScope trx, int transferRequestId);
        Task SendStartProjectTaskAsync(ITransactionScope trx, int taskId);

        Task SendFormulaTaskOutsourcesAsync(ITransactionScope trx, int requestId);
        Task SendProjectTaskOutsourcesAsync(ITransactionScope trx, int requestId);
        Task SendProjectTaskOutsourcesUnavailableAsync(ITransactionScope trx, int requestId);
        Task SendStartProjectTaskOutsourcesAsync(ITransactionScope trx, int taskId);
        Task SendProjectTaskOutsourcesAcceptAsync(ITransactionScope trx, int taskId);
        Task SendAcceptOutsourcePaymentAsync(ITransactionScope trx, CreditLog creditLog);
        Task SendInviteOutsourceAsync(ProjectTask task);
        Task SendStopVendorTaskOnCancelNudgeAsync(ITransactionScope trx, int requestId);
        Task SendExpiredJobInvitesNotificationToOwner(ITransactionScope trx, int requestId);
        Task SendExpiredJobInvitesNotificationToVendor(ITransactionScope trx, int requestId);
        Task SendPublishFormulaNotification(PublishFormulaList item);
        Task SendPaymentRequestAcceptedAsync(ITransactionScope trx, int requestId);
        Task SendPaymentRequestDeclinedAsync(ITransactionScope trx, int requestId);
    }
}
