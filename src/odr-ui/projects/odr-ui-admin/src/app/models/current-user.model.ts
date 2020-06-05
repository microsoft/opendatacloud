// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


export interface CurrentUser {
  isAuthenticated: boolean;
  displayId?: string;
  name?: string;
  bearerToken?: string;
}
