export enum LicenseContentType {
  Unknown = "Unknown",
  Standard = "Standard",
  HtmlText = "HtmlText",
  InputFile = "InputFile"
}

export interface LicenseEntryType {
  contentType: LicenseContentType;
  id?: string;
  name?: string;
  additionalInfoUrl?: string;
  contentHtml?: string;
  fileName?: string;
  fileInput?: File;
  errors?: {
    propertyName: string,
    errorCode: string,
    errorMessage: string
  }[];
  readOnly?: boolean;
}
