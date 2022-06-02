import * as _ from 'lodash';
import { UNITY_SERVER_WORKER } from './unity-server-worker';
import { Message, WorkerServiceProxy } from '../core';
import { Subject } from 'rxjs';

export class UnityClient {
    public id: number;
    public group = '';
    public name = '';
}

export interface UnityMessage extends Message {
    origin: UnityClient;
}

export class UnityServerProxy extends WorkerServiceProxy {
    public serviceName = 'UnityServer';
    public groupName = 'unity';

    private clients: UnityClient[] = [];
    private clientStream = new Subject<UnityClient[]>();
    private clientAddedStream = new Subject<UnityClient>();
    private clientRemovedStream = new Subject<UnityClient>();

    private messageStream = new Subject<UnityMessage>();

    public get clients$() { return this.clientStream.asObservable(); }
    public get currentClients(): ReadonlyArray<UnityClient> { return this.clients; }
    public get clientsAdded$() { return this.clientAddedStream.asObservable(); }
    public get clientsRemoved$() { return this.clientRemovedStream.asObservable(); }
    public get messages$() { return this.messageStream.asObservable(); }


    public constructor() {
        super();
        this.initWorker(UNITY_SERVER_WORKER);

        this.workerMessages$.subscribe(msg => {
            switch (msg.channel) {
                case 'clientConnected$':
                    this.onClientConnected(msg.content.id);
                    break;

                case 'clientDisconnected$':
                    this.onClientDisconnected(msg.content.id);
                    break;

                case 'clientMessage$':
                    this.onClientMessage(msg.content.id, msg.content.packet);
                    break;
            }
        });
    }

    public start(port: number): void {
        this.postMessage('m:start', { port: port, });
        this.clientStream.next(this.clients);
    }

    public stop(): void {
        this.postMessage('m:stop');
    }

    public broadcast(msg: Message, clients: ReadonlyArray<UnityClient> = this.clients): void {
        this.postMessage('m:broadcast', {
            msg: msg,
            clients: _.map(clients, c => c.id)
        });
    }


    private onClientConnected(id: number) {
        const client = new UnityClient();
        client.id = id;

        this.clients.push(client);

        this.clientAddedStream.next(client);
        this.clientStream.next(this.clients);
    }

    private onClientDisconnected(id: number): void {
        const removedClients = _.remove(this.clients, c => c.id === id);
        for (const client of removedClients) {
            this.clientRemovedStream.next(client);
        }
        this.clientStream.next(this.clients);
    }

    private onClientMessage(id: number, msg: UnityMessage): void {
        const client = _.find(this.clients, c => c.id === id);
        if (client) {
            msg.origin = client;
            this.messageStream.next(msg);
        }
    }
}
