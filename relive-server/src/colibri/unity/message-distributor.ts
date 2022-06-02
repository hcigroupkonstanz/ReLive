import { SocketIOServer } from '../web/socket-io-server';
import _ from 'lodash';
import { UnityServerProxy } from './unity-server-proxy';
import { Service, DataStore, Message } from '../core';
import logger from '../../util/logger';
import { merge } from 'rxjs';

export class MessageDistributor extends Service {
    public serviceName = 'MessageDistributor';
    public groupName = 'unity';

    public get messages$() {
        return merge(
            this.unityServer.messages$,
            this.socketioServer.messages$
        );
    }

    public constructor(private unityServer: UnityServerProxy, private store: DataStore, private socketioServer: SocketIOServer) {
        super();

        unityServer.messages$
            .subscribe(msg => {
                // FIXME: Workaround since models are now handled by relive directly
                if (!msg.channel || !msg.channel.endsWith('Model')) {
                    logger.debug(`Unity message on ${msg.channel}: ` + JSON.stringify(msg.payload));
                    socketioServer.broadcast(msg.channel, {
                        payload: msg.payload,
                        command: msg.command
                    });

                    unityServer.broadcast(msg, _.without(unityServer.currentClients, msg.origin))
                }
            });

        socketioServer.messages$
            .subscribe(msg => {
                if (!msg) {
                    (<any>msg) = {  };
                }

                // FIXME: Workaround since models are now handled by relive directly
                if (!msg.channel || !msg.channel.endsWith('Model')) {
                    unityServer.broadcast({
                        group: msg.group,
                        channel: msg.channel,
                        command: msg.command,
                        payload: msg.payload
                    });

                    socketioServer.broadcast(msg.channel, {
                        command: msg.command,
                        payload: msg.payload
                    }, msg.origin);
                }
            });
    }

    public sendMessage(msg: Message, to: any) {
        delete msg.origin;
        this.unityServer.broadcast(msg, [ to ]);
        this.socketioServer.emit(msg.channel, {
            command: msg.command,
            payload: msg.payload
        }, to);
    }

    public broadcast(msg: Message) {
        // remove clients, since they can't be cloned and cause issues when trying to serialize the message
        const origin = msg.origin;
        delete msg.origin;

        this.unityServer.broadcast(msg, _.without(this.unityServer.currentClients, origin));
        this.socketioServer.broadcast(msg.channel, {
            command: msg.command,
            payload: msg.payload
        }, origin);
    }

}
