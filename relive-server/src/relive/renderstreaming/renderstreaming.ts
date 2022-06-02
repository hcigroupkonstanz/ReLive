import * as express from 'express';
import { WebServer } from '../../relive-logging/WebServer';
import signaling from './signaling';
export interface Options {
    secure?: boolean;
    port?: number;
    keyfile?: string;
    certfile?: string;
    websocket?: boolean;
}

export class RenderStreaming {
    public app: express.Application;

    constructor(webserver: WebServer) {
        const prefix = '/renderstreaming';
        webserver.use(`${prefix}/signaling`, signaling);
        webserver.use(`${prefix}/protocol`, (req, res) => res.json({ useWebSocket: false }));
        // this.app = createServer();

        // if (this.options.websocket) {
        //     console.log(`start websocket signaling server ws://${this.getIPAddress()[0]}`)
        //     // Start Websocket Signaling server
        //     new WSSignaling(server);
        // }
    }
}
