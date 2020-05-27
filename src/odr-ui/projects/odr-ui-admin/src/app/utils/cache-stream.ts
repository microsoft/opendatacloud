import { Observable, Subject } from 'rxjs';
import { mergeMap, isEmpty, shareReplay } from 'rxjs/operators';

const defaultCacheDurationMs = 20 * 60 * 1000;

export function buildCacheStream<T>(fetchStream: Observable<T>, cacheDurationMs: number = defaultCacheDurationMs) {

    let lastTime = 0;
    const requestSubject = new Subject<boolean>();

    const requestStream = requestSubject
        .asObservable()
        .pipe(
            mergeMap(() => {
                const currentTime: number = Date.now();
                if ((lastTime === 0) || (currentTime - lastTime) >= cacheDurationMs) {
                    lastTime = Date.now();
                    return fetchStream;
                } else {
                    return isEmpty();
                }
            }),
            shareReplay(1)
        );

    return () => {
        requestSubject.next(true);
        return requestStream;
    };
}
