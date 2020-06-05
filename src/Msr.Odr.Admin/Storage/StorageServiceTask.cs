// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Msr.Odr.Admin.Commands.Options;
using Msr.Odr.Model.Configuration;

namespace Msr.Odr.Admin.Storage
{
	/// <summary>
	/// Base for CosmosDB services.
	/// </summary>
	public abstract class StorageServiceTask
	{
		/// <summary>
		/// The special dataset identifier assigned for license detail storage
		/// </summary>
		protected static readonly Guid LicenseDatasetId = WellKnownIds.LicenseDatasetId;

		/// <summary>
		/// The client instance cache.
		/// </summary>
		private Dictionary<string, DocumentClient> clientInstances = new Dictionary<string, DocumentClient>();

		/// <summary>
		/// Initializes a new instance of the <see cref="StorageServiceTask" /> class.
		/// </summary>
		/// <param name="cosmos">The options.</param>
		public StorageServiceTask(CosmosOptions cosmos)
		{
			this.Endpoint = cosmos.Endpoint;
			this.Key = cosmos.Key;
			this.Database = cosmos.Database;
			this.Collection = cosmos.DatasetsCollection;
			this.CosmosOptions = cosmos;
		}

		/// <summary>
		/// Gets the Cosmos options
		/// </summary>
		protected CosmosOptions CosmosOptions
		{
			get;
		}

		/// <summary>
		/// Gets the Cosmos DB endpoint.
		/// </summary>
		/// <value>The endpoint.</value>
		protected string Endpoint { get; }

		/// <summary>
		/// Gets the access key.
		/// </summary>
		/// <value>The key.</value>
		protected SecureString Key { get; }

		/// <summary>
		/// Gets the database identifier.
		/// </summary>
		/// <value>The database.</value>
		protected string Database { get; }

		/// <summary>
		/// Gets the collection identifier.
		/// </summary>
		/// <value>The collection.</value>
		protected string Collection { get; }

		/// <summary>
		/// Gets the collection Uri
		/// </summary>
		/// <value>The document collection URI.</value>
		protected Uri DocumentCollectionUri
		{
			get
			{
				return UriFactory.CreateDocumentCollectionUri(this.Database, this.Collection);
			}
		}

		/// <summary>
		/// Executes the task asynchronously
		/// </summary>
		/// <returns>A status code </returns>
		public abstract Task<int> ExecuteAsync();

		/// <summary>
		/// Creates a <see cref="RequestOption" /> using the specified partition key
		/// </summary>
		/// <param name="partitionId">The partition identifier</param>
		/// <returns>Microsoft.Azure.Documents.Client.RequestOptions.</returns>
		protected RequestOptions PartitionRequestOption(Guid partitionId)
		{
			return this.PartitionRequestOption(partitionId.ToString());
		}

		/// <summary>
		/// Creates a <see cref="RequestOption" /> using the specified partition key
		/// </summary>
		/// <param name="partitionId">The partition identifier</param>
		/// <returns>Microsoft.Azure.Documents.Client.RequestOptions.</returns>
		protected RequestOptions PartitionRequestOption(string partitionId)
		{
			return new RequestOptions
			{
				PartitionKey = new PartitionKey(partitionId)
			};
		}

		/// <summary>
		/// Creates a document URI for the specified document identifier
		/// </summary>
		/// <param name="id">The document</param>
		/// <returns>The URI</returns>
		protected Uri CreateDocumentUri(Guid id)
		{
			return UriFactory.CreateDocumentUri(this.Database, this.Collection, id.ToString());
		}

		/// <summary>
		/// Creates a attachment URI for the specified document identifier
		/// </summary>
		/// <param name="id">The document</param>
		/// <param name="attachmentName">The attachment name</param>
		/// <returns>The URI</returns>
		protected Uri CreateAttachmentUri(Guid id, string attachmentName)
		{
			return UriFactory.CreateAttachmentUri(this.Database, this.Collection, id.ToString(), attachmentName);
		}

		/// <summary>
		/// Creates the client asynchronously.
		/// </summary>
		/// <returns>Creates the document client instance</returns>
		protected async Task<DocumentClient> CreateClientAsync()
		{
			var clientKey = $"{this.Endpoint}:{this.Key}";
			if (this.clientInstances.ContainsKey(clientKey))
			{
				return this.clientInstances[clientKey];
			}

			DocumentClient client = new DocumentClient(
				new Uri($"https://{this.Endpoint}.documents.azure.com/"),
				this.Key,
				new ConnectionPolicy
				{
					ConnectionMode = ConnectionMode.Direct,
					ConnectionProtocol = Protocol.Tcp
				},
				ConsistencyLevel.Session);

			await client.OpenAsync().ConfigureAwait(false);
			this.clientInstances[clientKey] = client;
			return client;
		}
	}
}
