using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Msr.Odr.Services
{
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Gets the user's name from a Claim principal
        /// </summary>
        /// <param name="user">The user claim</param>
        /// <returns>The identifier, or null if the claim does not exist</returns>
        public static string GetUserName(this IPrincipal user)
        {
            return GetClaim(user as ClaimsPrincipal, null, "name");
        }

        /// <summary>
        /// Gets the user's email from a Claim principal
        /// </summary>
        /// <param name="user">The user claim</param>
        /// <returns>The identifier, or null if the claim does not exist</returns>
        public static string GetUserEmail(this IPrincipal user)
        {
            return GetClaim(user as ClaimsPrincipal, null, ClaimTypes.Upn, ClaimTypes.Email);
        }

        /// <summary>
        /// Gets the user's unique identifier from a Claim principal (the unique id within the identity provider).
        /// </summary>
        /// <param name="user">The user claim</param>
        /// <returns>The identifier, or null if the claim does not exist</returns>
        public static string GetUserId(this IPrincipal user)
        {
            return GetClaim(user as ClaimsPrincipal, null, ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Gets the value of a claim from a Claim principal
        /// </summary>
        /// <param name="user">The user claim</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="claimTypes">The claim types to search.</param>
        /// <returns>The claim or a default value if the claim is not found</returns>
        private static string GetClaim(ClaimsPrincipal user, string defaultValue, params string[] claimTypes)
        {
            if (user == null)
            {
                return defaultValue;
            }

            return user.Claims
                .Where(claim => claimTypes.Contains(claim.Type, StringComparer.InvariantCultureIgnoreCase))
                .Select(claim => claim.Value)
                .FirstOrDefault();
        }
    }
}
