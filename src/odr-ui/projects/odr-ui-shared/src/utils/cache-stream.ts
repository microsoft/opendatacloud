import { Observable, of } from "rxjs";
import { mergeMap, tap } from "rxjs/operators";

const defaultCacheDurationMs = 20 * 60 * 1000;

export function buildCacheStream<T>(
  fetchStream: Observable<T>,
  cacheDurationMs: number = defaultCacheDurationMs
) {
  let lastTime = 0;
  let cachedValue: T | undefined;

  return () =>
    of(cachedValue).pipe(
      mergeMap(value => {
        const currentTime: number = Date.now();
        if (value === undefined || currentTime - lastTime >= cacheDurationMs) {
          return fetchStream.pipe(
            tap(fetchedValue => {
              lastTime = currentTime;
              cachedValue = fetchedValue;
            })
          );
        } else {
          return of(cachedValue);
        }
      })
    );
}
