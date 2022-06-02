import * as _ from 'lodash';
import { Service } from './service';

export class DataStore extends Service {
    public serviceName = 'DataStore';
    public groupName = 'core';

    private store: { [key: string]: any[] } = {};

    public constructor() {
        super();
    }

    public addModel(group: string, channel: string, id: number): void {
        const key = group + channel;
        if (!this.store[key]) {
            this.store[key] = [];
        }

        if (!this.getModel(group, channel, id)) {
            this.store[key].push({ id: id });
        }
    }


    public updateModel(group: string, channel: string, model: any): void {
        const key = group + channel;
        if (!this.store[key]) {
            this.store[key] = [];
        }

        const existingModel = this.getModel(group, channel, model.id);
        if (!existingModel) {
            this.store[key].push(model);
        } else {
            for (const k of Object.keys(model)) {
                existingModel[k] = model[k];
            }
        }
    }


    public removeModel(group: string, channel: string, id: any): void {
        const key = group + channel;
        if (this.store[key]) {
            _.remove(this.store[key], m => m.id === id);
        }
    }

    public clear(group: string, channel: string): void {
        delete this.store[group + channel];
    }

    public clearGroup(group: string): void {
        for (const key of Object.keys(this.store)) {
            if (key.startsWith(group)) {
                delete this.store[key];
            }
        }
    }

    public getModel(group: string, channel: string, id: number | string): any {
        const key = group + channel;
        if (this.store[key]) {
            return _.find(this.store[key], m => m.id === id);
        }
        return undefined;
    }

    public getAll(group: string, channel: string): any {
        const key = group + channel;
        return this.store[key] || [];
    }
}
