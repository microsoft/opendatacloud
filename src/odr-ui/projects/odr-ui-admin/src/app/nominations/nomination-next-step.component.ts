// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {Component, Input, Output, EventEmitter} from '@angular/core';
import {DatasetNomination} from 'odr-ui-shared';
import {NominationStatus} from '../models/nomination-status.enum';
import {DatasetStorage} from '../models/dataset-storage.type';

@Component({
    selector: 'app-nomination-next-step',
    templateUrl: './nomination-next-step.component.html',
    styleUrls: ['./nomination-next-step.component.scss'],
})
export class NominationNextStepComponent {

    @Input() nomination: DatasetNomination;
    @Input() canUpdateCallback: Function;
    @Input() canApproveCallback: Function;
    @Input() datasetStorage: DatasetStorage;

    @Output() onSubmitResult: EventEmitter<any> = new EventEmitter<any>();
    @Output() onUpdateNomination: EventEmitter<any> = new EventEmitter<any>();
    @Output() onApproveNomination: EventEmitter<any> = new EventEmitter<any>();
    @Output() onRejectNomination: EventEmitter<any> = new EventEmitter<any>();
    @Output() onCreateStorageForNomination: EventEmitter<any> = new EventEmitter<any>();
    @Output() onImportDatasetFromStorage: EventEmitter<any> = new EventEmitter<any>();

    get isPendingApproval() {
        return !this.nomination.nominationStatus || this.nomination.nominationStatus === NominationStatus.PendingApproval;
    }

    get isRejected() {
        return this.nomination.nominationStatus === NominationStatus.Rejected;
    }

    get isApproved() {
        return this.nomination.nominationStatus === NominationStatus.Approved;
    }

    get isUploading() {
        return this.nomination.nominationStatus === NominationStatus.Uploading;
    }

    get isImporting() {
        return this.nomination.nominationStatus === NominationStatus.Importing;
    }

    get isComplete() {
        return this.nomination.nominationStatus === NominationStatus.Complete;
    }

    get isError() {
        return this.nomination.nominationStatus === NominationStatus.Error;
    }

    canApprove() {
        return this.isPendingApproval && this.canApproveCallback();
    }

    canUpdate() {
        return this.isPendingApproval && this.canUpdateCallback();
    }

    canReject() {
        return this.isPendingApproval;
    }

    updateNomination() {
        this.onUpdateNomination.emit();
    }

    approveNomination() {
        this.onApproveNomination.emit();
    }

    rejectNomination() {
        this.onRejectNomination.emit();
    }

    createStorageForNomination() {
        this.onCreateStorageForNomination.emit();
    }

    importDatasetFromStorage() {
        this.onImportDatasetFromStorage.emit();
    }

    get datasetUtilityUrl() {
        return `/api/assets/datasetutil`;
    }

    get datasetConfigUrl() {
        const id = this.nomination.id;
        return `/api/assets/dataset-import/${id}`;
    }
}
