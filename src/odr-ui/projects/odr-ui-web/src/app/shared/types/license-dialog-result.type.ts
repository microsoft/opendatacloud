// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/**
 * The configuration for license dialog results
 */
export class LicenseDialogResult {

    /**
     * Gets or sets the current user status.
     */
    public userAccept: boolean;

    /**
     * Gets or sets the current user's dataset usage reason.
     */
    public userReason: string;
}
