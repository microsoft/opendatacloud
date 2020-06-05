// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/**
 * Order of sorted items in search results.
 */
export enum SortOrder {

    /**
     * Sorts by the name
     */
    name = 1,

    /**
     * Sorts by the last modified time
     */
    lastModified = 2,

     /**
     * Sorts by featured datasets first
     */
    featured = 3
}

export function sortOrderValueToText(sortOrder: SortOrder) {
  switch(sortOrder) {
    case SortOrder.name:
      return "name";
    case SortOrder.lastModified:
      return "lastModified";
    default:
      return "featured";
  }
}

export function sortOrderTextToValue(sortOrder: string) {
  switch(sortOrder) {
    case "name":
      return SortOrder.name;
    case "lastModified":
      return SortOrder.lastModified;
    default:
      return SortOrder.featured;
  }
}
