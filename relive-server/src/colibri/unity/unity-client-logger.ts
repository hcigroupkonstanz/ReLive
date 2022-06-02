import { filter } from 'rxjs/operators';
import { Service } from '../core';
import { UnityServerProxy } from './unity-server-proxy';

export class UnityClientLogger extends Service {
    public get serviceName(): string { return 'UnityClientLogger'; }
    public get groupName(): string { return 'unity'; }

    public constructor(private unityServer: UnityServerProxy) {
        super();

        unityServer.messages$
            .pipe(filter(m => m.channel === 'log'))
            .subscribe(m => {
                switch (m.command) {

                case 'info':
                    this.logInfo(`[${m.origin.name}] ${m.payload}`);
                    break;

                case 'warning':
                    this.logWarning(`[${m.origin.name}] ${m.payload}`);
                    break;

                case 'error':
                    this.logError(`[${m.origin.name}] ${m.payload}`, false);
                    break;

                case 'debug':
                default:
                    this.logDebug(`[${m.origin.name}] ${m.payload}`);
                    break;
                }
            });
    }
}
