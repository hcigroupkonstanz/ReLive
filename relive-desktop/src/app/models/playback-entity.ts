import { Syncable } from './syncable';

export class PlaybackEntity extends Syncable {
    public entityId: string;
    public parentEntityId?: string;
    public sessionId: string;
    public entityType: string; // : 'object' | 'video' | 'audio';
    public name?: string;
    public space: 'world' | 'screen';
    public filePaths?: string[];
    public attachments: { id: string, type: 'video' | 'audio' | 'image' | 'model', startTime?: number, duration?: number }[];
    public attributes: string[];

    // Relive-specific attributees
    public isVisible = true;
    public timeStart: number;
    public timeEnd: number;

    // other metadata
    // [x: string]: any;

    public constructor(data: any) {
        super();
        this.applyUpdate(data);
    }

    public toJson(): any {
        return {
            sessionId: this.sessionId,
            entityId: this.entityId,
            // only sync properties that can actually change, ignore the rest
            isVisible: this.isVisible
        };
    }
}
