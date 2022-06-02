import { Syncable  } from './syncable';

export class PlaybackEvent extends Syncable {
    // only for type "critical incidents"
    public toolId?: string;

    public eventId: string;
    public name?: string;
    public entityIds?: string[];
    public sessionId: string;
    public eventType: string;
    public timestamp: number;
    public message?: string;
    public attachments: { id: string, type: 'video' | 'audio' | 'image' | 'model', content: string }[];

    public endTimestamp?: number;

    // Relive-specific attributees
    public timeStart: number;
    public timeEnd: number;

    // other metadata
    [x: string]: any;

    public constructor(data: any) {
        super();
        this.applyUpdate(data);
    }


    public toJson(): any {
        return {
            sessionId: this.sessionId,
            eventId: this.eventId,
            // only sync properties that can actually change, ignore the rest
            // TODO: might not be necessary for event?
        };
    }
}
