// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boxed.Mapping;
using Msr.Odr.Model;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services.Mappers;

namespace Msr.Odr.Services
{
    public static class ModelExtensions
    {
        public static Dataset ToDataset(this DatasetStorageItem source, Action<Dataset> update = null)
        {
            return ModelMapper<DatasetStorageItemMapper>.Mapper.Map<DatasetStorageItem, Dataset>(source).WithUpdate(update);
        }

        public static DatasetEditStorageItem ToDatasetEditStorageItem(this DatasetStorageItem source, Action<DatasetEditStorageItem> update = null)
        {
            return ModelMapper<DatasetStorageItemMapper>.Mapper.Map<DatasetStorageItem, DatasetEditStorageItem>(source).WithUpdate(update);
        }

        public static DatasetStorageItem ToDatasetStorageItem(this Dataset source, Action<DatasetStorageItem> update = null)
        {
            return ModelMapper<DatasetMapper>.Mapper.Map(source).WithUpdate(update);
        }

        public static DatasetNominationStorageItem ToDatasetNominationStorageItem(this DatasetNomination source, Action<DatasetNominationStorageItem> update = null)
        {
            return ModelMapper<DatasetNominationMapper>.Mapper.Map(source).WithUpdate(update);
        }

        public static DatasetNomination ToDatasetNomination(this DatasetNominationStorageItem source, Action<DatasetNomination> update = null)
        {
            return ModelMapper<DatasetNominationStorageItemMapper>.Mapper.Map(source).WithUpdate(update);
        }

        public static T WithUpdate<T>(this T item, Action<T> update)
        {
            update?.Invoke(item);
            return item;
        }

        public static async Task<T> WithUpdateAsync<T>(this T item, Func<T, Task<T>> update)
        {
            return await update(item);
        }

        public static ICollection<T> OrEmptyList<T>(this IEnumerable<T> list)
        {
            return (list ?? Enumerable.Empty<T>()).ToList();
        }
    }
}
