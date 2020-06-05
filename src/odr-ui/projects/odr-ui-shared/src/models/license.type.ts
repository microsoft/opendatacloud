// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export class License {
    id: string;
    name: string;
    contentUri?: string;
    isStandard?: boolean;
    isOther?: boolean;
    isFileBased?: boolean;
    fileContentType?: string;
    fileName?: string;
    fileUrl?: string;
}
