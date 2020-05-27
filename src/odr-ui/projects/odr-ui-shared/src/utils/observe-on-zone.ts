import { NgZone } from "@angular/core";
import { Observable, Observer } from "rxjs";

export function observeOnZone<T>(zone: NgZone) {
  return (source: Observable<T>) =>
    new Observable<T>((observer: Observer<T>) => {
      const subscription = source.subscribe({
        next: value => zone.run(() => observer.next(value)),
        error: err => zone.run(() => observer.error(err)),
        complete: () => zone.run(() => observer.complete())
      });
      return () => {
        subscription.unsubscribe();
      };
    });
}
