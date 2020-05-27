import { Component } from '@angular/core';
import { ReportsService } from '../services';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  constructor(private reportsService: ReportsService) {
  }

  viewsByDateData = this.reportsService.getViewsByDate();
  datasetsByDomainData = this.reportsService.getDatasetsByDomain();
  datasetsByLicenseData = this.reportsService.getDatasetsByLicense();
  viewByDatasetData = this.reportsService.getDatasetPageViews();
  datasetsAzureDeploymentsData = this.reportsService.getDatasetAzureDeployments();
  viewsByRegionData = this.reportsService.getViewsByRegion();
  searchByDomainData = this.reportsService.getSearchesByDomain();
  searchTermsData = this.reportsService.getSearchesByTerm();
}
