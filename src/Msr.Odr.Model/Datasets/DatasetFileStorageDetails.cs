using System;

namespace Msr.Odr.Model.Datasets
{
    public enum DatasetFileStorageTypes
    {
        Blob
    };

    public abstract class DatasetFileStorageDetails
    {
        public Guid DatasetId;
        public Guid FileId;
        public string Name;
        public string FullName;
        public string ContentType;
        public DatasetFileStorageTypes StorageType;
    }

    public class DatasetFileBlobStorageDetails : DatasetFileStorageDetails
    {
        public string Account;
        public string Container;
    }
}
