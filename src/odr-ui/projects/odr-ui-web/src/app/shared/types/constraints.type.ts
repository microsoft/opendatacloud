// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/**
 * Global constraints for model values.
 */
export class Constraints {
    /**
     * The maximum length for a facet (tags, licenses, extensions) value
     */
    public facetValue = 64;

    /**
     * The maximum number of facet values in a search request
     */
    public maxFacetValues = 10;

    /**
     * The maximum facets in a search request
     */
    public maxFacets = 4;

    /**
     * The plain text search criteria length
     */
    public termLength = 256;

    /**
     * The page size
     */
    public pageSize = 2;

    /**
     * The maximum length for long names (128)
     */
    public longName = 64;

    /**
     * The maximum length for medium names (64)
     */
    public medName = 64;

    /**
     * The maximum length for short names (32)
     */
    public shortName = 32;

    /**
     * The maximum length for a medium description (1024)
     */
    public medDescription = 1024;

    /**
     * The maximum file name length
     */
    public maxFileNameLength = 256;
}
