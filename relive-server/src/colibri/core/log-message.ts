export const enum LogLevel {
    Error,
    Warn,
    Info,
    Debug
}

export interface LogMessage {
    origin: string;
    level: LogLevel;
    message: string;
    group: string;
    created: Date;
}
