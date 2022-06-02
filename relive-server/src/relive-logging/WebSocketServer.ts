import ws from 'ws';
import { Subject } from 'rxjs';
import logger from '../util/logger';
import { WebServer } from './WebServer';

export interface WebSocketMessage {
    readonly study: string;
    readonly channel: string;
    readonly data: any;

    // only for attachments:
    readonly id?: string;
    readonly sessionId?: string;
}

export class WebSocketServer {
    private readonly messagesStream: Subject<WebSocketMessage> = new Subject<WebSocketMessage>();
    private readonly server: ws.Server;

    public get messages$() { return this.messagesStream.asObservable(); }

    public constructor(private webserver: WebServer) {
        this.server = new ws.Server({ server: webserver.server });
        this.server.on('connection', socket => this.handleConnection(socket));
    }

    private handleConnection(socket: ws): void {
        socket.on('message', messageText => {
            try {
                this.messagesStream.next(JSON.parse(messageText.toString()));
            } catch (e) {
                logger.error(e);
            }
        });
    }
}
