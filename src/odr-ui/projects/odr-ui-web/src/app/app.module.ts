import { registerLocaleData } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import en from '@angular/common/locales/en';
import { APP_INITIALIZER, ErrorHandler, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AngularEditorModule } from '@kolkov/angular-editor';
import { AgGridModule } from 'ag-grid-angular';
import { ToasterModule } from 'angular2-toaster';
import { Ng2CompleterModule } from 'ng2-completer';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ClipboardModule } from 'ngx-clipboard';
import { SharedLibModule, NominationLicenseDialogComponent } from 'odr-ui-shared';
import { AppConfigurationService } from './app-configuration.service';
import { GlobalErrorHandler } from './app.error-handler';
import { routeImport } from './app.routes';
import { AboutComponent } from './components/about/about.component';
import { AppLoginComponent } from './components/app/app-login.component';
import { AppComponent } from './components/app/app.component';
import { CategoriesComponent } from './components/categories/categories.component';
import { AzureDeployButtonComponent } from './components/dataset-detail/buttons/azure-deploy-button.component';
import { DownloadButtonComponent } from './components/dataset-detail/buttons/download-button.component';
import { DatasetDetailPropertiesComponent } from './components/dataset-detail/dataset-detail-properties.component';
import { DatasetDetailComponent } from './components/dataset-detail/dataset-detail.component';
import { DatasetDownloadDialogComponent } from './components/dataset-detail/dataset-download-dialog.component';
import { DatasetFileExplorerComponent } from './components/dataset-detail/dataset-file-explorer.component';
import { DatasetLoaderComponent } from './components/dataset-detail/dataset-loader.component';
import { ExportChooserComponent } from './components/dataset-detail/export-chooser.component';
import { LicenseAcceptanceComponent } from './components/dataset-detail/license-acceptance.component';
import { LicenseDialogComponent } from './components/dataset-detail/license-dialog.component';
import { PreviewDialogComponent } from './components/dataset-detail/preview-dialog.component';
import { DatasetEditFormComponent } from './components/dataset-edit/dataset-edit-form.component';
import { DatasetEditLoaderComponent } from './components/dataset-edit/dataset-edit-loader.component';
import { DatasetEditNavbarComponent } from './components/dataset-edit/dataset-edit-navbar.component';
import { DatasetEditComponent } from './components/dataset-edit/dataset-edit.component';
import { EditConfirmationDialogComponent } from './components/dataset-edit/edit-confirmation-dialog.component';
import { StorageTokenDialogComponent } from './components/dataset-edit/storage-token-dialog.component';
import { FaqComponent } from './components/faq/faq.component';
import { HomeComponent } from './components/home/home.component';
import { GeneralIssueComponent } from './components/issue/general-issue.component';
import { IssueComponent } from './components/issue/issue.component';
import { NominateComponent } from './components/nominate/nominate.component';
import { SearchComponent } from './components/search/search.component';
import { SearchCategoriesComponent } from './components/search/search-categories.component';
import { SearchFormatsComponent } from './components/search/search-formats.component';
import { SearchLicensesComponent } from './components/search/search-licenses.component';
import { ValidatedDateComponent } from './shared/components/validated-date/validated-date.component';
import { PromptLoginForUnauthGuard } from './shared/guards/prompt-login-for-unauth.guard';
import { CommaDelimitedPipe } from './shared/pipes/comma-delimited.pipe';
import { EnumPipe } from './shared/pipes/enum.pipe';
import { FileSizePipe } from './shared/pipes/filesize.pipe';
import { AuthService } from './shared/services/auth.service';
import { DatasetService } from './shared/services/dataset.service';
import { DatasetSchemaService } from './shared/services/dataset-schema.service';
import { OdrService } from './shared/services/odr.service';
import { AuthorizedRequestInterceptor, JsonErrorResponseInterceptor } from './shared/utils/http-interceptors.util';

registerLocaleData(en);

@NgModule({
  declarations: [
    AppComponent,
    DatasetDetailComponent,
    EnumPipe,
    FileSizePipe,
    CommaDelimitedPipe,
    LicenseDialogComponent,
    PreviewDialogComponent,
    SearchComponent,
    SearchCategoriesComponent,
    SearchFormatsComponent,
    SearchLicensesComponent,
    FaqComponent,
    NominateComponent,
    IssueComponent,
    GeneralIssueComponent,
    ValidatedDateComponent,
    ExportChooserComponent,
    AzureDeployButtonComponent,
    DownloadButtonComponent,
    AppLoginComponent,
    LicenseAcceptanceComponent,
    DatasetLoaderComponent,
    DatasetEditComponent,
    DatasetDetailPropertiesComponent,
    DatasetFileExplorerComponent,
    AboutComponent,
    HomeComponent,
    CategoriesComponent,
    DatasetDownloadDialogComponent,
    DatasetEditNavbarComponent,
    DatasetEditFormComponent,
    DatasetEditLoaderComponent,
    EditConfirmationDialogComponent,
    StorageTokenDialogComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    SharedLibModule,
    routeImport,
    AgGridModule.withComponents([]),
    ClipboardModule,
    BrowserAnimationsModule,
    BsDropdownModule.forRoot(),
    Ng2CompleterModule,
    ToasterModule.forRoot(),
    BsDatepickerModule.forRoot(),
    AngularEditorModule
  ],
  entryComponents: [
    LicenseDialogComponent,
    PreviewDialogComponent,
    DatasetDownloadDialogComponent,
    NominationLicenseDialogComponent
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
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    { provide: 'ORIGIN_URL', useValue: location.origin },
    {
      provide: 'API_BASE_URL',
      useFactory: (appConfig: AppConfigurationService) => {
        return appConfig.current.apiBaseUrl;
      },
      deps: [AppConfigurationService]
    },
    {
      provide: 'AZURE_AD_CONFIG',
      useFactory: (appConfig: AppConfigurationService) => {
        return appConfig.current.azureAD;
      },
      deps: [AppConfigurationService]
    },
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizedRequestInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: JsonErrorResponseInterceptor, multi: true },
    AuthService,
    DatasetService,
    DatasetSchemaService,
    OdrService,
    PromptLoginForUnauthGuard
  ],
  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
