using System;

namespace Msr.Odr.Model.Datasets
{
    public enum DatasetStorageTypes
    {
        Blob
    };

    public abstract class DatasetStorageDetails
    {
        public Guid DatasetId;
        public DatasetStorageTypes StorageType;
    }

    public class DatasetBlobStorageDetails : DatasetStorageDetails
    {
        public string Account;
        public string Container;
    }
}
