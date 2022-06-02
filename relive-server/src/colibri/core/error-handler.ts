import { Service } from './service';

export class ErrorHandler extends Service {
    private static _instance: ErrorHandler;

    public serviceName = 'Internal Error';
    public groupName = 'core';

    static initialize() {
        this._instance = new ErrorHandler();
    }

    public static logInfo(msg: string) {
        this._instance.logInfo(msg);
    }

    public static logDebug(msg: string) {
        this._instance.logDebug(msg);
    }

    public static logWarning(msg: string) {
        this._instance.logWarning(msg);
    }

    public static logError(msg: string) {
        this._instance.logError(msg);
    }
}

ErrorHandler.initialize();
