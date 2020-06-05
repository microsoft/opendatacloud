// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Boxed.Mapping;
using Msr.Odr.Model;
using Msr.Odr.Model.Datasets;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services;
using Msr.Odr.WebApi.ViewModels.Mappers;

namespace Msr.Odr.WebApi.ViewModels
{
    public static class ModelExtensions
    {
        public static DatasetViewModel ToDatasetViewModel(this Dataset source, Action<DatasetViewModel> update = null)
        {
            return ModelMapper<DatasetMapper>.Mapper.Map(source).WithUpdate(update);
        }

        public static DatasetEditViewModel ToDatasetEditViewModel(this DatasetEditStorageItem source, Action<DatasetEditViewModel> update = null)
        {
            return ModelMapper<DatasetEditStorageItemMapper>.Mapper.Map(source).WithUpdate(update);
        }

        public static DatasetEditStorageItem ToDatasetEditStorageItem(this DatasetEditViewModel source, Action<DatasetEditStorageItem> update = null)
        {
            return ModelMapper<DatasetEditViewModelMapper>.Mapper.Map(source).WithUpdate(update);
        }
    }
}
