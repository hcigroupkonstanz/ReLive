import { DatabaseClient } from './DatabaseClient';
import { Collection } from 'mongodb';
import { Service } from './Service';
import logger from '../util/logger';
import { LoggedState } from '../models';
import { Observable, Subject } from 'rxjs';
import _ from 'lodash';


export class StateManager extends Service {

    private _collection: Collection;

    public constructor(private db: DatabaseClient) {
        super();
    }

    public async init() {
        this._collection = this.db.getCollection('states');
        await this._collection.createIndexes([
            {
                key:
                {
                    'timestamp': 1,
                    'parentId': 1,
                    'sessionId': 1,
                    'stateType': 1
                },
                unique: true
            }
        ]);
    }

    public async getEventStates(sessionId: string, eventId: string): Promise<LoggedState[]> {
        return await this._collection.find({ sessionId: sessionId, parentId: eventId, stateType: 'event' }).sort({ timestamp: 1 }).toArray();
    }

    public async getEntityStates(sessionId: string, entityId: string): Promise<LoggedState[]> {
        return await this._collection.find({ sessionId: sessionId, parentId: entityId, stateType: 'entity' }).sort({ timestamp: 1 }).toArray();
    }

    public async getStates(sessionId: string): Promise<LoggedState[]> {
        return await this._collection.find({ sessionId: sessionId }).toArray();
    }

    public async getEventStatesMeta(sessionId: string, eventId: string): Promise<any[]> {
        const pipeline = [{
            $match: {
                'sessionId': sessionId,
                'parentId': eventId,
                'stateType': 'event'
            }
        },
        {
            $group: {
                '_id': null,
                'sessionId': { '$first': '$sessionId' },
                'parentId': { '$first': '$parentId' },
                'timeStart': { $min: '$timestamp' },
                'timeEnd': { $max: '$timestamp' }
            }
        }];
        return await this._collection.aggregate(pipeline).sort({ timestamp: 1 }).toArray();
    }

    public async getAttributes(entityId: string): Promise<string[]> {
        const attributes = [];
        await this._collection.find({ parentId: entityId }).forEach(result => {
            for (const key of Object.keys(result)) {
                if (attributes.indexOf(key) < 0) {
                    attributes.push(key);
                }
            }
        });

        return attributes;
    }

    public async getEntityStatesMeta(sessionId: string, entityId: string): Promise<any[]> {
        const pipeline = [
            {
                '$match': {
                    'sessionId': sessionId,
                    'parentId': entityId,
                    'stateType': 'entity'
                }
            },
            {
                '$group': {
                    '_id': null,
                    'sessionId': { '$first': '$sessionId' },
                    'parentId': { '$first': '$parentId' },
                    'timeStart': { '$min': '$timestamp' },
                    'timeEnd': { '$max': '$timestamp' }
                }
            }
        ];
        return await this._collection.aggregate(pipeline).toArray();
    }

    public async addState(state: LoggedState) {
        try {
            delete state._id;
            const result = await this._collection.updateOne({
                'parentId': state.parentId,
                'sessionId': state.sessionId,
                'stateType': state.stateType,
                'timestamp': state.timestamp
            }, {
                '$set': state
            }, {
                upsert: true
            });

        } catch (e) {
            logger.error(e);
        }
    }

    public deleteSession(sessionId: string): void {
        this._collection.deleteMany({ sessionId: sessionId }).then(result => {
            logger.info(`Removed ${result.deletedCount} states`);
        });
    }

    public async deleteEntity(sessionId: string, entityId: string): Promise<number> {
        return new Promise((resolve, reject) => {
            this._collection.deleteMany({ sessionId: sessionId, parentId: entityId, stateType: 'entity' }).then(result => {
                logger.info(`Removed ${result.deletedCount} states for entity ${entityId}`);
                resolve(result.deletedCount);
            });
        })
    }

    public streamStates(): Observable<LoggedState> {
        const state$ = new Subject<LoggedState>();
        this._collection.find().forEach(result => {
            state$.next(result);
        }, () => {
            state$.complete();
        });

         return state$.asObservable();
    }
}
