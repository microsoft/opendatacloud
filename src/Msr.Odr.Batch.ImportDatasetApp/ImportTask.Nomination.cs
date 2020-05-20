using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Msr.Odr.Batch.Shared;
using Msr.Odr.Model;
using Msr.Odr.Model.UserData;
using Msr.Odr.Services;
using Msr.Odr.Services.Batch;

namespace Msr.Odr.Batch.ImportDatasetApp
{
    public partial class ImportTask
    {
        private async Task<(DatasetNomination nomination, DatasetImportProperties storage)> LoadNomination(CancellationToken cancellationToken)
        {
            var nominationId = Options.NominationId;

            var nomination = await UserDataStorage.GetByIdAsync(nominationId, cancellationToken);
            if (nomination == null)
            {
                throw new InvalidOperationException("Nomination Id was not found.");
            }

            Log.Add($"Name: {nomination.Name}");

            var storage =
                await UserDataStorage.GetDatasetImportPropertiesForNomination(nominationId, cancellationToken);
            if (storage == null)
            {
                throw new InvalidOperationException("Nomination storage details were not found.");
            }

            return (nomination, storage);
        }

        private async Task UpdateNominationStatusToCompleted(CancellationToken cancellationToken)
        {
            await UserDataStorage
                .UpdateNominationStatus(Options.NominationId, NominationStatus.Complete, null, cancellationToken);
        }

        private async Task UpdateNominationStatusToError()
        {
            try
            {
                await UserDataStorage
                    .UpdateNominationStatus(Options.NominationId, NominationStatus.Error, null, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.BatchError(ex);
            }
        }
    }
}
