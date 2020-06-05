// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { NominationLicenseType } from "./nomination-license-type.enum";

export enum DatasetEditStatus {
  /// The original dataset has not been modified.
  Unmodified = "unmodified",

  /// The details (metadata) of the dataset has been modified, but not yet saved.
  /// </summary>
  DetailsModified = "details-modified",

  /// The contents of the dataset are being updated (also implies "DetailsModified").
  ContentsModified = "contents-modified",

  /// The contents of the dataset are being imported.
  Importing = "importing"
}

export class DatasetEdit {
  id: string;
  name: string;
  description: string;
  datasetUrl: string;
  projectUrl?: string;
  tags?: string[];
  published?: Date;
  version?: string;
  domain?: string;
  domainId?: string;
  licenseId?: string;
  licenseName?: string;
  licenseType?: NominationLicenseType;
  licenseContentUri?: string;
  contactName?: string;
  contactInfo?: string;
  digitalObjectIdentifier?: string;
  otherLicenseAdditionalInfoUrl?: string;
  otherLicenseContentHtml?: string;
  otherLicenseFileName?: string;
  otherLicenseName?: string;
  otherLicenseFile?: File;
  isDownloadAllowed?: boolean;
  editStatus: DatasetEditStatus;
}
