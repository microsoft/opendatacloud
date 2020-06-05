// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { BrowserModule } from '@angular/platform-browser';
import {APP_INITIALIZER, NgModule} from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { ViewsByDateVisualizationComponent } from './home/views-by-date-visualization.component';
import { DatasetsByDomainVisualizationComponent } from './home/datasets-by-domain-visualization.component';
import { DatasetsByLicenseVisualizationComponent } from './home/datasets-by-license-visualization.component';
import { ViewsByDatasetVisualizationComponent } from './home/views-by-dataset-visualization.component';
import { DeploymentsByDatasetVisualizationComponent } from './home/deployments-by-dataset-visualization.component';
import { ViewsByRegionVisualizationComponent } from './home/views-by-region-visualization.component';
import { SearchByDomainVisualizationComponent } from './home/search-by-domain-visualization.component';
import { SearchTermsVisualizationComponent } from './home/search-terms-visualization.component';
import { UIComponentsModule } from './ui-components.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { routeProvider, routeComponents } from './app.routes';
import { UnauthorizedGuard } from './utils/unauthorized.guard';
import { AuthorizedRequestInterceptor } from './utils/http-interceptors.util';
import { SharedLibModule } from 'odr-ui-shared';
import { ToasterModule } from 'angular2-toaster';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

import {
    AuthService,
    DatasetsService,
    NominationsService,
    BatchService,
    ReportsService
} from './services';
import {AppConfigurationService} from './app-configuration.service';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        ViewsByDateVisualizationComponent,
        DatasetsByDomainVisualizationComponent,
        DatasetsByLicenseVisualizationComponent,
        ViewsByDatasetVisualizationComponent,
        DeploymentsByDatasetVisualizationComponent,
        ViewsByRegionVisualizationComponent,
        SearchByDomainVisualizationComponent,
        SearchTermsVisualizationComponent,
        ...routeComponents
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule,
        routeProvider,
        UIComponentsModule,
        SharedLibModule,
        BrowserAnimationsModule,
        ToasterModule.forRoot(),
        BsDropdownModule.forRoot()
    ],
    providers: [
        AppConfigurationService,
        {
            provide: APP_INITIALIZER,
            useFactory: (appConfig: AppConfigurationService) => {
                return () => appConfig.loadConfig();
            },
            multi: true,
            deps: [AppConfigurationService]
        },
        {
            provide: 'AZURE_AD_CONFIG',
            useFactory: (appConfig: AppConfigurationService) => {
                return appConfig.current.azureAD;
            },
            deps: [AppConfigurationService]
        },
        {
            provide: 'API_BASE_URL',
            useValue: '/api/'
        },
        { provide: HTTP_INTERCEPTORS, useClass: AuthorizedRequestInterceptor, multi: true },
        AuthService,
        UnauthorizedGuard,
        DatasetsService,
        NominationsService,
        BatchService,
        ReportsService
    ],
    bootstrap: [AppComponent]
})
export class AppModule {
}
