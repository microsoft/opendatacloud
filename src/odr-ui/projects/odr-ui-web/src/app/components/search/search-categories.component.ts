// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, EventEmitter, Input, Output } from "@angular/core";
import { Observable } from "rxjs";
import { OdrService } from "../../shared/services";
import { DatasetDomainType } from "../../shared/types";

// Categories selector
@Component({
  selector: "app-search-categories",
  styleUrls: ["./search-categories.component.scss"],
  templateUrl: "./search-categories.component.html"
})
export class SearchCategoriesComponent {
  @Input() isCollapsed: boolean;
  @Input() tabindex: number;
  @Output() toggleCollapsed = new EventEmitter<boolean>();
  @Output() selectCategory = new EventEmitter<DatasetDomainType>();

  domainsList: Observable<DatasetDomainType[]>;

  constructor(private odrService: OdrService) {
    this.domainsList = this.odrService.getDomainsInUseByDatasets();
  }

  onToggleClick() {
    this.toggleCollapsed.emit(!this.isCollapsed);
  }

  onDomainSelected(domain: DatasetDomainType) {
    this.selectCategory.emit(domain);
  }
}
