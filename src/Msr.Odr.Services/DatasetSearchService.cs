// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Provides access to the stored dataset data from Azure search
    /// </summary>
    public class DatasetSearchService : AzureSearchService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetSearchService" /> class.
        /// </summary>
        /// <param name="options">The current request context</param>
        public DatasetSearchService(IOptions<SearchConfiguration> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task&lt;Msr.Odr.Api.PagedResult&lt;Msr.Odr.Api.Dataset&gt;&gt;.</returns>
        public async Task<PagedResult<Dataset>> GetAsync(int page, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = new SearchParameters()
            {
                OrderBy = new[] { "name asc" },
                Top = PageHelper.PageSize,
                Skip = page * PageHelper.PageSize,
                IncludeTotalResultCount = true
            };

            var results = await this.Client.Documents.SearchAsync<DatasetStorageItem>("*", parameters, null, cancellationToken);

            return new PagedResult<Dataset>
            {
                PageCount = PageHelper.CalculateNumberOfPages(results.Count),
                Value = (from item in results.Results
                         let doc = item.Document
                         select doc.ToDataset()).ToList()
            };
        }

        /// <summary>
        /// Gets the file types.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task&lt;System.Collections.Generic.IEnumerable&lt;System.String&gt;&gt;.</returns>
        public async Task<IEnumerable<string>> GetFileTypes(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = new SearchParameters()
            {
                Facets = new List<string>(new[] { FacetKey.FileTypes }),
                IncludeTotalResultCount = true,
                Top = 0
            };

            var results = await this.Client.Documents.SearchAsync(string.Empty,
                parameters, null, cancellationToken);
            var types = from item in results.Facets
                        from element in item.Value
                        select element.Value;
            return types.OfType<string>().OrderBy(t => t).ToArray();
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task&lt;System.Collections.Generic.IEnumerable&lt;System.String&gt;&gt;.</returns>
        public async Task<IEnumerable<string>> GetTags(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = new SearchParameters()
            {
                Facets = new List<string>(new[] { FacetKey.Tags }),
                IncludeTotalResultCount = true,
                Top = 0
            };

            var results = await this.Client.Documents.SearchAsync(string.Empty,
                parameters, null, cancellationToken);
            var types = from item in results.Facets
                        from element in item.Value
                        select element.Value;
            return types.OfType<string>().OrderBy(t => t).ToArray();
        }

        /// <summary>
        /// Searches the asynchronous.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The search results.</returns>
        public async Task<PagedResult<Dataset>> SearchAsync(DatasetSearch criteria, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var orderBy = string.Empty;

            switch (criteria.SortOrder)
            {
                case SortOrder.LastModified:
                    orderBy = "modified desc";
                    break;
                case SortOrder.Featured:
                    orderBy = "isFeatured desc, name asc";
                    break;
                default:
                    orderBy = "name asc";
                    break;
            }

            var parameters = new SearchParameters()
            {
                OrderBy = new[] { orderBy },
                Top = PageHelper.PageSize,
                Skip = criteria.Page * PageHelper.PageSize,
                IncludeTotalResultCount = true,
                ScoringProfile = "textBoostScoring"
            };

            parameters.Facets = criteria.Facets.Keys.ToList();
            parameters.Filter = CreateFilterString(criteria);

            var results = await this.Client.Documents.SearchAsync<DatasetStorageItem>(criteria.Terms, parameters, null, cancellationToken);

            return new PagedResult<Dataset>
            {
                RecordCount = results.Count.GetValueOrDefault(),
                PageCount = PageHelper.CalculateNumberOfPages(results.Count),
                Value = (from item in results.Results
                         let doc = item.Document
                         select doc.ToDataset()).ToList()
            };
        }

        /// <summary>
        /// Searches the dataset for requested number of featured datasets.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested number of featured datasets.</returns>
        public async Task<IEnumerable<Dataset>> GetFeaturedDatasetsAsync(int quantity, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = new SearchParameters
            {
                Filter = " isFeatured eq true"
            };

            var results = await this.Client.Documents.SearchAsync<DatasetStorageItem>(null, parameters, null, cancellationToken);
            var featuredDatasets =
                results.Results.OrderBy(ds => Guid.NewGuid())  // this returns a randomly ordered list of featured datasets
                                .Take(quantity)
                                .Select(ds => ds.Document.ToDataset());

            return featuredDatasets;
        }

        /// <summary>
        /// Updates the current dataset document details in Azure Search (without having to
        /// wait for the indexer to run).
        /// </summary>
        /// <param name="dataset">The updated dataset contents.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public async Task<IndexingResult> UpdateDatasetDocInSearchIndex(DatasetStorageItem dataset, CancellationToken token)
        {
            if (dataset is null)
            {
                throw new ArgumentNullException(nameof(dataset));
            }

            var searchDoc = new DatasetSearchItem()
            {
                Id = dataset.Id,
                Name = dataset.Name,
                Description = dataset.Description,
                Published = dataset.Published,
                Created = dataset.Created,
                Modified = dataset.Modified,
                License = dataset.License,
                LicenseId = dataset.LicenseId,
                Domain = dataset.Domain,
                DomainId = dataset.DomainId,
                Tags = dataset.Tags,
                FileCount = dataset.FileCount,
                FileTypes = dataset.FileTypes,
                Size = dataset.Size,
                IsCompressedAvailable = dataset.IsCompressedAvailable,
                IsDownloadAllowed = dataset.IsDownloadAllowed,
                IsFeatured = dataset.IsFeatured
            };

            var actions = new IndexAction<DatasetSearchItem>[]
            {
                IndexAction.MergeOrUpload(searchDoc)
            };
            var batch = IndexBatch.New(actions);
            var results = await Client.Documents.IndexAsync(batch);
            return results.Results.FirstOrDefault();
        }

        private static string CreateFilterString(DatasetSearch criteria)
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

        private static StringBuilder AppendLicenseFilterString(DatasetSearch criteria, StringBuilder sb)
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

        private static StringBuilder AppendDomainFilterString(DatasetSearch criteria, StringBuilder sb)
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
            return this.CreateSearchClient(Configuration.DatasetIndex);
        }

        internal async Task UpdateIndexAsync<T>(IndexBatch<T> indexBatch)
        {
            await this.Client.Documents.IndexAsync(indexBatch);
        }
    }
}