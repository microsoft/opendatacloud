// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {RouterModule} from '@angular/router';

import {HomeComponent} from './home/home.component';
import {DatasetsViewComponent} from './datasets/datasets-view.component';
import {DatasetEditComponent} from './datasets/dataset-edit.component';
import {DatasetEditFormComponent} from './datasets/dataset-edit-form.component';
import {NominationsViewComponent} from './nominations/nominations-view.component';
import {LogoutComponent} from './users/logout.component';
import {UnauthorizedGuard} from './utils/unauthorized.guard';
import {NominationEditComponent} from './nominations/nomination-edit.component';
import {NominationNextStepComponent} from './nominations/nomination-next-step.component';
import {OperationsViewComponent} from './operations/operations-view.component';

export const routeComponents = [
    HomeComponent,
    DatasetsViewComponent,
    DatasetEditComponent,
    DatasetEditFormComponent,
    NominationsViewComponent,
    NominationEditComponent,
    NominationNextStepComponent,
    OperationsViewComponent,
    LogoutComponent
];

export const routeProvider = RouterModule.forRoot([
    {path: '', component: HomeComponent, pathMatch: 'full', canActivate: [UnauthorizedGuard]},
    {path: 'datasets', component: DatasetsViewComponent, canActivate: [UnauthorizedGuard]},
    {path: 'datasets/:datasetId', component: DatasetEditComponent, canActivate: [UnauthorizedGuard]},
    {path: 'nominations', component: NominationsViewComponent, canActivate: [UnauthorizedGuard]},
    {path: 'nominations/:nominationId', component: NominationEditComponent, canActivate: [UnauthorizedGuard]},
    {path: 'operations', component: OperationsViewComponent, canActivate: [UnauthorizedGuard]},
    {path: 'logout', component: LogoutComponent},
    {path: '**', redirectTo: '/'}
]);
