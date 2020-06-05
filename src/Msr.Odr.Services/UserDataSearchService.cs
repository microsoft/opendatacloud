// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;
using Msr.Odr.Model;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to the stored user data from Azure search
    /// </summary>
    public class UserDataSearchService : AzureSearchService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataSearchService" /> class.
        /// </summary>
        /// <param name="options">The current request context</param>
        public UserDataSearchService(IOptions<SearchConfiguration> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets a page of dataset nominations.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<PagedResult<DatasetNomination>> GetNominations(int page, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = new SearchParameters()
            {
                OrderBy = new[] { "timeStamp desc" },
                Top = PageHelper.PageSize,
                Skip = page * PageHelper.PageSize,
                IncludeTotalResultCount = true
            };

            var results = await Client.Documents.SearchAsync<DatasetNominationStorageItem>("*", parameters, null, cancellationToken);

            return new PagedResult<DatasetNomination>
            {
                PageCount = PageHelper.CalculateNumberOfPages(results.Count),
                Value = (from item in results.Results
                    let doc = item.Document
                    select doc.ToDatasetNomination()).ToList()
            };
        }

        /// <summary>
        /// Searches for dataset nominations
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The search results.</returns>
        public async Task<PagedResult<DatasetNomination>> SearchNominations(NominationSearch criteria, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = new SearchParameters
            {
                //OrderBy = new[] {(criteria.SortOrder == SortOrder.Modified ? "modified desc" : "name asc")},
                OrderBy = new[] { "timeStamp desc" },
                Top = PageHelper.PageSize,
                Skip = criteria.Page * PageHelper.PageSize,
                IncludeTotalResultCount = true,
                ScoringProfile = "textBoostScoring",
                Facets = criteria.Facets.Keys.ToList(),
                Filter = CreateFilterString(criteria)
            };

            var searchTerm = string.IsNullOrWhiteSpace(criteria.Terms) ? "*" : criteria.Terms;
            var results = await Client.Documents.SearchAsync<DatasetNominationStorageItem>(searchTerm, parameters, null, cancellationToken);

            return new PagedResult<DatasetNomination>
            {
                PageCount = PageHelper.CalculateNumberOfPages(results.Count),
                Value = (from item in results.Results
                         let doc = item.Document
                         select doc.ToDatasetNomination()).ToList()
            };
        }

        private static string CreateFilterString(NominationSearch criteria)
        {
            StringBuilder sb = new StringBuilder();

            // Convert the facets into OData formatted query members
            foreach (var facetKey in criteria.Facets.Keys.Where(t => !string.IsNullOrEmpty(t)))
            {
                var key = UpdateFacetKey(facetKey);

                if (sb.Length > 0)
                {
                    sb.Append(" and ");
                }

                if (key == FacetKey.DomainId)
                {
                    sb = AppendDomainFilterString(criteria, sb);
                }
                else if (key == FacetKey.LicenseId)
                {
                    sb = AppendLicenseFilterString(criteria, sb);
                }
                else
                {
                    bool needsOr = false;
                    foreach (var value in criteria.Facets[key].Where(t => !string.IsNullOrEmpty(t)))
                    {
                        if (!needsOr)
                        {
                            needsOr = true;
                        }
                        else
                        {
                            sb.Append($" and ");
                        }

                        sb.Append($"{key}/any(t:");
                        sb.Append($"t eq '{value.Replace("'", "''")}'");
                        sb.Append(")");
                    }
                }
            }

            return sb.ToString();
        }

        private static StringBuilder AppendLicenseFilterString(NominationSearch criteria, StringBuilder sb)
        {
            bool needsOr = false;
            foreach (var value in criteria.Facets[FacetKey.LicenseId].Where(t => !string.IsNullOrEmpty(t)))
            {
                if (!needsOr)
                {
                    needsOr = true;
                }
                else
                {
                    sb.Append($" and ");
                }

                sb.Append($"{FacetKey.LicenseId} eq '{value}'");
            }

            return sb;
        }

        private static StringBuilder AppendDomainFilterString(NominationSearch criteria, StringBuilder sb)
        {
            var domainValues = criteria.Facets[FacetKey.DomainId].Where(t => !string.IsNullOrEmpty(t)).ToList();

            if (!domainValues.Any())
            {
                return sb;
            }

            sb.Append($" ( ");
            bool needsOr = false;
            foreach (var value in domainValues)
            {
                if (!needsOr)
                {
                    needsOr = true;
                }
                else
                {
                    sb.Append($" or ");
                }

                sb.Append($"{FacetKey.DomainId} eq '{value}'");
            }
            sb.Append($" ) ");

            return sb;
        }

        private static string UpdateFacetKey(string facetKey)
        {
            var key = facetKey;
            if (key == FacetKey.Tag)
            {
                key = FacetKey.Tags;
            }

            if (key == FacetKey.FileType)
            {
                key = FacetKey.FileTypes;
            }

            if (key == FacetKey.License)
            {
                key = FacetKey.LicenseId;
            }
            return key;
        }

        /// <summary>
        /// Creates the search client.
        /// </summary>
        /// <returns>The search client instance.</returns>
        protected override SearchIndexClient CreateSearchClient()
        {
            return this.CreateSearchClient(Configuration.NominationsIndex);
        }
    }
}