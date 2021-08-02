using IAutoM8.WebSockets.Stores.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Stores.Interfaces
{
    public interface INotificationSocketStore
    {
        Task TaskNewNotification(Guid user, NotificationModel message);
        Task TaskNewNotification(IEnumerable<Guid> users, NotificationModel message);
        Task TaskUnreadNotificationCount(Guid user, int count);
    }
}
