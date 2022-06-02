import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.prod';

const loggingUrl = `${window.location.protocol === 'http:' ? 'ws' : 'wss'}://${window.location.hostname}:55211`;

@Injectable({
    providedIn: 'root'
})
export class LoggerService {
    private ws: WebSocket;
    private sessionId: string;

    // see: https://stackoverflow.com/a/2117523/4090817
    private static uuidv4(): string {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            // tslint:disable-next-line: no-bitwise
            const r = Math.random() * 16 | 0;
            // tslint:disable-next-line: no-bitwise
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }


    constructor() {
        // this.ws = new WebSocket(loggingUrl);
        // this.sessionId = environment.sessionName;
    }

    public log(channel: 'entities' | 'events' | 'states' | 'sessions' | 'attachments-entities', data: any): any {
        // disabled
        return data;
        

        // if (!this.sessionId) {
        //     return data;
        // }

        // if (channel === 'entities') {
        //     data.entityId = data.entityId || LoggerService.uuidv4();
        // } else if (channel === 'events') {
        //     data.eventId = data.eventId || LoggerService.uuidv4();
        //     data.source = 'web';
        // }

        // if (!data.timestamp) {
        //     data.timestamp = Date.now();
        // }

        // data.sessionId = this.sessionId;

        // this.ws.send(JSON.stringify({
        //     study: 'Relive',
        //     channel,
        //     data
        // }));

        // return data;
    }

}
