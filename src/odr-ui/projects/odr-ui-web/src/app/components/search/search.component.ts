// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, OnDestroy, OnInit } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { License } from "odr-ui-shared";
import { Observable, Subscription, EMPTY } from "rxjs";
import { map, switchMap, tap, catchError, delay } from "rxjs/operators";
import { OdrService } from "../../shared/services";
import {
  Constraints,
  Dataset,
  DatasetDomainType,
  DatasetSearch,
  SortOrder
} from "../../shared/types";
import {
  sortOrderTextToValue,
  sortOrderValueToText
} from "../../shared/types/sort-order.enum";
import { scrollToTop } from "../../shared/utils/scroll-to-top";
import { ToasterService } from "angular2-toaster";

interface SearchParams {
  term?: string;
  domain?: string;
  filetypes?: string;
  license?: string;
  collapse?: string;
  sort?: "name" | "lastModified" | "featured";
  page?: number;
}

const CategoriesFlag = "c";
const FormatsFlag = "f";
const LicensesFlag = "l";

// const maxActiveFacets = 3;
// this.toasterService.pop('info', 'Note', `Maximum of ${maxActiveFacets} concurrent filters allowed.`);

function getPageNumber(page: any) {
  return Number(page) || 1;
}

function isTextFlagSet(current: string, flag: string): boolean {
  return (current || "").indexOf(flag.toLowerCase()) !== -1;
}

function toggleTextFlag(current: string, flag: string): string | undefined {
  const flagsSet = new Set<string>(current || "");
  const v = flag.toLowerCase();
  if (flagsSet.has(v)) {
    flagsSet.delete(v);
  } else {
    flagsSet.add(v);
  }
  return flagsSet.size > 0 ? Array.from(flagsSet).join("") : undefined;
}

// Search for data sets
@Component({
  selector: "app-search",
  styleUrls: ["./search.component.scss"],
  templateUrl: "./search.component.html"
})
export class SearchComponent implements OnInit, OnDestroy {
  searchForm: FormGroup;
  get searchFormControls() {
    return this.searchForm.controls as {
      term: FormControl;
      sortOrder: FormControl;
    };
  }
  searchResults: Observable<Dataset[]>;

  constraints = new Constraints();
  isSearchBusy: boolean = true;
  totalRecordCount: number = 0;
  canNextPage = false;
  canPreviousPage = false;
  SortOrders = SortOrder;
  get termLengthErrorMessage() {
    return `Search term cannot be more than ${this.constraints.termLength} characters long.`;
  }

