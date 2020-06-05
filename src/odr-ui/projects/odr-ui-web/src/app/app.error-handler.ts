// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ErrorHandler, Injectable } from '@angular/core';
import { AppInsights } from 'applicationinsights-js';
import { environment } from '../environments/environment';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    handleError(err) {
        console.error(err);
        if (environment.appInsights.enableAppInsights) {
            AppInsights.trackException(err);
        }
    }
}
