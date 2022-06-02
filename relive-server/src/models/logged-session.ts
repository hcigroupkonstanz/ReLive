import { Observable, BehaviorSubject } from 'rxjs';
import logger from '../util/logger';
import { Updatable } from './updatable';

export interface LoggedSession {
    sessionId: string;
    startTime?: number;
    endTime?: number;
    name?: string;
    description?: string;
    color: string;

    // other metadata
    [metadata: string]: any;
}

export enum SessionStatus {
    ready, active, completed
}

export class MistSession extends Updatable<LoggedSession> {
    public readonly data: LoggedSession;

    private status = new BehaviorSubject<SessionStatus>(SessionStatus.ready);
    public get status$(): Observable<SessionStatus> { return this.status.asObservable(); }

    public constructor(session: LoggedSession) {
        super();
        this.data = session;
        this.updateStatus();

        // keep toJson working with mongoDB
        delete this.data['_id'];
    }

    private updateStatus(): void {
        if (this.data.endTime > 0 && this.data.startTime > 0) {
            logger.debug(`Status of session ${this.data.sessionId} now completed`);
            this.status.next(SessionStatus.completed);
            this.data['status'] = 'completed';
        } else if (this.data.startTime > 0) {
            logger.debug(`Status of session ${this.data.sessionId} now active`);
            this.status.next(SessionStatus.active);
            this.data['status'] = 'active';
        } else {
            logger.debug(`Status of session ${this.data.sessionId} now ready`);
            this.status.next(SessionStatus.ready);
            this.data['status'] = 'ready';
        }
    }


    public update(session: LoggedSession): void {
        const statusUpdateRequired =
            (session.startTime !== undefined && session.startTime !== this.data.startTime)
            || (session.endTime !== undefined && session.endTime !== this.data.endTime);

        // TODO: copy code from STREAM to update local session object
        for (const key of Object.keys(session)) {
            if (key !== '_id') {
                logger.silly(`Updating ${key} from ${this.data[key]} to ${session[key]}`);
                this.data[key] = session[key];
            }
        }

        if (statusUpdateRequired) {
            this.updateStatus();
        }

        this.updates.next(this.data);
    }

    public toJson(): LoggedSession {
        return this.data;
    }
}
