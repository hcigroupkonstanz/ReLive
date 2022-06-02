export interface LoggedEvent {
    eventId: string;
    entityIds?: string;
    sessionId: string;
    eventType: string;
    timestamp: number;
    message?: string;

    // other metadata
    [metadata: string]: any;
}
