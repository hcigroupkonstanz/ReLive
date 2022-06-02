import { WorkerMessage } from './worker-message';
import { LogLevel } from './log-message';
import * as threads from 'worker_threads';
import { Subject } from 'rxjs';

export abstract class WorkerService {
    private readonly parentMessages = new Subject<WorkerMessage>();
    protected readonly parentMessages$ = this.parentMessages.asObservable();

    protected logDebug(msg: string): void {
        this.postMessage('log', {
            level: LogLevel.Debug,
            msg: msg
        });
    }

    protected logInfo(msg: string): void {
        this.postMessage('log', {
            level: LogLevel.Info,
            msg: msg
        });
    }

    protected logWarning(msg: string): void {
        this.postMessage('log', {
            level: LogLevel.Warn,
            msg: msg
        });
    }

    protected logError(msg: string, printStacktrace: boolean = true): void {
        if (printStacktrace) {
            msg += '\n' + new Error().stack;
        }

        this.postMessage('log', {
            level: LogLevel.Error,
            msg: msg
        });
    }


    protected postMessage(channel: string, content: any) {
        const msg: WorkerMessage = {
            channel: channel,
            content: content
        };

        if (this.useWorkers) {
            threads.parentPort.postMessage(msg);
        } else {
            process.send(msg);
        }
    }

    public constructor(private useWorkers: boolean) {
        if (useWorkers) {
            threads.parentPort.on('message', (msg: WorkerMessage) => {
                this.parentMessages.next(msg);
            });
        } else {
            process.on('message', (msg: WorkerMessage) => {
                this.parentMessages.next(msg);
            });
        }
    }
}
