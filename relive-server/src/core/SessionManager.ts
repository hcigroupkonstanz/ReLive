import { DatabaseClient } from './DatabaseClient';
import { LoggedSession, MistSession } from '../models';
import { Collection } from 'mongodb';
import _ from 'lodash';
import { Service } from './Service';
import logger from '../util/logger';

export class SessionManager extends Service {
    private _collection: Collection;
    private _sessionCache: { [sessionId: string]: Promise<MistSession> } = {};

    public constructor(private db: DatabaseClient) {
        super();
    }

    public async init() {
        this._collection = this.db.getCollection('sessions');
    }

    public async getSession(sessionId: string): Promise<MistSession> {
        if (this._sessionCache[sessionId] === undefined) {
            this._sessionCache[sessionId] = new Promise(async (resolve, reject) => {
                const dbSession = await this._collection.findOne({ sessionId: sessionId }) as LoggedSession;
                logger.debug(`Loading session ${sessionId} from DB, found: ${JSON.stringify(dbSession)}`);

                let session: MistSession = null;
                if (dbSession) {
                    session = new MistSession(dbSession);
                } else {
                    await this._collection.insertOne({ sessionId: sessionId });
                    session = new MistSession({
                        sessionId: sessionId,
                        color: ''
                    });
                }

                session.updates$.subscribe(() => {
                    this._collection.updateOne({ sessionId: session.data.sessionId }, { '$set': session.toJson() });
                    logger.debug(`Updating session ${session.data.name} (${sessionId})`);
                }, error => { }, () => {
                    delete this._sessionCache[sessionId];
                });

                resolve(session);
            });


        }
        return this._sessionCache[sessionId];
    }


    public deleteSession(sessionId: string): void {
        this._collection.deleteOne({ sessionId: sessionId }).then(async result => {
            if (this._sessionCache[sessionId] !== undefined) {
                const session = await this._sessionCache[sessionId];
                session.dispose();
                delete this._sessionCache[sessionId];
            }

            logger.info(`Removed ${result.deletedCount} sessions`);
        });
    }

    public async listSessions(): Promise<LoggedSession[]> {
        return await this._collection.find().toArray();
    }
}
