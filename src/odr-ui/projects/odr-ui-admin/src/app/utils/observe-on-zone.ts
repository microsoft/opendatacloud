import { Observable } from 'rxjs';
import { NgZone } from '@angular/core';

declare module 'rxjs' {
    // noinspection TsLint
    interface Observable<T> {
        observeOnZone: (zone: NgZone) => Observable<any>;
    }
}

// Ensures that the observable is observed within the Angular Zone.
Observable.prototype.observeOnZone = function (zone) {
    return Observable.create((observer) => {
        return this.subscribe({
            next: (value) => zone.run(() => observer.next(value)),
            error: (err) => zone.run(() => observer.error(err)),
            complete: () => zone.run(() => observer.complete()),
        });
    });
};
