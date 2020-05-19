using System;

namespace Msr.Odr.Model.Datasets
{
    public class DatasetItemContainerDetails
    {
        public Guid DatasetId;
        public string Account;
        public string Container;
        public string Name => "Content";
        public string ContentType => "x-azure-blockstorage";
        public string Uri;
    }
}
