import { WorkerMessage } from './worker-message';
import { Service } from './service';
import * as threads from 'worker_threads';
import * as cluster from 'cluster';
import { Subject } from 'rxjs';
import { LogLevel } from './log-message';

export abstract class WorkerServiceProxy extends Service {
    private threadWorker: threads.Worker;
    private clusterWorker: cluster.Worker;


    private readonly workerMessages = new Subject<WorkerMessage>();
    protected readonly workerMessages$ = this.workerMessages.asObservable();

    public constructor() {
        super();
    }

    protected initCluster(path: string, env?: any): void {
        cluster.setupMaster({
            exec: path
        });
        this.clusterWorker = cluster.fork(env);

        this.clusterWorker.on('online', () => {
            this.logInfo(`Cluster ${path} online`);
        });

        this.clusterWorker.on('close', () => {
            this.logInfo(`Cluster ${path} closed`);
        });

        this.clusterWorker.on('exit', () => {
            this.logInfo(`Cluster ${path} exited`);
        });

        this.clusterWorker.on('message', (data: WorkerMessage) => {
            if (data.channel === 'log') {
                this.handleLogMessage(data.content);
            } else {
                this.workerMessages.next(data);
            }
        });
    }

    protected initWorker(path: string, workerData?: any): void {
        this.threadWorker = new threads.Worker(path, { workerData: workerData });

        this.threadWorker.on('error', err => {
            this.logError(err.message + '\n' + err.stack, false);
        });

        this.threadWorker.on('online', () => {
            this.logInfo(`Worker ${path} online`);
        });

        this.threadWorker.on('close', () => {
            this.logInfo(`Worker ${path} closed`);
        });

        this.threadWorker.on('message', (data: WorkerMessage) => {
            if (data.channel === 'log') {
                this.handleLogMessage(data.content);
            } else {
                this.workerMessages.next(data);
            }
        });
    }

    protected postMessage(channel: string, content?: any) {
        const msg: WorkerMessage = {
            channel: channel,
            content: content
        };

        if (this.threadWorker) {
            this.threadWorker.postMessage(msg);
        } else if (this.clusterWorker) {
            this.clusterWorker.send(msg);
        }
    }

    private handleLogMessage(log: any): void {
        switch (log.level) {
            case LogLevel.Debug:
                this.logDebug(log.msg);
                break;

            case LogLevel.Info:
                this.logInfo(log.msg);
                break;

            case LogLevel.Warn:
                this.logWarning(log.msg);
                break;

            case LogLevel.Error:
                this.logError(log.msg, false);
                break;

            default:
                this.logWarning('Unknown log level!');
                this.logDebug(log.msg);
                break;
        }
    }
}
