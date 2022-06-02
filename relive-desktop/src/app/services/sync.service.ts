import { Injectable, NgZone } from '@angular/core';
import { Socket } from 'ngx-socket-io';
import _ from 'lodash';

interface ModelMessage {
    command: 'modelUpdate' | 'modelDelete';
    payload: any;
}

interface ModelRegistration {
    name: string;
    onUpdate: (modelData: any) => void;
    onDelete: (modelData: any) => void;
    registrationData?: any;
}

@Injectable({
    providedIn: 'root'
})
export class SyncService {

    constructor(
        private socket: Socket,
        private zone: NgZone) {

        // for debugging
        (window as any).SyncService = this;
    }

    public sendCommand(channel: string, valueType: 'bool', value: any): void {
        this.socket.emit(channel, {
            command: valueType,
            payload: value
        });
    }

    public registerModel(registration: ModelRegistration): Promise<void> {
        // receive updates
        this.socket.on(`${registration.name}Model`, (msg: ModelMessage) => {
            if (msg.command === 'modelUpdate') {
                registration.onUpdate(msg.payload);
            }
            if (msg.command === 'modelDelete') {
                registration.onDelete(msg.payload);
            }
        });

        // initial data fetch
        return new Promise<void>((resolve, reject) => {
            this.socket.once(`${registration.name}ModelInit`, (msg: ModelMessage) => {
                for (const data of msg.payload) {
                    registration.onUpdate(data);
                }
                resolve();
            });
            this.socket.emit(`${registration.name}Model`, { command: 'modelFetch', payload: registration.registrationData });
        });
    }

    public updateModel(name: string, model: any): void {
        this.socket.emit(`${name}Model`, {
            command: 'modelUpdate',
            payload: model
        });
    }

    public deleteModel(name: string, modelId: any): void {
        this.socket.emit(`${name}Model`, {
            command: 'modelDelete',
            payload: modelId
        });
    }
}

