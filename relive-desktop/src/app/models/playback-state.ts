export class PlaybackState {

    public parentId: string;
    public sessionId: string;
    public stateType: 'entity' | 'event';
    public timestamp: number;
    public status?: 'active' | 'inactive' | 'deleted';
    public position?: { x: number, y: number, z: number };
    public rotation?: { x: number, y: number, z: number, w: number };
    public scale?: { x: number, y: number, z: number };
    public color?: string;

    public constructor(data: any) {
        for (const key of Object.keys(data)) {
            this[key] = data[key];
        }
    }

}
