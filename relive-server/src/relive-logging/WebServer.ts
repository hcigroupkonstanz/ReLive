import express, { Router, RequestHandler } from 'express';
import https from 'https';
import http from 'http';
import * as cors from 'cors';
import logger from '../util/logger';

export class WebServer {
    private app: express.Application;
    private isRunning = false;
    public readonly server: http.Server | https.Server;

    public constructor(private port: number, sslCerts?: { key: string, cert: string, ca: string }) {
        this.app = express();
        this.app.set('port', port);
        this.app.set('json spaces', 2);
        this.app.use(express.json({ limit: '8gb' }));
        this.app.use(cors.default());

        if (sslCerts && sslCerts.key) {
            this.server = https.createServer(sslCerts, this.app);
        } else {
            this.server = http.createServer(this.app);
        }
    }

    public start() {
        this.server.listen(this.port, () => {
            logger.info(`Web server listening on 0.0.0.0:${this.port}`);
            this.isRunning = true;
        });
    }

    public use(url: string, handler: Router | RequestHandler) {
        if (this.isRunning) {
            logger.error(`Could not add route ${url}: Server already running`);
        } else {
            this.app.use(url, handler);
        }
    }

}
