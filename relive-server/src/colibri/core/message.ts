export interface Message {
    group?: string;
    channel: string;
    command: string;
    payload: any;
    origin?: any;
}
