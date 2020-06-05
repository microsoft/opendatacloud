// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/**
 * Represents a collection of files in a published dataset.
 */
export class Dataset {

    public created: Date;
    public description: string;
    public fileCount: number;
    public fileTypes: string[];
    public id: string;
    public licenseId: string;
    public licenseName: string;
    public licenseContentUri?: string;
    public modified: Date;
    public name: string;
    public tags: string[];
    public size: number;
    public zipFileSize: number;
    public gzipFileSize: number;
    public isCompressedAvailable: boolean;

    public datasetUrl?: string;

    public projectUrl?: string;

    public domain?: string;

    public domainId: string;

    public published?: Date;

    public version?: string;
    public contactName?: string;
    public contactInfo?: string;
    public digitalObjectIdentifier?: string;
    public isFeatured?: boolean;

    public isCurrentUserOwner?: boolean;

    constructor(fields?: Partial<Dataset>) {
      if(fields) {
        for(const [key, value] of Object.entries(fields)) {
          this[key] = value;
        }
      }
    }

    static createEmptyDataset(): Dataset {
      return new Dataset({
        created: new Date(),
        description: '',
        fileCount: 0,
        fileTypes: [],
        id: '',
        name: '',
        tags: [],
        size: 0,
        zipFileSize: 0,
        gzipFileSize: 0,
        isCompressedAvailable: false,
      });
    }
}
