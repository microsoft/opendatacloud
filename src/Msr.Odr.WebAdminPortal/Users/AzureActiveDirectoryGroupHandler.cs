// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Msr.Odr.WebAdminPortal.Users
{
    public class AzureActiveDirectoryGroupHandler : AuthorizationHandler<AzureActiveDirectoryGroupRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AzureActiveDirectoryGroupRequirement requirement)
        {
            var email = context.User.FindFirst("emails")?.Value;
            if (email != null && requirement.AuthorizedUsersSet.Contains(email))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
