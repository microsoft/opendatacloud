using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Msr.Odr.WebAdminPortal.Users
{
    /// <summary>
    ///   This is for DEV only.  It skip's the Admin group requirement.
    /// </summary>
    public class DevelopmentOnlyNoAuthDirectoryGroupHandler : AuthorizationHandler<AzureActiveDirectoryGroupRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AzureActiveDirectoryGroupRequirement requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
