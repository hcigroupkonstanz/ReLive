export interface LoggedState {
    parentId: string;
    sessionId: string;
    stateType: 'entity' | 'event';
    timestamp: number;
    status?: 'active' | 'inactive' | 'deleted';
    position?: { x: number; y: number; z: number };
    rotation?: { x: number; y: number; z: number; w: number };
    scale?: { x: number; y: number; z: number };
    color?: string;

    // other metadata
    [metadata: string]: any;
}
