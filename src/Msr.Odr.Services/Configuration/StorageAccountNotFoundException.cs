using System;

namespace Msr.Odr.Services.Configuration
{
    public class StorageAccountNotFoundException : Exception
    {
        public StorageAccountNotFoundException(string key) :
            base($"The storage account information for \"{key}\" was not found.")
        {
        }
    }
}
