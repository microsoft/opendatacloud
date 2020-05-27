/**
 * Criteria for searching a dataset
 */
export interface DatasetSearch {

    /**
     * Gets or sets the search terms.
     */
    terms: string;

    /**
     * Gets or sets the facets for the search by facet name and allowed values. Facets include licenses, tags, etc.
     */
    facets: { [key: string]: string[] };

    /**
     * Gets or sets the sort order.
     */
    sortOrder: "name" | "lastModified" | "featured";

    /**
     * Gets or sets the page in the search results to be returned.
     */
    page: number;
}
