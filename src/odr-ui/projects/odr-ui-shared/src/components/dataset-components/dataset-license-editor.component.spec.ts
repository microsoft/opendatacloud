// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { CommonModule } from "@angular/common";
import { async, ComponentFixture, TestBed } from "@angular/core/testing";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import {
  DatasetLicenseEditorComponent,
  datasetLicenseFormControl
} from "./dataset-license-editor.component";
import { of, Observable } from "rxjs";
import { LicensesService } from "../../services/licenses.service";

function valueOrDefault<T>(value: T | undefined, defaultValue: T) {
  return value === undefined ? defaultValue : value;
}

describe("DatasetLicenseEditorComponent", () => {
  let fixture: ComponentFixture<DatasetLicenseEditorComponent>;

  const getComponent = (options: Partial<DatasetLicenseEditorComponent> = {}): DatasetLicenseEditorComponent => {
    fixture = TestBed.createComponent(DatasetLicenseEditorComponent);
    const component = fixture.componentInstance;

    component.control = valueOrDefault(options.control, datasetLicenseFormControl());
    component.size = valueOrDefault(options.size, 6);
    component.required = valueOrDefault(options.required, true);
    component.includeOtherLicense = valueOrDefault(options.includeOtherLicense, true);

    fixture.detectChanges();
    return component;
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [DatasetLicenseEditorComponent],
      imports: [CommonModule, FormsModule, ReactiveFormsModule],
      providers: [LicensesService]
    })
      .overrideComponent(DatasetLicenseEditorComponent, {
        set: {
          providers: [
            { provide: LicensesService, useClass: MockLicenseService }
          ]
        }
      })
      .compileComponents();
  }));

  it("should create", () => {
    const component = getComponent();
    expect(component).toBeDefined();
  });
});

class MockLicenseService {
  private static otherLicense = {
    id: "00000000-0000-0000-0000-000000000000",
    name: "Other License (must enter license info)",
    isOther: true
  };

  private static licenses = [
    {
      id: "51259f77-12fd-47b4-8efe-a5b3344767ae",
      name: "Community Data License Agreement – Permissive, Version 1.0",
      contentUri: "/licenses/51259f77-12fd-47b4-8efe-a5b3344767ae/file",
      isStandard: true,
      isFileBased: true,
      fileContentType: "application/pdf",
      fileName: "CDLA-Permissive-v1.0.pdf"
    },
    {
      id: "19c49d7e-16e9-4f2d-b914-439b0488cdbe",
      name: "Creative Commons",
      contentUri: "/licenses/19c49d7e-16e9-4f2d-b914-439b0488cdbe/file",
      isStandard: true,
      isFileBased: true,
      fileContentType: "text/plain",
      fileName: "CreativeCommonsLicense.txt"
    },
    {
      id: "2f933be3-284d-500b-7ea3-2aa2fd0f1bb2",
      name: "Legacy Microsoft Research Data License Agreement",
      contentUri: "/licenses/2f933be3-284d-500b-7ea3-2aa2fd0f1bb2/file",
      isStandard: true,
      isFileBased: true,
      fileContentType: "application/pdf",
      fileName: "MSR Open Data Research License May 2019 v1.pdf"
    },
    {
      id: "a889b26e-5149-4486-866e-ec896bb728c4",
      name: "Computational Use of Data Agreement v1.0",
      contentUri: "/licenses/a889b26e-5149-4486-866e-ec896bb728c4/file",
      isStandard: true,
      isFileBased: true,
      fileContentType: "application/pdf",
      fileName: "C-UDA-1.0.pdf"
    },
    {
      id: "f1f352a6-243f-4905-8e00-389edbca9e83",
      name: "Open Use of Data Agreement v1.0",
      contentUri: "/licenses/f1f352a6-243f-4905-8e00-389edbca9e83/file",
      isStandard: true,
      isFileBased: true,
      fileContentType: "application/pdf",
      fileName: "O-UDA-1.0.pdf"
    }
  ];

  public getLicensesList(): Observable<any[]> {
    return of([
      MockLicenseService.otherLicense,
      ...MockLicenseService.licenses
    ]);
  }
}
