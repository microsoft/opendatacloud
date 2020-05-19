using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Msr.Odr.Services.Configuration
{
    public class StorageAccountsConfiguration : IDictionary<string, string>
    {
        private string _firstAccount = null;
        private readonly IDictionary<string, string> _accountMap =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private void SetFirstAccountIfApplicable(string key)
        {
            if (_accountMap.Count == 0)
            {
                _firstAccount = key;
            }
        }

        public void Add(string key, string value)
        {
            SetFirstAccountIfApplicable(key);
            _accountMap.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _accountMap.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _accountMap.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _accountMap.TryGetValue(key, out value);
        }

        public string this[string key]
        {
            get
            {
                if (!_accountMap.TryGetValue(key, out string value))
                {
                    throw new StorageAccountNotFoundException(key);
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new StorageAccountNotFoundException(key);
                }

                return value;
            }
            set
            {
                SetFirstAccountIfApplicable(key);
                _accountMap[key] = value;
            }
        }

        public ICollection<string> Keys => _accountMap.Keys;
        public ICollection<string> Values => _accountMap.Values;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _accountMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _accountMap.GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            SetFirstAccountIfApplicable(item.Key);
            _accountMap.Add(item);
        }

        public void Clear()
        {
            _accountMap.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _accountMap.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            _accountMap.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return _accountMap.Remove(item);
        }

        public int Count => _accountMap.Count;
        public bool IsReadOnly => _accountMap.IsReadOnly;

        public string DefaultStorageAccount => _firstAccount;
    }
}
