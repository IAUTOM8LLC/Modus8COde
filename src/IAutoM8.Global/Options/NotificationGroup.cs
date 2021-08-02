using System.Collections.Generic;
using IAutoM8.Global.Enums;

namespace IAutoM8.Global.Options
{
    public static class NotificationGroup
    {
        public static List<NotificationType> TaskNotifications = new List<NotificationType>
        {
            NotificationType.TaskInProgress,
            NotificationType.TaskOverdue,
            NotificationType.TaskDeadline,
            NotificationType.TaskDeclineReview,
            NotificationType.TaskNeedReview,
            NotificationType.TaskCommented
        };

        public static List<NotificationType> UserNotifications = new List<NotificationType>
        {
            NotificationType.AssignToTask,
            NotificationType.TaskInProgress,
            NotificationType.TaskOverdue,
            NotificationType.TaskDeadline,
            NotificationType.TaskDeclineReview,
            NotificationType.TaskNeedReview,
            NotificationType.AssignToProject,
            NotificationType.DailySummary,
            NotificationType.TaskCommented
        };

        public static List<NotificationType> RoleNotifications = UserNotifications;
        public static List<NotificationType> BussinessNotifications = UserNotifications;
    }
}
