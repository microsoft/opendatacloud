// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {AppInsights, TelemetryContext} from 'applicationinsights-js';
import {environment} from './environments/environment';
import {Observable, ReplaySubject} from 'rxjs';

interface UserSessionIds {
    userId?: string;
    sessionId?: string;
}

const emptyUserSessionIds: UserSessionIds = {
    userId: null,
    sessionId: null
};

const initializerSubject = new ReplaySubject<UserSessionIds>(1);

if (environment.appInsights.enableAppInsights) {

    // Initialization performed after AppInsights has finishing loading.
    AppInsights.queue.push(() => {
        // Unfortunately, AppInsights has not yet finished initializing itself.
        watchForUserSessionIds();
    });

    AppInsights.downloadAndSetup(environment.appInsights);

} else {

    setUserSessionIds(emptyUserSessionIds);

}

export const appInsightsContextStream: () => Observable<UserSessionIds> = () => {
    return initializerSubject.asObservable();
};


// Wait up to two seconds for App Insights to set the user and session ids.
async function watchForUserSessionIds() {
    let userSessionIds: UserSessionIds;
    for (let i = 0; i < 20; i++) {
        userSessionIds = getUserSessionIdsFromCookie();
        if (userSessionIds.sessionId) {
            break;
        }
        await new Promise(resolve => setTimeout(resolve, 100));
    }
    setUserSessionIds(userSessionIds);
}

function setUserSessionIds(userSessionIds: UserSessionIds) {
    setTimeout(() => {
        initializerSubject.next(userSessionIds);
        initializerSubject.complete();
    });
}

function getUserSessionIdsFromCookie(): UserSessionIds {
    return document.cookie
        .split(';')
        .reduce((data: any, cookie: string) => {
            const [key, value] = cookie.trim().split('=');
            switch (key) {
                case 'ai_user':
                    data.userId = value;
                    break;
                case 'ai_session':
                    data.sessionId = value;
                    break;
            }
            return data;
        }, {});
}
