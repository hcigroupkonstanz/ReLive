import { LoggedEntity } from '../models/logged-entity';
import { DatabaseClient } from './DatabaseClient';
import { Collection } from 'mongodb';
import { Service } from './Service';
import logger from '../util/logger';


export class EntityManager extends Service {

    private _collection: Collection;

    public constructor(private db: DatabaseClient) {
        super();
    }

    public async init() {
        this._collection = this.db.getCollection('entities');
        this._collection.createIndexes([
            {
                key:
                {
                    'entityId': 1,
                    'sessionId': 1
                },
                unique: true
            }
        ]);
    }

    public async getEntity(sessionId: string, entityId: string): Promise<LoggedEntity> {
        const entity =  await this._collection.findOne({ 'sessionId': sessionId, 'entityId': entityId });
        // TODO: not optimal....
        if (!entity) {
            this.updateEntity({
                entityId: entityId,
                sessionId: sessionId,
                entityType: 'object',
                space: 'world'
            });
            return await this._collection.findOne({ sessionId: sessionId, entityId: entityId });
        } else {
            return entity;
        }
    }

    public async getEntities(sessionId: string): Promise<LoggedEntity[]> {
        return await this._collection.find({ sessionId: sessionId }).toArray();
    }

    public async updateEntity(entity: LoggedEntity) {
        try {
            delete entity._id;
            const result = await this._collection.updateOne({
                'entityId': entity.entityId,
                'sessionId': entity.sessionId
            }, {
                '$set': entity
            }, {
                upsert: true
            });

            if (result) {
                logger.debug('Entity update: ' + entity.entityId);
            }
        } catch (e) {
            logger.error(e);
        }
    }

    public deleteSession(sessionId: string): void {
        // TODO: check for attachments & delete attachments on server
        this._collection.deleteMany({ sessionId: sessionId }).then(result => {
            logger.info(`Removed ${result.deletedCount} entities`);
        });
    }

    public deleteEntity(sessionId: string, entityId: string): void {
        this._collection.deleteOne({ sessionId: sessionId, entityId: entityId }).then(result => {
            logger.info(`Removed entity ${entityId} from session ${sessionId}`);
        });
    }

    public async listEntities(): Promise<LoggedEntity[]> {
        return await this._collection.find().toArray();
    }
}
