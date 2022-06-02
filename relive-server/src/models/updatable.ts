import { ReplaySubject, Observable } from 'rxjs';

export abstract class Updatable<T> {
    protected updates = new ReplaySubject<T>();
    public get updates$(): Observable<T> { return this.updates.asObservable(); }

    public dispose(): void {
        this.updates.complete();
    }

    public abstract toJson(): any;
}
