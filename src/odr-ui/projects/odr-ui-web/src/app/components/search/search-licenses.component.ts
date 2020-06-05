// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, EventEmitter, Input, Output } from "@angular/core";
import { Observable } from "rxjs";
import { OdrService } from "../../shared/services";
import { License } from "odr-ui-shared";
import { tap } from "rxjs/operators";

// Licenses selector
@Component({
  selector: "app-search-licenses",
  styleUrls: ["./search-licenses.component.scss"],
  templateUrl: "./search-licenses.component.html"
})
export class SearchLicensesComponent {
  @Input() isCollapsed: boolean;
  @Input() tabindex: number;
  @Output() toggleCollapsed = new EventEmitter<boolean>();
  @Output() selectLicense = new EventEmitter<License>();

  licensesList: Observable<License[]>;

  private currentLicenses: License[] = [];

  constructor(private odrService: OdrService) {
    this.licensesList = this.odrService.getLicenses().pipe(
      tap(licenses => {
        this.currentLicenses = licenses;
      })
    );
  }

  onToggleClick() {
    this.toggleCollapsed.emit(!this.isCollapsed);
  }

  onLicenseSelected(license: License) {
    this.selectLicense.emit(license);
  }

  lookupLicenseName(licenseId: string) {
    const license = this.currentLicenses.find(lic => lic.id === licenseId);
    return license ? license.name : licenseId;
  }
}
