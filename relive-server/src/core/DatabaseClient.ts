import { MongoClient, Db, Collection, FilterQuery, ObjectID, GridFSBucket } from 'mongodb';
import { BehaviorSubject } from 'rxjs';
import { first } from 'rxjs/operators';
import logger from '../util/logger';
import { Service } from './Service';

export class DatabaseClient extends Service {
    // FIXME: workaround - should only call client.connect once!
    private static isConnected = new BehaviorSubject<boolean>(false);
    private static client: MongoClient;

    private grid: GridFSBucket;
    private db: Db;

    public constructor(connectionUri: string, private databaseName: string) {
        super();
        if (!DatabaseClient.client) {
            DatabaseClient.client = new MongoClient(connectionUri, {
                useUnifiedTopology: true
            });
            DatabaseClient.client.connect().then(() => DatabaseClient.isConnected.next(true));
        }
    }

    public async init() {
        // FIXME: workaround - should only call client.connect once!
        await DatabaseClient.isConnected.pipe(first(v => v)).toPromise();
        this.db = DatabaseClient.client.db(this.databaseName);
        this.grid = new GridFSBucket(this.db);
    }

    public getCollection(collectionName: string): Collection {
        return this.db.collection(collectionName);
    }

    public addFile(): string {
        const stream = this.grid.openUploadStream('test');
        logger.debug('start gridfs file open with id: ' + stream.id);
        stream.write('test', err => {
            if (err) {
                logger.error(err);
            }
            logger.debug('stream write');
        });
        stream.end(() => {
            logger.debug('stream end');
        });

        return stream.id.toString();
    }
}
