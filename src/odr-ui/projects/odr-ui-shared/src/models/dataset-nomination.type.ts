// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { NominationStatus } from "./nomination-status.enum";
import { NominationLicenseType } from "./nomination-license-type.enum";

export class DatasetNomination {
  id?: string;
  datasetId?: string;
  name: string;
  description: string;
  datasetUrl: string;
  projectUrl?: string;
  tags?: string[];
  published?: Date;
  modified?: Date;
  modifiedByUserName?: String;
  modifiedByUserEmail?: String;
  created?: Date;
  createdByUserName?: String;
  createdByUserEmail?: String;
  version?: string;
  domain?: string;
  domainId?: string;
  licenseId?: string;
  licenseName?: string;
  contactName?: string;
  contactInfo?: string;
  nominationStatus?: NominationStatus;
  digitalObjectIdentifier?: string;
  otherLicenseAdditionalInfoUrl?: string;
  otherLicenseContentHtml?: string;
  otherLicenseFileName?: string;
  otherLicenseName?: string;
  otherLicenseFile?: File;
  nominationLicenseType?: NominationLicenseType;

  constructor(fields?: Partial<DatasetNomination>) {
    if (fields) {
      for (const [key, value] of Object.entries(fields)) {
        this[key] = value;
      }
    }
  }

  static createEmptyDatasetNomination(): DatasetNomination {
    return new DatasetNomination({
      created: new Date(),
      description: "",
      id: "",
      name: "",
      tags: []
    });
  }
}
