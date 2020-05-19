using System;
using System.Collections.Generic;
using System.Security;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace Msr.Odr.Admin.Commands.Options
{
	/// <summary>
	/// Cosmos DB command line options
	/// </summary>
	public class CosmosOptions
    {
        private IConfiguration Config => Startup.Configuration;

        public string Endpoint => Config["Documents:Account"];
        public string RawKey => Config["Documents:Key"];
		public string Database => Config["Documents:Database"];
        public string DatasetsCollection => Config["Documents:DatasetCollection"];
        public string UserDataCollection => Config["Documents:UserDataCollection"];

        public SecureString Key => RawKey?.ToSecureString();

        public Uri DatasetsDocumentCollectionUri => UriFactory.CreateDocumentCollectionUri(this.Database, this.DatasetsCollection);
        public Uri UserDataDocumentCollectionUri => UriFactory.CreateDocumentCollectionUri(this.Database, this.UserDataCollection);
	}
}
