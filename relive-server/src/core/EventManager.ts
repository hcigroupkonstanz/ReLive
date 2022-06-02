import { DatabaseClient } from './DatabaseClient';
import { Collection } from 'mongodb';
import { LoggedEvent } from '../models';
import logger from '../util/logger';
import { Service } from './Service';

export class EventManager extends Service {
    private _collection: Collection;

    public constructor(private db: DatabaseClient) {
        super();
    }

    public async init() {
        this._collection = this.db.getCollection('events');

        await this._collection.createIndexes([
            {
                key:
                {
                    'eventId': 1,
                    'sessionId': 1
                },
                unique: true
            }
        ]);
    }

    public async getEvent(sessionId: string, eventId: string): Promise<LoggedEvent> {
        return await this._collection.findOne({ 'sessionId': sessionId, 'eventId': eventId });
    }

    public async getEvents(sessionId: string): Promise<LoggedEvent[]> {
        return await this._collection.find({ sessionId: sessionId }).sort({ timestamp: 1 }).toArray();
    }

    public async addEvent(event: LoggedEvent) {
        try {
            const result = await this._collection.updateOne({ eventId: event.eventId }, { '$set': event }, { 'upsert': true });
        } catch (e) {
            logger.error(e);
        }
    }

    public deleteSession(sessionId: string): void {
        // TODO: check for attachments & delete attachments on server
        this._collection.deleteMany({ sessionId: sessionId }).then(result => {
            logger.info(`Removed ${result.deletedCount} events`);
        });
    }


    public async listEvents(): Promise<LoggedEvent[]> {
        return await this._collection.find().toArray();
    }
}
