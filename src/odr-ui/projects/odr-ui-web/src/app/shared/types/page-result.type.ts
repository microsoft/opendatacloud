/**
 * Represents a paged data result
 */
export class PageResult<T> {

    /**
     * Gets or sets the total number of results available.
     */
    public recordCount?: number;

    /**
     * Gets or sets the total number of pages of results available.
     */
    public pageCount: number;

    /**
     * Gets or sets the data returned by the search.
     */
    public value: Array<T>;
}
