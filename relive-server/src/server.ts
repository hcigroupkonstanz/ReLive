import { ToolProcessor } from './relive/ToolProcessor';
import { DataStore } from './colibri';
import { StudyManager } from './core/StudyManager';
import { LoggingWebSocketEndpoint } from './relive-logging/LoggingWebSocketEndpoint';
import { RequestRestEndpoint } from './relive-logging/RequestRestEndpoint';
import fs from 'fs';
import dotenv from 'dotenv';
import { WebSocketServer } from './relive-logging/WebSocketServer';
import { WebServer } from './relive-logging/WebServer';
import logger from './util/logger';
import { Service } from './core/Service';
import * as colibri from './colibri';
import { merge } from 'rxjs';
import _ from 'lodash';
import { ReliveService } from './relive/ReliveService';

// Better TypeScript error messages
require('source-map-support').install();

// Unlimited stacktrace depth (due to RxJS)
Error.stackTraceLimit = Infinity;

/*
 * Setup Environment
 */
if (fs.existsSync('.env')) {
    // tslint:disable-next-line:no-console
    logger.debug('Using .env file to supply config environment variables');
    dotenv.config({ path: '.env' });
}

let sslCerts = null;
if (process.env.SSL_KEY_PATH) {
    sslCerts = {
        key: fs.readFileSync(process.env.SSL_KEY_PATH).toString(),
        cert: fs.readFileSync(process.env.SSL_CERT_PATH).toString(),
        ca: fs.readFileSync(process.env.SSL_CA_PATH).toString()
    };
}


/*
 * Servers
 */
const webServer = new WebServer(Number(process.env.REST_PORT) || 55211, sslCerts);
const ws = new WebSocketServer(webServer);

/*
 * Managers
 */
const studyManager = new StudyManager(process.env.MONGODB_URI);


/*
 * Endpoints
 */

const httpEndpoint = new RequestRestEndpoint(webServer, studyManager, process.env.PERSISTENT_FILE_PATH);
const wsEndpoint = new LoggingWebSocketEndpoint(ws, studyManager, process.env.PERSISTENT_FILE_PATH);

/*
 *  Colibri
 */

// Server
const colibriWebServer = new colibri.WebServer(Number(process.env.COLIBRI_WEB_PORT) || 55214, '/', sslCerts);
const unityServer = new colibri.UnityServerProxy();
const socketioServer = new colibri.SocketIOServer();


// API
const restApi = new colibri.RestAPI(process.env.PERSISTENT_FILE_PATH, colibriWebServer);

// Relive - Renderstreaming
import { RenderStreaming } from './relive/renderstreaming/renderstreaming';
const renderstreamingServer = new RenderStreaming(webServer);

// Plumbing
const messageDistributor = new colibri.MessageDistributor(unityServer, new DataStore(), socketioServer);
const unityLog = new colibri.UnityClientLogger(unityServer);

const reliveService = new ReliveService(studyManager, messageDistributor);
const toolProcessor = new ToolProcessor(reliveService, messageDistributor, studyManager);


const outputs = _.map(colibri.Service.Current, s => s.output$);
merge(...outputs).subscribe(log => logger.debug(log.message));

/*
 *  Boot up
 */
async function startup() {
    for (const service of Service.Current) {
        await service.init();
    }

    for (const service of colibri.Service.Current) {
        await service.init();
    }

    const httpServer = colibriWebServer.start();
    socketioServer.start(colibriWebServer.server);
    unityServer.start(Number(process.env.COLIBRI_UNITY_PORT) || 55212);

    webServer.start();
}

startup();
