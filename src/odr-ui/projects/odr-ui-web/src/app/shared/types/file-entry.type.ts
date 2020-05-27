/**
 * Represents a file or folder within a dataset
 */
export class FileEntry {

    /**
     * Gets or sets the type of file entry: file or folder.
     */
    public entryType: string;

    /**
     * Display Download Url in UI
     */
    public displayDownloadUrl: boolean;

    /**
     * Gets or sets download url for file.
     */
    public downloadUrl: string;

    /**
     * Gets or sets full folder path.
     */
    public fullFolderPath: string;

    /**
     * Gets or sets the name of the file.
     */
    public id: string;

    /**
     * Gets or sets the name of the file.
     */
    public name: string;

    /**
     * Gets or sets the parent folder name.
     */
    public parentFolder: string;

    /**
     * Gets or sets the content preview URL.
     */
    public previewUrl: string;
    public canPreview: boolean;

    /**
     * Gets or sets the length of the file.
     */
    public length: number;
}
