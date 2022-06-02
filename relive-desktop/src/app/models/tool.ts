import { BehaviorSubject } from 'rxjs';
import { PlaybackSession } from './playback-session';
import { SharedEntity } from './shared-entity';
import { SharedEvent } from './shared-event';
import { Syncable } from './syncable';

export type ToolType = 'property' | 'angle' | 'distance' | 'trail' | 'frustum' | 'eventTimer' | 'camera';
export type ParameterType = 'bool' | 'color' | 'dropdown' | 'number';

/**
 * Contains data for synchronization with ReliveVR
 */
export interface ToolInfo {
    type: ToolType;
    name: string;
    minEvents: number;
    maxEvents: number;
    minEntities: number;
    maxEntities: number; // 0 -> Max = Min | -1 -> Infinite
    renderVisualization: boolean;
    parameters: { [key: string]: ToolParameter };
}

export interface ToolParameter {
    type: ParameterType;
    name: string;
    value: any;
}

export interface BoolToolParameter extends ToolParameter {
    type: 'bool';
    value: boolean;
}

export interface ColorToolParameter extends ToolParameter {
    type: 'color';
    value: string;
}

export interface NumberToolParameter extends ToolParameter {
    type: 'number';
    value: number;
    minValue?: number;
    maxValue?: number;
    stepSize?: number;
    unit?: string;
}
export interface DropdownToolParameter extends ToolParameter {
    type: 'dropdown';
    value: number;
    options: string[];
}

export type ToolInstance = {
    session: PlaybackSession;
    color: string;
};

export class Tool extends Syncable implements ToolInfo {
    public name: string;
    public renderVisualization: boolean;
    public parameters: { [key: string]: ToolParameter };

    public maxEvents: number;
    public minEvents: number;

    public maxEntities: number;
    public minEntities: number;

    public readonly onEntitiesChanged = new BehaviorSubject<number>(0); // FIXME: convert to JS event
    public readonly entities: SharedEntity[] = [];

    public readonly onEventsChanged = new BehaviorSubject<number>(0); // FIXME: convert to JS event
    public readonly events: SharedEvent[] = [];

    public readonly onInstancesChanged = new BehaviorSubject<number>(0); // FIXME: convert to JS event
    public readonly instances: ToolInstance[] = [];

    public isLoading: boolean;
    public isExpanded = true;
    public readonly data = new BehaviorSubject<any>(null);
    // FIXME: workaround to sync properly
    public notebookIndex: number;

    public constructor(public readonly type: ToolType, public readonly id: string) {
        super();
    }

    // see: https://stackoverflow.com/a/2117523/4090817
    private static uuidv4(): string {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            // tslint:disable-next-line: no-bitwise
            const r = Math.random() * 16 | 0;
            // tslint:disable-next-line: no-bitwise
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    public static fromTemplate(template: ToolInfo): Tool {
        const tool = new Tool(template.type, this.uuidv4());
        tool.name = template.name;
        tool.renderVisualization = template.renderVisualization;
        tool.parameters = template.parameters;
        tool.minEntities = template.minEntities;
        tool.maxEntities = template.maxEntities;
        tool.minEvents = template.minEvents;
        tool.maxEvents = template.maxEvents;
        return tool;
    }


    public update(): void {
        // FIXME: could be smarter
        this.onEntitiesChanged.next(0);
        this.onEventsChanged.next(0);
        this.onInstancesChanged.next(0);
        super.update();
    }
}
