export class Dataset {
    id?: string;
    name: string;
    description: string;
    datasetUrl?: string;
    projectUrl?: string;
    tags?: string[];
    fileTypes?: string[];
    fileCount?: number;
    domain?: string;
    domainId?: string;
    licenseId?: string;
    licenseName?: string;
    licenseContentUri?: string;
    published?: Date;
    modified?: Date;
    created?: Date;
    size?: number;
    zipFileSize?: number;
    gzipFileSize?: number;
    modifiedByUserName?: string;
    modifiedByUserEmail?: string;
    createdByUserName?: string;
    createdByUserEmail?: string;
    version?: string;
    digitalObjectIdentifier?: string;
    isFeatured?: boolean;
    datasetOwners?: {
      name: string;
      email: string;
    }[];
}
