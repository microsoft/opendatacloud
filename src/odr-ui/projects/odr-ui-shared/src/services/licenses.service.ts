import { Injectable, Inject } from "@angular/core";
import { Observable } from "rxjs";
import { HttpClient } from "@angular/common/http";
import { buildCacheStream } from "../utils/cache-stream";
import { map } from "rxjs/operators";
import { License } from "../models/license.type";

@Injectable()
export class LicensesService {
  public static otherLicense = {
    id: "00000000-0000-0000-0000-000000000000",
    name: "Other License (must enter license info)",
    isOther: true
  };

  public static oudaAgreementLicense = {
    id: "f1f352a6-243f-4905-8e00-389edbca9e83",
    name: "Open Use of Data Agreement v1.0"
  };

  public static cudaAgreementLicense = {
    id: "a889b26e-5149-4486-866e-ec896bb728c4",
    name: "Computational Use of Data Agreement v1.0"
  };

  private licensesListCache: () => Observable<License[]>;
  private baseUrl: string;

  constructor(
    @Inject("API_BASE_URL") apiBaseUrl: string,
    private http: HttpClient
  ) {
    this.baseUrl = apiBaseUrl;

    const fetchStream = this.http.get(`${this.baseUrl}licenses/standard`).pipe(
      map((licenses: any[]) => [
        {
          contentUri: null,
          id: null,
          name: "Unknown License"
        },
        LicensesService.otherLicense,
        ...licenses
      ])
    );

    this.licensesListCache = buildCacheStream(fetchStream);
  }

  public getLicensesList(): Observable<License[]> {
    return this.licensesListCache();
  }
}
