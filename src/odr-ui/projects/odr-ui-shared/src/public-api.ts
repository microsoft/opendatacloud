// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/*
 * Public API Surface of odr-ui-shared
 */

export { SharedLibModule } from "./odr-ui-shared.module";

export { AppVarDirective } from "./components/app-var/app-var.directive";

export { datasetNameFormControl, DatasetNameEditorComponent } from "./components/dataset-components/dataset-name-editor.component";
export { datasetFeaturedFormControl, DatasetFeaturedEditorComponent }
    from "./components/dataset-components/dataset-featured-editor.component";
export {
    datasetDescriptionFormControl,
    DatasetDescriptionEditorComponent
} from "./components/dataset-components/dataset-description-editor.component";
export { datasetUrlFormControl, DatasetUrlEditorComponent } from "./components/dataset-components/dataset-url-editor.component";
export { projectUrlFormControl, ProjectUrlEditorComponent } from "./components/dataset-components/project-url-editor.component";
export {
    datasetPublishedFormControl,
    DatasetPublishedEditorComponent
} from "./components/dataset-components/dataset-published-editor.component";
export { datasetVersionFormControl, DatasetVersionEditorComponent } from "./components/dataset-components/dataset-version-editor.component";
export { datasetDomainFormControl, DatasetDomainEditorComponent } from "./components/dataset-components/dataset-domain-editor.component";
export {
    datasetLicenseFormControl,
    DatasetLicenseEditorComponent
} from "./components/dataset-components/dataset-license-editor.component";
export { datasetTagsFormControl, DatasetTagsEditorComponent } from "./components/dataset-components/dataset-tags-editor.component";
export {
    datasetFileTypesFormControl,
    DatasetFileTypesEditorComponent
} from "./components/dataset-components/dataset-filetypes-editor.component";
export {
    datasetContactNameFormControl,
    DatasetContactNameEditorComponent
} from "./components/dataset-components/dataset-contactname-editor.component";
export {
    datasetContactInfoFormControl,
    DatasetContactInfoEditorComponent
} from "./components/dataset-components/dataset-contactinfo-editor.component";
export { datasetDoiFormControl, DatasetDoiEditorComponent } from "./components/dataset-components/dataset-doi-editor.component";
export { LicenseViewLinkComponent } from "./components/dataset-components/license-view-link.component";

export { ErrMsgComponent } from "./components/errmsg/errmsg.component";

export { NominationLicenseDialogComponent } from './components/nomination-license-dialog/nomination-license-dialog.component';
export { DatasetLicenseDialogComponent } from './components/dataset-components/dataset-license-dialog.component';

export { OutsideLinkComponent } from "./components/outside-link/outside-link.component";

export { LoadingSpinnerComponent } from "./components/spinner/loading-spinner.component";

export { ValidatedInputComponent } from "./components/validated-input/validated-input.component";
export { ValidatedTextareaComponent } from "./components/validated-textarea/validated-textarea.component";
export { ValidatedUrlComponent } from "./components/validated-url/validated-url.component";

export { DomainsService } from "./services/domains.service";
export { LicensesService } from "./services/licenses.service";
export { TagsService } from "./services/tags.service";
export { ModalDialogService, ModalConfiguration } from "./services/modal-dialog.service";

export { NominationLicenseDialogConfig } from './models/nomination-license-dialog-config.type';
export { NominationLicenseType } from './models/nomination-license-type.enum';
export { NominationStatus } from './models/nomination-status.enum';
export { DatasetNomination } from './models/dataset-nomination.type';
export { License } from './models/license.type';
export { LicenseEntryType, LicenseContentType } from './models/license-entry.type';
export { DatasetEdit, DatasetEditStatus } from './models/dataset-edit.type';

export {isEmptyValue} from "./utils/value-utils";
export {observeOnZone} from "./utils/observe-on-zone";
