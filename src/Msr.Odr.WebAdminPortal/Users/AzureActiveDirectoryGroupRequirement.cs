// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;

namespace Msr.Odr.WebAdminPortal.Users
{
    public class AzureActiveDirectoryGroupRequirement : IAuthorizationRequirement
    {
        public HashSet<string> AuthorizedUsersSet { get; }

        public AzureActiveDirectoryGroupRequirement(string authorizedUsers)
        {
            if (string.IsNullOrWhiteSpace(authorizedUsers))
            {
                throw new System.ArgumentException("List of authorized admin users must be provided.", nameof(authorizedUsers));
            }

            var splitRegex = new Regex(@"\s*;\s*");

            var userList = splitRegex.Split(authorizedUsers)
                .Where(user => !string.IsNullOrWhiteSpace(user))
                .Select(user => user.Trim())
                .ToList();
            AuthorizedUsersSet = new HashSet<string>(userList, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
