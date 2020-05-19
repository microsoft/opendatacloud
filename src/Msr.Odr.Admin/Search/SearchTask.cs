using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Msr.Odr.Admin.Commands.Options;

namespace Msr.Odr.Admin.Search
{
	/// <summary>
	/// Base for Azure Search services.
	/// </summary>
	public abstract class SearchTask
	{
		/// <summary>
		/// The client instance cache.
		/// </summary>
		private Dictionary<string, ISearchServiceClient> clientInstances = new Dictionary<string, ISearchServiceClient>();

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchTask" /> class.
		/// </summary>
		/// <param name="search">The Azure search configuration options.</param>
		/// <param name="index">The index options</param>
		public SearchTask(SearchOptions search, IndexOptions index)
		{
			this.SearchOptions = search;
			this.IndexOptions = index;
		}

		/// <summary>
		/// Gets the Search options
		/// </summary>
		protected SearchOptions SearchOptions
		{
			get;
		}

		/// <summary>
		/// Gets the index options
		/// </summary>
		protected IndexOptions IndexOptions
		{
			get;
		}

		/// <summary>
		/// Executes the task asynchronously
		/// </summary>
		/// <returns>A status code </returns>
		public abstract Task<int> ExecuteAsync();

		/// <summary>
		/// Creates the client asynchronously.
		/// </summary>
		/// <returns>Creates the document client instance</returns>
		protected Task<ISearchServiceClient> CreateClientAsync()
		{
			var clientKey = $"{this.SearchOptions.Name}:{this.SearchOptions.Key}";
			if (this.clientInstances.ContainsKey(clientKey))
			{
				return Task.FromResult(this.clientInstances[clientKey]);
			}

			ISearchServiceClient searchClient = new SearchServiceClient(this.SearchOptions.Name, new SearchCredentials(this.SearchOptions.Key));
			this.clientInstances[clientKey] = searchClient;
			return Task.FromResult(searchClient);
		}

		/// <summary>
		/// Monitors an indexing operation
		/// </summary>
		/// <param name="indexer">The name of the indexer</param>
		/// <returns>The completion task</returns>
		protected async Task MonitorIndexingAsync(string indexer)
		{
			var client = await this.CreateClientAsync();
			Console.WriteLine($"Waiting for {indexer} to finish ...");

			while (true)
			{
				var status = await client.Indexers.GetStatusAsync(indexer);
			    var lastResult = status.LastResult;
				if (lastResult != null)
				{
					switch (lastResult.Status)
					{
						case IndexerExecutionStatus.InProgress:
							Console.WriteLine($"Synchronizing {indexer}");
							break;

						case IndexerExecutionStatus.Success:
							Console.WriteLine($"Synchronized {indexer}: {lastResult.ItemCount} succeeded, {lastResult.FailedItemCount} failed");
							return;

						default:
							Console.WriteLine($"Synchronization of {indexer} failed with message '{lastResult.ErrorMessage}'. {lastResult.ItemCount} succeeded, {lastResult.FailedItemCount} failed");
							foreach (var error in lastResult.Errors)
							{
								Console.WriteLine($"\tError: {error.ErrorMessage}");
							}
							return;
					}
				}

			    await Task.Delay(1000);
			}
        }
	}
}
