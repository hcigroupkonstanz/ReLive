export interface LoggedEntity {
    entityId: string;
    parentEntityId?: string;
    sessionId: string;
    entityType?: 'object' | 'video' | 'audio';
    name?: string;
    space?: 'world' | 'screen';
    attachments?: any[];

    // other metadata
    [metadata: string]: any;
}
