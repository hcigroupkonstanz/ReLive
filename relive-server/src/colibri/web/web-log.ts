import { Service, LogMessage } from '../core';
import { SocketIOServer } from './socket-io-server';
import { merge } from 'rxjs';
import * as _ from 'lodash';

interface WebMessage {
    origin: string;
    level: number;
    message: string;
    group: string;
    created: number;
}

export class WebLog extends Service {
    public serviceName = 'WebLog';
    public groupName = 'web';

    private logMessages: WebMessage[] = [];

    public constructor(private socketio: SocketIOServer) {
        super();
    }

    public init(): void {
        super.init();

        this.socketio.clientConnected$
            .subscribe(c => {
                for (const msg of this.logMessages) {
                    c.socket.emit('log', msg);
                }
            });

        const outputs = _.map(Service.Current, s => s.output$);
        merge(...outputs).subscribe(log => {
            while (this.logMessages.length > 1000) {
                this.logMessages.shift();
            }

            const webMsg: WebMessage = {
                origin: log.origin,
                level: log.level,
                group: log.group,
                message: log.message,
                created: log.created.getTime()
            };

            this.logMessages.push(webMsg);
            this.socketio.broadcast('log', webMsg);
        });
    }
}
