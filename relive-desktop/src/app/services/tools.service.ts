import { PlaybackEvent } from './../models/playback-event';
import { ReliveService } from './relive.service';
import { SyncService } from './sync.service';
import { Tool, SharedEntity, PlaybackSession } from '../models/';
import { Injectable } from '@angular/core';
import _ from 'lodash';
import { SharedEvent } from '../models/shared-event';

const modelChannel = 'tools';

@Injectable({
    providedIn: 'root'
})
export class ToolsService {
    public readonly activeTools: Tool[] = [];

    constructor(private sync: SyncService, private relive: ReliveService) {
        // for debugging
        (window as any).ToolsService = this;
    }

    // should only be called from ReliveResolver!
    public initialize(): Promise<void> {
        return this.sync.registerModel({
            name: modelChannel,
            onUpdate: update => {
                const tool = _.find(this.activeTools, t => t.id === update.id);
                if (!tool) {
                    this.addRemoteTool(update);
                } else {
                    this.applyUpdate(tool, update);
                }
            },
            onDelete: deletedTool => {
                _.remove(this.activeTools, t => t.id === deletedTool.id);
            }
        });
    }

    public moveTool(tool: Tool, by: number): void {
        const index = this.activeTools.indexOf(tool);
        _.pull(this.activeTools, tool);
        const newIndex = Math.min(Math.max(0, index + by), this.activeTools.length);
        this.activeTools.splice(newIndex, 0, tool);

        for (let i = Math.min(index, newIndex); i < this.activeTools.length; i++) {
            const oldIndex = this.activeTools[i].notebookIndex;
            this.activeTools[i].notebookIndex = i;

            if (oldIndex !== this.activeTools[i].notebookIndex) {
                this.activeTools[i].update();
            }
        }
    }

    private applyUpdate(tool: Tool, update: any): void {
        tool.name = update.name;
        tool.renderVisualization = update.renderVisualization;
        tool.parameters = update.parameters;
        tool.isLoading = update.isLoading;
        tool.minEvents = update.minEvents;
        tool.maxEvents = update.maxEvents;
        tool.minEntities = update.minEntities;
        tool.maxEntities = update.maxEntities;
        tool.notebookIndex = update.notebookIndex;

        if (update.data !== undefined) {
            tool.data.next(update.data);
        }

        // apply entity updates
        const entities: SharedEntity[] = [];
        const study = this.relive.getActiveStudy();
        for (const e of update.entities || []) {
            if (e) {
                const entity = _.find(study.sharedEntities, { name: e });
                entities.push(entity);
            }
        }

        while (tool.entities.length > 0) {
            tool.entities.pop();
        }
        for (const e of entities) {
            tool.entities.push(e);
        }

        // apply event updates
        const events: SharedEvent[] = [];
        for (const e of update.events || []) {
            if (e) {
                const event = _.find(study.sharedEvents, { name: e });
                if (!event) {
                    let eventHack: PlaybackEvent = null;
                    for (const session of study.sessions) {
                        eventHack = _.find(session.events, x => x.eventId === e);
                        if (eventHack) {
                            break;
                        }
                    }

                    if (eventHack) {
                        events.push(new SharedEvent(eventHack));
                    }
                } else {
                    events.push(event);
                }
            }
        }

        while (tool.events.length > 0) {
            tool.events.pop();
        }
        for (const e of events) {
            tool.events.push(e);
        }

        // apply tool instances updates
        for (const instance of update.instances) {
            const localInstance = _.find(tool.instances, i => i.session.sessionId === instance.sessionId);
            if (localInstance) {
                if (localInstance.color !== instance.color) {
                    localInstance.color = instance.color;
                    tool.onInstancesChanged.next(0);
                }
            } else {
                tool.instances.push({
                    session: _.find(study.sessions, { sessionId: instance.sessionId }),
                    color: instance.color
                });
            }
        }
        // remove deleted instances
        _.remove(tool.instances, i => _.find(update.instances, ui => ui.sessionId === i.session.sessionId) === null);

        // reorder tools *without* changing array references...
        // TODO: could probably be simplified, but we don't properly sync the whole array if an index changes
        const sortedTools = _.sortBy(this.activeTools, t => t.notebookIndex);
        while (this.activeTools.length > 0) {
            this.activeTools.pop();
        }
        for (const st of sortedTools) {
            this.activeTools.push(st);
        }
    }

    private toJson(t: Tool): any {
        return {
            type: t.type,
            notebookIndex: t.notebookIndex,
            id: t.id,
            name: t.name,
            minEvents: t.minEvents,
            maxEvents: t.maxEvents,
            minEntities: t.minEntities,
            maxEntities: t.maxEntities,
            renderVisualization: t.renderVisualization,
            parameters: t.parameters,
            entities: t.entities.map(e => e.name),
            events: t.events.map(e => e.name),
            instances: t.instances.map(i => ({
                sessionId: i.session.sessionId,
                color: i.color
            }))
        };
    }

    private addRemoteTool(data: any): void {
        const tool = new Tool(data.type, data.id);
        this.applyUpdate(tool, data);
        this.activeTools.splice(tool.notebookIndex, 0, tool);
        // TODO: do we need to unregister, or is deleting the model enough for automatic GC?
        tool.registerUpdateHandler(() => this.sync.updateModel(modelChannel, this.toJson(tool)));
    }

    public async addTool(tool: Tool, index: number): Promise<void> {
        this.activeTools.splice(index, 0, tool);

        // FIXME: workaround to sync tool position without re-syncing entire array
        for (let i = index; i < this.activeTools.length; i++) {
            this.activeTools[i].notebookIndex = i;
            this.activeTools[i].update();
        }

        // TODO: do we need to unregister, or is deleting the model enough for automatic GC?
        tool.registerUpdateHandler(() => this.sync.updateModel(modelChannel, this.toJson(tool)));
        this.sync.updateModel(modelChannel, await this.toJson(tool));
    }

    public deleteTool(tool: Tool): void {
        this.sync.deleteModel('tools', { id: tool.id, name: tool.name });
        _.pull(this.activeTools, tool);
    }

    public clearTools(): void {
        while (this.activeTools.length > 0) {
            this.deleteTool(this.activeTools.pop());
        }
    }

    public getIconFromType(type: string): string {
        switch (type) {
            case 'distance':
                return 'square_foot';
            case 'property':
                return 'search';
            case 'angle':
                return 'text_rotation_angleup';
            case 'trail':
                return 'gesture';
            case 'frustum':
                return 'change_history';
            case 'eventTimer':
                return 'timer';
            case 'camera':
                return 'photo_camera';
        }

        return '';
    }
}
