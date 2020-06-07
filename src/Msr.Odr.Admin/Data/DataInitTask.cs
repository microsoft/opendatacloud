// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markdig;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Msr.Odr.Admin.Commands.Options;
using Msr.Odr.Model.Configuration;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msr.Odr.Admin.Data
{
    public class DataInitTask
    {
        public CosmosOptions CosmosOptions { get; }
        public StorageOptions StorageOptions { get; }
        public DataInitTypes SelectedTypes { get; }

        public DataInitTask(
            CosmosOptions cosmosOptions,
            StorageOptions storageOptions,
            DataInitTypes selectedTypes)
        {
            CosmosOptions = cosmosOptions;
            StorageOptions = storageOptions;
            SelectedTypes = selectedTypes;
        }

        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine($"Adding data to {CosmosOptions.Endpoint} CosmosDB");

            using (var client =
                new DocumentClient(
                    new Uri($"https://{CosmosOptions.Endpoint}.documents.azure.com/"),
                    CosmosOptions.RawKey,
                    new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp
                    },
                    ConsistencyLevel.Session))
            {
                await client.OpenAsync();

                if ((SelectedTypes & DataInitTypes.Domains) != 0)
                {
                    Console.WriteLine("Adding Domains document.");
                    await AddDomains(client);
                }

                if ((SelectedTypes & DataInitTypes.Licenses) != 0)
                {
                    Console.WriteLine("Adding standard Licenses documents.");
                    await AddLicenses(client);
                }

                if ((SelectedTypes & DataInitTypes.FAQs) != 0)
                {
                    Console.WriteLine("Adding FAQ documents.");
                    await AddFAQs(client);
                }

                if ((SelectedTypes & DataInitTypes.Email) != 0)
                {
                    Console.WriteLine("Adding Email templates.");
                    await AddEmailTemplates(client);
                }

                if ((SelectedTypes & DataInitTypes.ARM) != 0)
                {
                    Console.WriteLine("Adding ARM template documents.");
                    await AddARMTemplates(client);
                }

                if ((SelectedTypes & DataInitTypes.DatasetOwners) != 0)
                {
                    Console.WriteLine("Adding Dataset Owners document.");
                    await AddDatasetOwners(client);
                }
            }

            return 0;
        }

        private async Task AddDomains(DocumentClient client)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(
                CosmosOptions.Database,
                CosmosOptions.DatasetsCollection);
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };

            var doc = await ReadJsonDocument("domains.json");
            await client.UpsertDocumentAsync(uri, doc, options);
        }

        private async Task AddLicenses(DocumentClient client)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(
                CosmosOptions.Database,
                CosmosOptions.DatasetsCollection);
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.LicenseDatasetId.ToString())
            };

            var licenseFiles = FindDocuments("license-*.json");
            Console.WriteLine($"Found {licenseFiles.Count} License documents");
            foreach (var fileName in licenseFiles)
            {
                Console.WriteLine(fileName);
                var doc = await ReadJsonDocument(fileName);
                await client.UpsertDocumentAsync(uri, doc, options);
            }
        }

        private async Task AddFAQs(DocumentClient client)
        {
            var partitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString());

            var markdown = await ReadDocument("initial-faqs.md");
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            var index = 0;
            var splitRegex = new Regex(@"(?=^#.*$)", RegexOptions.Multiline);
            var titleRegex = new Regex(@"(?<=^#.*$)", RegexOptions.Multiline);
            var faqs = splitRegex.Split(markdown)
                .Where(txt => !string.IsNullOrWhiteSpace(txt))
                .Select(txt =>
                {
                    var sections = titleRegex.Split(txt);
                    return (
                        Title: sections[0].TrimStart('#').Trim(),
                        Content: Markdown.ToHtml(sections[1].Trim(), pipeline),
                        Index: ++index
                    );
                })
                .ToList();

            Console.WriteLine($"Found {faqs.Count} FAQs");

            var idList = new List<string>();
            var queryable = client
                .CreateDocumentQuery(
                    UriFactory.CreateDocumentCollectionUri(CosmosOptions.Database, CosmosOptions.DatasetsCollection),
                    "SELECT c.id FROM c WHERE c.dataType = 'faq'",
                    new FeedOptions
                    {
                        PartitionKey = partitionKey,
                        MaxItemCount = -1
                    })
                .AsDocumentQuery();
            while (queryable.HasMoreResults)
            {
                var results = await queryable.ExecuteNextAsync();
                idList.AddRange(results.Select(r => (string)r.id));
            }
            Console.WriteLine($"Deleting {idList.Count} existing FAQs");
            var options = new RequestOptions
            {
                PartitionKey = partitionKey
            };
            foreach (var id in idList)
            {
                await client.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(CosmosOptions.Database, CosmosOptions.DatasetsCollection, id),
                    options);
            }

            var uri = UriFactory.CreateDocumentCollectionUri(
                CosmosOptions.Database,
                CosmosOptions.DatasetsCollection);
            foreach (var faq in faqs)
            {
                Console.WriteLine($"[{faq.Index}] {faq.Title}");
                //Console.WriteLine(faq.Content);
                //Console.WriteLine();

                var doc = new
                {
                    id = Guid.NewGuid().ToString(),
                    title = faq.Title,
                    content = faq.Content,
                    order = faq.Index,
                    datasetId = WellKnownIds.ConfigurationDatasetId.ToString(),
                    dataType = "faq",
                };
                await client.UpsertDocumentAsync(uri, doc, options);
            }
        }

        private async Task AddEmailTemplates(DocumentClient client)
        {
            // Content generated from Fxns/EmailTemplatesII project
            var templateMap = new Dictionary<string, string>
            {
                { "286353d1-2d54-4d25-8930-00465136cb96", "dataset-nomination.html" },
                { "ab4313a8-b3fc-447a-bf61-413e6a6c983f", "dataset-issue.html" },
                { "0926ee72-a168-4ae5-afb3-12e46d09e263", "nomination-approved.html" },
                { "8b891dda-6d5f-4bcb-ab11-b5f0ebd61dcd", "nomination-rejected.html" },
                { "e63bd247-16f6-45d9-bc49-0ad1372963c9", "general-issue.html" },
            };

            var uri = UriFactory.CreateDocumentCollectionUri(
                CosmosOptions.Database,
                CosmosOptions.DatasetsCollection);
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };

            Console.WriteLine($"Found {templateMap.Count} Email templates");
            foreach (var kvp in templateMap)
            {
                Console.WriteLine(kvp.Value);
                var content = await ReadDocument(kvp.Value);
                var doc = new
                {
                    html = content,
                    id = kvp.Key,
                    datasetId = WellKnownIds.ConfigurationDatasetId.ToString(),
                    dataType = "email-templates",
                };
                await client.UpsertDocumentAsync(uri, doc, options);
            }
        }

        private async Task AddARMTemplates(DocumentClient client)
        {
            var workPath = Environment.CurrentDirectory;

            Console.WriteLine($"Working from {workPath}");

            var templateMapFile = Path.Combine(workPath, "..", "Msr.Odr.WebApi", "template-mapping.json");
            var templateMap = JObject.Parse(await File.ReadAllTextAsync(templateMapFile));

            var uri = UriFactory.CreateDocumentCollectionUri(
                CosmosOptions.Database,
                CosmosOptions.DatasetsCollection);
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };

            foreach (JProperty item in templateMap["armTemplates"])
            {
                Console.WriteLine($" - {item.Name}");
                var value = (JObject)item.Value;
                var id = (string)value["id"];
                var template = (string)value["template"];

                var templateFileName = Path.Combine(workPath, "../../assets/deployments", template);
                var templateDoc = JObject.Parse(await File.ReadAllTextAsync(templateFileName));

                var doc = new
                {
                    id,
                    datasetId = WellKnownIds.ConfigurationDatasetId.ToString(),
                    dataType = "arm-template",
                    name = item.Name,
                    fileName = template,
                    template = templateDoc,
                };
                await client.UpsertDocumentAsync(uri, doc, options);
            }

            foreach (JProperty item in templateMap["staticAssets"])
            {
                Console.WriteLine($" - {item.Name}");
                var value = (JObject)item.Value;
                var id = (string)value["id"];
                var fileName = (string)value["file"];
                var mimeType = (string)value["mimeType"];

                var assetFileName = Path.Combine(workPath, "../../assets/deployments", fileName);
                var content = await File.ReadAllTextAsync(assetFileName);

                var doc = new
                {
                    id,
                    datasetId = WellKnownIds.ConfigurationDatasetId.ToString(),
                    dataType = "static-asset",
                    name = item.Name,
                    fileName,
                    mimeType,
                    content,
                };
                await client.UpsertDocumentAsync(uri, doc, options);
            }
        }

        private async Task AddDatasetOwners(DocumentClient client)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(
                CosmosOptions.Database,
                CosmosOptions.DatasetsCollection);
            var options = new RequestOptions
            {
                PartitionKey = new PartitionKey(WellKnownIds.ConfigurationDatasetId.ToString())
            };

            var doc = await ReadJsonDocument("datasetOwners.json");
            await client.UpsertDocumentAsync(uri, doc, options);
        }

        private string DataFilesPath => Path.Combine(AppContext.BaseDirectory, "Data/Files");

        private ICollection<string> FindDocuments(string searchPattern)
        {
            return Directory.EnumerateFiles(DataFilesPath, searchPattern)
                .Select(fn => Path.GetFileName(fn))
                .ToList();
        }

        private async Task<JObject> ReadJsonDocument(string name)
        {
            var content = await ReadDocument(name);
            return JObject.Parse(content);
        }

        private async Task<string> ReadDocument(string name)
        {
            var fullName = Path.Combine(DataFilesPath, name);
            return await File.ReadAllTextAsync(fullName);
        }
    }
}
