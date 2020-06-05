// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { NgModule } from '@angular/core';
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { AngularEditorModule } from '@kolkov/angular-editor';

import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { ModalModule } from 'ngx-bootstrap/modal';

import { AppVarDirective } from './components/app-var/app-var.directive';
import { DatasetNameEditorComponent } from "./components/dataset-components/dataset-name-editor.component";
import { DatasetFeaturedEditorComponent } from "./components/dataset-components/dataset-featured-editor.component";
import { DatasetDescriptionEditorComponent } from "./components/dataset-components/dataset-description-editor.component";
import { DatasetUrlEditorComponent } from "./components/dataset-components/dataset-url-editor.component";
import { ProjectUrlEditorComponent } from "./components/dataset-components/project-url-editor.component";
import { DatasetPublishedEditorComponent } from "./components/dataset-components/dataset-published-editor.component";
import { DatasetVersionEditorComponent } from "./components/dataset-components/dataset-version-editor.component";
import { DatasetDomainEditorComponent } from './components/dataset-components/dataset-domain-editor.component';
import { DatasetLicenseEditorComponent } from './components/dataset-components/dataset-license-editor.component';
import { DatasetTagsEditorComponent } from './components/dataset-components/dataset-tags-editor.component';
import { DatasetFileTypesEditorComponent } from "./components/dataset-components/dataset-filetypes-editor.component";
import { DatasetContactNameEditorComponent } from "./components/dataset-components/dataset-contactname-editor.component";
import { DatasetContactInfoEditorComponent } from "./components/dataset-components/dataset-contactinfo-editor.component";
import { DatasetDoiEditorComponent } from "./components/dataset-components/dataset-doi-editor.component";
import { DatasetLicenseDialogComponent } from './components/dataset-components/dataset-license-dialog.component';
import { LicenseViewLinkComponent } from './components/dataset-components/license-view-link.component';
import { ErrMsgComponent } from "./components/errmsg/errmsg.component";
import { NominationLicenseDialogComponent } from './components/nomination-license-dialog/nomination-license-dialog.component';
import { OutsideLinkComponent } from "./components/outside-link/outside-link.component";
import { ValidatedInputComponent } from "./components/validated-input/validated-input.component";
import { ValidatedTextareaComponent } from "./components/validated-textarea/validated-textarea.component";
import { ValidatedUrlComponent } from "./components/validated-url/validated-url.component";
import { LoadingSpinnerComponent } from "./components/spinner/loading-spinner.component";
import { DomainsService } from './services/domains.service';
import { LicensesService } from './services/licenses.service';
import { TagsService } from './services/tags.service';
import { ModalDialogService } from './services/modal-dialog.service';

const exportedComponents = [
    AppVarDirective,
    DatasetContactInfoEditorComponent,
    DatasetContactNameEditorComponent,
    DatasetDescriptionEditorComponent,
    DatasetDoiEditorComponent,
    DatasetFileTypesEditorComponent,
    DatasetNameEditorComponent,
    DatasetPublishedEditorComponent,
    DatasetUrlEditorComponent,
    DatasetVersionEditorComponent,
    DatasetLicenseDialogComponent,
    ProjectUrlEditorComponent,
    DatasetDomainEditorComponent,
    DatasetLicenseEditorComponent,
    DatasetTagsEditorComponent,
    DatasetFeaturedEditorComponent,
    LicenseViewLinkComponent,
    ErrMsgComponent,
    NominationLicenseDialogComponent,
    OutsideLinkComponent,
    ValidatedInputComponent,
    ValidatedTextareaComponent,
    ValidatedUrlComponent,
    LoadingSpinnerComponent
];

@NgModule({
    declarations: [
        ...exportedComponents
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        BsDatepickerModule.forRoot(),
        TypeaheadModule.forRoot(),
        ModalModule.forRoot(),
        AngularEditorModule
    ],
    exports: [
        ...exportedComponents
    ],
    entryComponents: [
      NominationLicenseDialogComponent,
      DatasetLicenseDialogComponent
    ],
    providers: [
        DomainsService,
        LicensesService,
        TagsService,
        ModalDialogService
    ]
})
export class SharedLibModule { }
