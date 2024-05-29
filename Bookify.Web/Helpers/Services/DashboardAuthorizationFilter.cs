using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;

namespace Bookify.Web.Helpers.Services
{
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _policyName;

        public DashboardAuthorizationFilter(string policyName)
        {
            _policyName = policyName;
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var authorize = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            var isAuthorized = authorize.AuthorizeAsync(httpContext.User, _policyName).GetAwaiter().GetResult().Succeeded;

            return isAuthorized;
        }
    }
}
