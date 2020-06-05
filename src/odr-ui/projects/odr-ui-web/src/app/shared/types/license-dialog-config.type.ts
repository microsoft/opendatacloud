// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { License } from 'odr-ui-shared';

// The configuration for license dialog
export class LicenseDialogConfig {

    // Gets or sets the unique identifier for the license.
    public license: License;

    // Gets or sets the current user status.
    public isAuthenticated: boolean;

    // User has accepted license for the dataset
    public acceptedLicense: boolean;

    // Gets or sets the current user's dataset usage reason.
    public userReason?: string;
}
