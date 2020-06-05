// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to persisted, unindexed dataset details
    /// </summary>
    public abstract class UserClaimsStorageService : CosmosStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetStorageService" /> class.
        /// </summary>
        /// <param name="options">The current request context</param>
        /// <param name="sasTokens">The SAS token generation service.</param>
        protected UserClaimsStorageService(IOptions<CosmosConfiguration> options, SasTokenService sasTokens)
            : base(options)
        {
            this.SasTokens = sasTokens;
        }

        /// <summary>
        /// The SAS Token generation service
        /// </summary>
        protected SasTokenService SasTokens { get; }

        /// <summary>
        /// Gets the user's name from a Claim principal
        /// </summary>
        /// <param name="user">The user claim</param>
        /// <returns>The identifier, or null if the claim does not exist</returns>
        protected static string GetUserName(ClaimsPrincipal user)
        {
            return GetClaim(user, null, "name");
        }

        /// <summary>
        /// Gets the user's email from a Claim principal
        /// </summary>
        /// <param name="user">The user claim</param>
        /// <returns>The identifier, or null if the claim does not exist</returns>
        protected static string GetUserEmail(ClaimsPrincipal user)
        {
            return GetClaim(user, null, ClaimTypes.Email);
        }

        /// <summary>
        /// Gets the user's unique identifier from a Claim principal
        /// </summary>
        /// <param name="user">The user claim</param>
        /// <returns>The identifier, or null if the claim does not exist</returns>
        protected static Guid? GetUserId(ClaimsPrincipal user)
        {
            var claim = GetClaim(user, null, ClaimTypes.NameIdentifier);
                // "http://schemas.microsoft.com/identity/claims/objectidentifier" - Obsolete?
            if (claim == null)
            {
                return null;
            }

            try { 
				return Guid.Parse(claim);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the value of a claim from a Claim principal
        /// </summary>
        /// <param name="user">The user claim</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="claimTypes">The claim types to search.</param>
        /// <returns>The claim or a default value if the claim is not found</returns>
        protected static string GetClaim(ClaimsPrincipal user, string defaultValue, params string[] claimTypes)
        {
            if (user == null)
            {
                return defaultValue;
            }

            var result = (from claim in user.Claims
                          where claimTypes.Contains(claim.Type)
                          select claim.Value).FirstOrDefault();

            return result;
        }

        protected async Task<DatasetStorageItem> RetrieveDatasetDocument(Guid datasetId, CancellationToken cancellationToken)
        {
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(datasetId.ToString())
            };
            var documentLink = this.CreateDatasetDocumentUri(datasetId);
            var document = await this.Client.ReadDocumentAsync<DatasetStorageItem>(documentLink.ToString(), options).ConfigureAwait(false);
            var doc = document.Document;
            return (doc == null || doc.DataType != StorageDataType.Dataset) ? null : doc;
        }
    }
}
