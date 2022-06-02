import { SessionManager } from './SessionManager';
import { StateManager } from './StateManager';
import { DatabaseClient } from './DatabaseClient';
import { Service } from './Service';
import { Collection } from 'mongodb';
import { EntityManager } from './EntityManager';
import { EventManager } from './EventManager';
import _ from 'lodash';


const META_DB = 'ReLive';
const DB_PREFIX = 'ReLive_';


export interface StudyInfo {
    name: string;
    dbName: string;

    // other metadata (for relive)
    [metadata: string]: any;
}
export interface Study {
    readonly info: StudyInfo;
    readonly entities: EntityManager;
    readonly states: StateManager;
    readonly events: EventManager;
    readonly sessions: SessionManager;
}

export class StudyManager extends Service {

    private _studies: { [studyName: string]: Study } = {};
    private studyCollection: Collection;
    private metaDb: DatabaseClient;

    public constructor(private dbUrl: string) {
        super();
        this.metaDb = new DatabaseClient(dbUrl, META_DB);
    }

    public async init() {
        await this.metaDb.init();
        this.studyCollection = this.metaDb.getCollection('studies');

        // retrieve all existing studies for API
        const studies = await this.studyCollection.find({}).toArray() as StudyInfo[];
        for (const study of studies) {
            this._studies[study.name] = await this.initStudy(study);
        }
    }

    private async initStudy(studyInfo: StudyInfo): Promise<Study> {
        const studyDb = new DatabaseClient(this.dbUrl, studyInfo.dbName);
        const study: Study = {
            info: studyInfo,
            sessions: new SessionManager(studyDb),
            entities: new EntityManager(studyDb),
            events: new EventManager(studyDb),
            states: new StateManager(studyDb),
        };

        await studyDb.init();
        await study.sessions.init();
        await study.entities.init();
        await study.events.init();
        await study.states.init();

        return study;
    }

    public getStudies(): StudyInfo[] {
        return _.map(this._studies, s => s.info);
    }

    public async getStudy(studyName: string, createIfMissing = true): Promise<Study> {
        let localStudy = _.find(this._studies, s => s.info.name.toLocaleLowerCase() === studyName.toLocaleLowerCase());

        if (!localStudy && createIfMissing) {
            console.log(`Unable to find study ${studyName}, creating new entry`);
            // create new study entry
            const studyInfo: StudyInfo = {
                name: studyName,
                // remove all possibly problematic characters for DB
                dbName: DB_PREFIX + studyName.toLowerCase().replace(/[^a-z0-9]/g, '_')
            };
            this.studyCollection.insertOne(studyInfo);

            localStudy = await this.initStudy(studyInfo);
            this._studies[studyName] = localStudy;
        }

        return localStudy;
    }
}
