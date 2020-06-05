// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Base class for Azure Search based services
    /// </summary>
    public abstract class AzureSearchService
    {
        /// <summary>
        /// Stores a value indicating how long to cache the  client. Because only
        /// a single client instance should be created per domain, we cache the instance.
        /// If the search options change, the index is automatically updated to match.
        /// </summary>
        private static readonly TimeSpan ClientCacheTime = TimeSpan.FromMinutes(15);

        /// <summary>
        /// The search index client
        /// </summary>
        private readonly Lazy<SearchIndexClient> client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSearchService" /> class.
        /// </summary>
        /// <param name="options">The current context</param>
        protected AzureSearchService(IOptions<SearchConfiguration> options)
        {
            this.Configuration = options.Value;
            this.client = new Lazy<SearchIndexClient>(this.CreateSearchClient, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the search client.
        /// </summary>
        /// <value>The client.</value>
        protected SearchIndexClient Client
        {
            get
            {
                return this.client.Value;
            }
        }

        /// <summary>
        /// Gets the configuration options.
        /// </summary>
        /// <value>The options.</value>
        protected SearchConfiguration Configuration { get; }

        /// <summary>
        /// Creates the search client.
        /// </summary>
        /// <returns>The search client instance.</returns>
        protected abstract SearchIndexClient CreateSearchClient();

        private static readonly ConcurrentDictionary<string, SearchIndexClient> _searchIndexes =
            new ConcurrentDictionary<string, SearchIndexClient>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Creates the search client.
        /// </summary>
        /// <param name="index">The index name.</param>
        /// <returns><see cref="Microsoft.Azure.Search.SearchIndexClient"/>.</returns>
        protected SearchIndexClient CreateSearchClient(string index)
        {
            if (string.IsNullOrWhiteSpace(index))
            {
                throw new ArgumentNullException(nameof(index));
            }

            return _searchIndexes
                .GetOrAdd(
                    index,
                    (name) => new SearchIndexClient(Configuration.Account, name, new SearchCredentials(Configuration.Key)));
        }
    }
}
