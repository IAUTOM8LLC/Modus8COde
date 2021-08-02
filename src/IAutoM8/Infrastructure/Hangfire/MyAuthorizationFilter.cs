using Hangfire.Dashboard;

namespace IAutoM8.Infrastructure.Hangfire
{

    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // Allow all users to see the Dashboard (potentially dangerous).
            return true;
        }
    }
}
