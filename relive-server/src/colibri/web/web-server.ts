import https from 'https';
import http from 'http';
import * as path from 'path';
import express from 'express';
import * as bodyparser from 'body-parser';
import cors from 'cors';

import { Service } from '../core';

export class WebServer extends Service {

    public get serviceName(): string { return 'WebServer'; }
    public get groupName(): string { return 'web'; }

    public readonly server: http.Server | https.Server;

    private app: express.Application;
    private isRunning = false;

    public constructor(private webPort: number, private webRoot: string, sslCerts?: { key: string, cert: string, ca: string }) {
        super();

        console.log(`Web server listening on 0.0.0.0:${this.webPort}`);

        this.app = express();
        this.app.set('port', this.webPort);

        // handle POST data
        this.app.use(bodyparser.urlencoded({ extended: false }));
        this.app.use(bodyparser.json());

        // enable CORS
        this.app.use(cors());

        // set up default routes
        this.app.use(express.static(path.join(this.webRoot)));

        if (sslCerts && sslCerts.key) {
            console.log('Using HTTPS for colibri');
            this.server = https.createServer(sslCerts, this.app);
        } else {
            console.log('Using HTTP for colibri');
            this.server = http.createServer(this.app);
        }
    }

    public start(): http.Server {
        // add default route for 404s last
        this.app.use((req, res, next) => {
            res.sendFile(path.join(this.webRoot, 'index.html'));
            this.logWarning(`Unmatched route: ${req.path}`);
        });

        // lock server so no more route changes are allowed
        this.isRunning = true;

        // start server
        this.server.listen(this.webPort, () => {
            this.logInfo(`Web server listening on 0.0.0.0:${this.webPort}`);
        });

        return this.server;
    }

    public stop(): void {
        if (this.isRunning) {
            this.server.close();
            this.isRunning = false;
        }
    }

    public addRoute(url: string, requestHandler: express.RequestHandler | express.Router): void {
        if (this.isRunning) {
            this.logError(`Could not add route ${url}: Server already running`);
        } else {
            this.app.use(url, requestHandler);
        }
    }

    public addApi(url: string, requestHandler: express.RequestHandler | express.Router): void {
        this.addRoute(path.join('/api/', url).replace(/\\/g, '/'), requestHandler);
    }

}
