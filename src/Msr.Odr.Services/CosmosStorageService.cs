// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Services.Configuration;

namespace Msr.Odr.Services
{
    /// <summary>
    /// Base class for services that utilize the Azure Cosmos DB service.
    /// </summary>
    public abstract class CosmosStorageService
    {
        /// <summary>
        /// Stores a value indicating how long to cache the storage client. Because only
        /// a single client instance should be created per domain, we cache the instance.
        /// If the search options change, the index is automatically updated to match.
        /// </summary>
        private static readonly TimeSpan ClientCacheTime = TimeSpan.FromMinutes(15);

        /// <summary>
        /// The client reference
        /// </summary>
        private readonly Lazy<DocumentClient> client;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosStorageService" /> class.
        /// </summary>
        /// <param name="options">The current request context</param>
        protected CosmosStorageService(IOptions<CosmosConfiguration> options)
        {
            this.Configuration = options.Value;
			this.client = new Lazy<DocumentClient>(this.CreateDocumentClient, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the Cosmos client.
        /// </summary>
        /// <value>The client.</value>
        protected DocumentClient Client
        {
            get
            {
                return this.client.Value;
            }
        }

        /// <summary>
        /// Gets the document collection URI.
        /// </summary>
        /// <value>The document collection URI.</value>
        protected Uri DatasetDocumentCollectionUri
        {
            get
            {
                var uri = UriFactory.CreateDocumentCollectionUri(this.Configuration.Database, this.Configuration.DatasetCollection);
                return uri;
            }
        }

        /// <summary>
        /// Gets the document collection URI.
        /// </summary>
        /// <value>The document collection URI.</value>
        protected Uri UserDataDocumentCollectionUri
        {
            get
            {
                var uri = UriFactory.CreateDocumentCollectionUri(this.Configuration.Database, this.Configuration.UserDataCollection);
                return uri;
            }
        }

        /// <summary>
        /// Gets the dataset document collection URI by document id.
        /// </summary>
        /// <value>The document collection URI.</value>
        protected Uri DatasetDocumentUriById(string documentId)
        {
            var uri = UriFactory.CreateDocumentUri(this.Configuration.Database, this.Configuration.DatasetCollection, documentId);
            return uri;
        }

        /// <summary>
        /// Gets the userdata document collection URI by document id.
        /// </summary>
        /// <value>The document collection URI.</value>
        protected Uri UserDataDocumentUriById(string documentId)
        {
            var uri = UriFactory.CreateDocumentUri(this.Configuration.Database, this.Configuration.UserDataCollection, documentId);
            return uri;
        }

        /// <summary>
        /// Gets the Cosmos DB configuration options.
        /// </summary>
        /// <value>The options.</value>
        private CosmosConfiguration Configuration { get; }

        /// <summary>
        /// Creates a document URI reference for the specified identifier.
        /// </summary>
        /// <param name="documentId">The document identifier.</param>
        /// <returns>Uri reference for the document</returns>
        protected Uri CreateDatasetDocumentUri(Guid documentId)
        {
            var uri = UriFactory.CreateDocumentUri(this.Configuration.Database, this.Configuration.DatasetCollection, documentId.ToString());
            return uri;
        }

        /// <summary>
        /// Creates a document URI reference for the specified identifier.
        /// </summary>
        /// <param name="documentId">The document identifier.</param>
        /// <returns>Uri reference for the document</returns>
        protected Uri CreateUserDataDocumentUri(Guid documentId)
        {
            var uri = UriFactory.CreateDocumentUri(this.Configuration.Database, this.Configuration.UserDataCollection, documentId.ToString());
            return uri;
        }

        /// <summary>
        /// Creates a dataset document attachment URI reference.
        /// </summary>
        /// <param name="documentId">The document identifier.</param>
        /// <param name="attachmentName">Name of the attachment.</param>
        /// <returns>Uri reference for the document</returns>
        protected Uri CreateDatasetDocumentAttachmentUri(Guid documentId, string attachmentName)
        {
            var uri = UriFactory.CreateAttachmentUri(this.Configuration.Database, this.Configuration.DatasetCollection, documentId.ToString(), attachmentName);
            return uri;
        }

        /// <summary>
        /// Creates a user data document attachment URI reference.
        /// </summary>
        /// <param name="documentId">The document identifier.</param>
        /// <param name="attachmentName">Name of the attachment.</param>
        /// <returns>Uri reference for the document</returns>
        protected Uri CreateUserDataDocumentAttachmentUri(Guid documentId, string attachmentName)
        {
            var uri = UriFactory.CreateAttachmentUri(this.Configuration.Database, this.Configuration.UserDataCollection, documentId.ToString(), attachmentName);
            return uri;
        }

        private static readonly Object _lockObject = new Object();
        private static DocumentClient _documentClient = null;

        /// <summary>
        /// Creates the CosmosDB client.
        /// </summary>
        /// <returns>The <see cref="DocumentClient "/> instance</returns>
        protected DocumentClient CreateDocumentClient()
        {
            if (_documentClient == null)
            {
                lock (_lockObject)
                {
                    if (_documentClient == null)
                    {
                        var policy = new ConnectionPolicy
                        {
                            ConnectionMode = ConnectionMode.Direct,
                            ConnectionProtocol = Protocol.Tcp
                        };

                        _documentClient = new DocumentClient(Configuration.Uri, Configuration.Key, policy);
                    }
                }
            }

            return _documentClient;
        }
    }
}