  private subscription: Subscription;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private toasterService: ToasterService,
    private odrService: OdrService
  ) {
    this.searchForm = new FormGroup({
      term: new FormControl("", [
        Validators.maxLength(this.constraints.termLength)
      ]),
      sortOrder: new FormControl(SortOrder.featured)
    });

    this.searchResults = this.route.queryParams.pipe(
      tap(params => {
        scrollToTop();
        this.isSearchBusy = true;
        setTimeout(() => {
          this.searchFormControls.term.setValue(params.term || "", {
            emitEvent: false
          });
          this.searchFormControls.sortOrder.setValue(
            sortOrderTextToValue(params.sort),
            {
              emitEvent: false
            }
          );
        });
      }),
      switchMap(params => {
        const searchParams = this.getDatasetSearch(params);
        return this.odrService.getDatasetSearchResults(searchParams).pipe(
          tap(results => {
            const currentPage = getPageNumber(searchParams.page);
            this.isSearchBusy = false;
            this.totalRecordCount = results.recordCount || 0;
            this.canPreviousPage = currentPage > 1;
            this.canNextPage = currentPage < results.pageCount;
          }),
          map(results => results.value),
          catchError(err => {
            console.error(err);
            return EMPTY;
          })
        );
      })
    );
  }

  ngOnInit(): void {
    this.subscription = this.searchFormControls.sortOrder.valueChanges
      .pipe(
        tap(sort => {
          this.updateRoute({
            sort: sortOrderValueToText(Number(sort))
          });
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  get currentSearchTerm() {
    return this.currentSearchParams.term;
  }

  searchForTerm() {
    if (!this.searchFormControls.term.errors) {
      const term = this.searchFormControls.term.value
        ? this.searchFormControls.term.value
        : undefined;
      this.updateRoute({
        term
      });
    }
  }

  get isDomainsCollapsed() {
    return isTextFlagSet(this.currentSearchParams.collapse, CategoriesFlag);
  }

  get isFormatsCollapsed() {
    return isTextFlagSet(this.currentSearchParams.collapse, FormatsFlag);
  }

  get isLicensesCollapsed() {
    return isTextFlagSet(this.currentSearchParams.collapse, LicensesFlag);
  }

  toggleDomainsCollapsed() {
    this.updateRoute({
      collapse: toggleTextFlag(
        this.currentSearchParams.collapse,
        CategoriesFlag
      )
    }, false);
  }

  toggleFormatsCollapsed() {
    this.updateRoute({
      collapse: toggleTextFlag(this.currentSearchParams.collapse, FormatsFlag)
    }, false);
  }

  toggleLicensesCollapsed() {
    this.updateRoute({
      collapse: toggleTextFlag(this.currentSearchParams.collapse, LicensesFlag)
    }, false);
  }

  selectCategory(domain: DatasetDomainType) {
    if (domain.id === this.currentSearchParams.domain) {
      this.toasterService.pop(
        "info",
        "Note",
        `Already filtering on ${domain.id}`
      );
    } else {
      this.updateRoute({
        domain: domain.id
      });
    }
  }

  selectFormat(fileType: string) {
    const fileTypesSet = this.getSelectedFilesTypesSet();
    const value = fileType.trim().toLowerCase();
    if (fileTypesSet.has(value)) {
      this.toasterService.pop(
        "info",
        "Note",
        `Already filtering on ${value} format`
      );
    } else {
      fileTypesSet.add(value);
      this.updateRoute({
        filetypes: Array.from(fileTypesSet).join(",")
      });
    }
  }

  selectLicense(license: License) {
    if (license.id === this.currentSearchParams.license) {
      this.toasterService.pop(
        "info",
        "Note",
        `Already filtering on ${license.name}`
      );
    } else {
      this.updateRoute({
        license: license.id
      });
    }
  }

  private get currentSearchParams(): SearchParams {
    return this.route.snapshot.queryParams || {};
  }

  private getSelectedFilesTypesSet(searchParams?: SearchParams) {
    const { filetypes } = searchParams || this.currentSearchParams;
    const fileTypeList = (filetypes || "")
      .split(",")
      .map(t => t.trim().toLowerCase())
      .filter(t => t);
    return new Set<string>(fileTypeList);
  }

  get selectedFacetDomain() {
    return this.currentSearchParams.domain;
  }

  get selectedFacetFileTypes() {
    const fileTypesSet = this.getSelectedFilesTypesSet();
    return fileTypesSet.size === 0 ? undefined : Array.from(fileTypesSet);
  }

  get selectedFacetLicense() {
    return this.currentSearchParams.license;
  }

  get hasSelectedFacets() {
    const { domain, license, filetypes } = this.currentSearchParams;
    return Boolean(domain || license || filetypes);
  }

  removeFacetDomain() {
    this.updateRoute({
      domain: undefined
    });
  }

  removeFacetFileType(fileType: string) {
    const fileTypesSet = this.getSelectedFilesTypesSet();
    fileTypesSet.delete(fileType.trim().toLowerCase());
    this.updateRoute({
      filetypes:
        fileTypesSet.size > 0 ? Array.from(fileTypesSet).join(",") : undefined
    });
  }

  removeFacetLicense() {
    this.updateRoute({
      license: undefined
    });
  }

  clearFacets() {
    this.updateRoute({
      domain: undefined,
      filetypes: undefined,
      license: undefined
    });
  }

  selectResult(result: Dataset): void {
    this.router.navigate(["/datasets", result.id]);
  }

  onNextPage(): void {
    const page = getPageNumber(this.currentSearchParams.page) + 1;
    this.updateRoute({
      page
    });
  }

  onPreviousPage(): void {
    const page = getPageNumber(this.currentSearchParams.page) - 1;
    this.updateRoute({
      page
    });
  }

  private updateRoute(params: SearchParams, resetPage: boolean = true) {
    this.router.navigate(["datasets"], {
      queryParams: {
        ...this.route.snapshot.queryParams,
        ...(resetPage ? { page: undefined } : {}),
        ...params
      },
      replaceUrl: true
    });
  }

  private getDatasetSearch(params: SearchParams): DatasetSearch {
    const terms = (params.term || "").trim();
    const sortOrder = params.sort || "featured";
    const page = getPageNumber(params.page);

    const facets: any = {};
    if (params.domain) {
      facets.domainId = [params.domain];
    }
    if (params.filetypes) {
      const fileTypesSet = this.getSelectedFilesTypesSet(params);
      if (fileTypesSet.size > 0) {
        facets.fileTypes = Array.from(fileTypesSet);
      }
    }
    if (params.license) {
      facets.licenseId = [params.license];
    }

    return {
      terms,
      facets,
      sortOrder,
      page
    };
  }
}
