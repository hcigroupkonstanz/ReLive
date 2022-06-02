import { LoggedEntity, LoggedEvent, LoggedSession, LoggedState } from '../models';
import { MessageDistributor } from '../colibri';
import { Study, StudyInfo, StudyManager } from '../core/StudyManager';
import _ from 'lodash';
import { filter, first, flatMap, map, mergeMap, tap, toArray } from 'rxjs/operators';
import { Service } from '../core/Service';
import logger from '../util/logger';
import { BehaviorSubject, from } from 'rxjs';

const studyModel = 'studiesModel';
const sessionModel = 'sessionsModel';
const entityModel = 'entitiesModel';
const eventModel = 'eventsModel';
const stateModel = 'statesModel';

const modelUpdate = 'modelUpdate';
const modelDelete = 'modelDelete';
const modelFetch = 'modelFetch';
const modelInit = 'Init';
const modelInitCmd = 'modelInit';

export class ReliveService extends Service {
    public currentStudy: Study = null;

    public readonly onStudyReset = new BehaviorSubject<number>(0);
    
    private readonly studies: StudyInfo[] = [];

    // null means loading
    private readonly sessions$ = new BehaviorSubject<LoggedSession[]>(null);
    private readonly entities$ = new BehaviorSubject<LoggedEntity[]>(null);
    private readonly events$ = new BehaviorSubject<LoggedEvent[]>(null);

    private readonly notebook = {
        name: 'Initial Notebook',
        isPaused: true,
        playbackTimeSeconds: 1,
        playbackSpeed: 1,
        sceneView: 'FreeFly',
        sceneViewOptions: {
            'follow-entity-name': '',
            'iso-perspective': 'top-down'
        }
    };

    private logStudy: Study;
    private logSessionId: string;

    private sessionColorIndex = 0;
    private sessionColors = [
        '#00FFFF', // blue
        '#00FF00', // green
        '#FFFF00', // yellow
        '#FF0099', // pink
        '#9900FF', // purple
        '#09FBD3', // green2
        '#FC6E22', // orange
        '#009688', // teal
        '#ffc107', // amber
        '#3f51b5', // indigo
        '#cddc39', // lime
        '#d500f9', // purple 2
        '#76ff03', // light green
        '#f44336', // red
        '#03a9f4' // light blue
    ]


    public constructor(private studyManager: StudyManager, private messageDistributor: MessageDistributor) {
        super();
        if (process.env.NOTEBOOK_NAME) {
            this.notebook.name = process.env.NOTEBOOK_NAME;
        }

        this.logSessionId = process.env.SESSION_ID;
    }

    
    public async init() {
        this.logStudy = await this.studyManager.getStudy(process.env.STUDY_NAME);
        this.logStudy.entities.updateEntity({
            entityId: 'notebook',
            sessionId: this.logSessionId
        });

        for (const study of this.studyManager.getStudies()) {
            study.isActive = false;
            this.studies.push(study);
        }

        if (process.env.AUTOLOAD_STUDY) {
            const study = _.find(this.studies, s => s.name === process.env.AUTOLOAD_STUDY);
            this.loadStudy(study);
        }

        // notebook (TODO: move elsewhere??)
        this.messageDistributor.messages$.pipe(filter(msg => msg.channel === 'notebookModel')).subscribe(msg => {
            switch (msg.command) {
                case modelUpdate:
                    _.assign(this.notebook, msg.payload);
                    this.messageDistributor.broadcast(msg);

                    this.logStudy.states.addState({
                        parentId: 'notebook',
                        sessionId: this.logSessionId,
                        stateType: 'entity',
                        timestamp: Date.now(),

                        name: this.notebook.name,
                        isPaused: this.notebook.isPaused,
                        playbackTimeSeconds: this.notebook.playbackTimeSeconds,
                        playbackSpeed: this.notebook.playbackSpeed,
                        sceneView: this.notebook.sceneView,
                        sceneViewOptions: this.notebook.sceneViewOptions,
                    })
                    break;

                case modelDelete:
                    logger.warn('Unsupported delete on notebook');
                    break;

                case modelFetch:
                    this.messageDistributor.sendMessage({
                        channel: msg.channel + modelInit,
                        command: modelInitCmd,
                        payload: [ this.notebook ]
                    }, msg.origin);
                    break;
            }
        });

        // studies
        this.messageDistributor.messages$.pipe(filter(msg => msg.channel === studyModel)).subscribe(async msg => {
            switch (msg.command) {
                case modelUpdate:
                    const localStudy = _.find(this.studies, s => s.name === msg.payload.name);
                    if (!localStudy) {
                        logger.warn('Unsupported study creation');
                    } else if (process.env.AUTOLOAD_STUDY) {
                        logger.warn('Ignoring study change due to autoloaded study');
                    } else {
                        _.assign(localStudy, msg.payload);

                        // Study has been deactivated
                        if (!localStudy.isActive && this.currentStudy !== null && this.currentStudy.info.name === localStudy.name) {
                            logger.info(`Unloading study ${localStudy.name}`);
                            this.reset();
                        }

                        // study has been activated
                        if (localStudy.isActive) {
                            // if changed study is current study we don't need to do anything here

                            // current study is different - reload
                            if (this.currentStudy !== null && this.currentStudy.info.name !== localStudy.name) {
                                logger.warn(`Switching study from ${this.currentStudy.info.name} to ${localStudy.name} - clients probably need to reload`);
                                _.find(this.studies, { name: this.currentStudy.info.name }).isActive = false;
                                this.reset();
                            }

                            if (this.currentStudy === null) {
                                this.loadStudy(localStudy);
                            }
                        }
                    }

                    this.messageDistributor.broadcast(msg);

                    break;
                case modelDelete:
                    logger.warn('Unsupported delete on study');
                    break;
                case modelFetch:
                    this.messageDistributor.sendMessage({
                        channel: msg.channel + modelInit,
                        command: modelInitCmd,
                        payload: this.studies
                    }, msg.origin);
                    break;
                default:
                    logger.warn(`Unknown command '${msg.command}'`);
            }
        });


        // sessions
        this.messageDistributor.messages$.pipe(filter(msg => msg.channel === sessionModel)).subscribe(msg => {
            switch (msg.command) {

                case modelUpdate:
                    const localSession = _.find(this.sessions$.value, s => s.sessionId === msg.payload.sessionId);
                    if (localSession) {
                        _.assign(localSession, msg.payload);

                        const state = _.cloneDeep(localSession) as LoggedState;
                        state.parentId = 'Session_' + localSession.sessionId;
                        state.stateType = 'entity';
                        state.sessionId = this.logSessionId;
                        state.timestamp = Date.now();
                        this.logStudy.states.addState(state);

                    } else {
                        logger.warn('Adding sessions is unsupported. Tried to add:');
                        logger.warn(msg.payload);
                    }

                    this.messageDistributor.broadcast(msg);
                    break;

                case modelDelete:
                    logger.warn('Unsupported delete on sessions');
                    break;

                case modelFetch:
                    this.sessions$.pipe(first(s => s !== null)).subscribe(sessions => {
                        this.messageDistributor.sendMessage({
                            channel: msg.channel + modelInit,
                            command: modelInitCmd,
                            payload: sessions
                        }, msg.origin);
                    });
                    break;
                default:
                    logger.warn(`Unknown command '${msg.command}'`);
            }
        });



        // events
        this.messageDistributor.messages$.pipe(filter(msg => msg.channel === eventModel)).subscribe(async msg => {
            switch (msg.command) {

                case modelUpdate:
                    const localEvent = _.find(this.events$.value, e => e.eventId === msg.payload.eventId && e.sessionId === msg.payload.sessionId);
                    if (localEvent) {
                        logger.info('Updating event ' + localEvent.eventId);
                        _.assign(localEvent, msg.payload);
                    } else {
                        logger.info('Creating new event ' + msg.payload.eventId);
                        this.events$.value.push(msg.payload);
                    }

                    this.messageDistributor.broadcast(msg);
                    break;


                case modelDelete:
                    logger.info(`Removing event ${msg.payload.eventId} from session ${msg.payload.sessionId}`);
                    _.remove(this.events$.value, e => e.eventId === msg.payload.eventId && e.sessionId === msg.payload.sessionId);
                    this.messageDistributor.broadcast(msg);
                    break;


                case modelFetch:
                    this.events$.pipe(first(ev => ev !== null)).subscribe(events => {
                        this.messageDistributor.sendMessage({
                            channel: msg.channel + modelInit,
                            command: modelInitCmd,
                            payload: events
                        }, msg.origin);
                    });
                    break;
                default:
                    logger.warn(`Unknown command '${msg.command}'`);
            }
        });

        // entities
        this.messageDistributor.messages$.pipe(filter(msg => msg.channel === entityModel)).subscribe(async msg => {
            switch (msg.command) {
                case modelUpdate:
                    const localEntity = _.find(this.entities$.value, e => e.entityId === msg.payload.entityId && e.sessionId === msg.payload.sessionId);
                    if (localEntity) {
                        logger.info('Updating entity ' + localEntity.entityId);
                        _.assign(localEntity, msg.payload);
                    } else {
                        logger.info('Creating new entity ' + msg.payload.entityId);
                        this.entities$.value.push(msg.payload);
                    }

                    this.messageDistributor.broadcast(msg);
                    break;


                case modelDelete:
                    logger.warn(`Removing entity ${msg.payload.entityId} from session ${msg.payload.sessionId} (this might be an error?)`);
                    _.remove(this.entities$.value, e => e.entityId === msg.payload.entityId && e.sessionId === msg.payload.sessionId);
                    this.messageDistributor.broadcast(msg);
                    break;


                case modelFetch:
                    this.entities$.pipe(first(e => e !== null)).subscribe(entities => {
                        this.messageDistributor.sendMessage({
                            channel: msg.channel + modelInit,
                            command: modelInitCmd,
                            payload: entities
                        }, msg.origin);
                    });
                    break;
                default:
                    logger.warn(`Unknown command '${msg.command}'`);
            }
        });
    }


    public reset() {
        logger.info('reset');
        this.currentStudy = null;

        this.sessions$.next(null);
        this.entities$.next(null);
        this.events$.next(null);

        this.onStudyReset.next(1);
    }

    private getRandomColor(): string {
        const color = this.sessionColors[this.sessionColorIndex];
        this.sessionColorIndex = (this.sessionColorIndex + 1) % this.sessionColors.length;
        return color;
    }


    private async loadStudy(study: StudyInfo) {
        logger.info('Loading study ' + study.name);
        study.isActive = true;
        this.currentStudy = await this.studyManager.getStudy(study.name, false);

        // load sessions
        from(this.currentStudy.sessions.listSessions())
            .pipe(tap(sessions => _.map(sessions, s => {
                s.eventFilters = {
                    'click': true,
                    'criticalincidents': true,
                    'task': true,
                    'log': true,
                    'screenshot': true
                };

                if (!s.color) {
                    s.color = this.getRandomColor();
                }

                this.logStudy.entities.updateEntity({
                    entityId: 'Session_' + s.sessionId,
                    sessionId: this.logSessionId
                });
            })))
            .subscribe(v => this.sessions$.next(v));

        // load entities
        from(this.currentStudy.entities.listEntities()).pipe(
            flatMap(entities => entities),
            filter(entity => !!entity.sessionId),  // TODO: quick workaround for some datasets
            mergeMap(async entity => {
                this.logStudy.entities.updateEntity({
                    entityId: 'Entity_' + entity.sessionId + '_' + entity.entityId,
                    sessionId: this.logSessionId
                });

                // add attributes that can be inspected by property tool
                const attributes = ['speed', 'distanceMoved']
                const blacklist = ['_id', 'entityType', 'filePaths', 'space', 'highlightColor', 'timestamp', 'stateType', 'parentId', 'attachments'];
                for (const attribute of await this.currentStudy.states.getAttributes(entity.entityId)) {
                    if (!_.includes(blacklist, attribute)) {
                        attributes.push(attribute);
                    }
                }
                for (const attribute of Object.keys(entity)) {
                    if (!_.includes(blacklist, attribute)) {
                        attributes.push(attribute);
                    }
                }
                entity.attributes = attributes;

                const metadata = await this.currentStudy.states.getEntityStatesMeta(entity.sessionId, entity.entityId);
                if (metadata.length > 0) {
                    _.assign(entity, metadata[0]);
                }
                return entity;
            }),
            toArray(),
            map(entities => _.sortBy(entities, e => e.name))
        ).subscribe(v => {
            this.entities$.next(v);
            logger.info('Finished loading entities');
        });

        // load events
        from(this.currentStudy.events.listEvents()).pipe(
            flatMap(events => events),
            filter(event => !!event.sessionId),  // TODO: quick workaround for some datasets
            mergeMap(async event => {
                const metadata = await this.currentStudy.states.getEventStatesMeta(event.sessionId, event.eventId);
                if (metadata.length > 0) {
                    _.assign(event, metadata[0]);
                }

                return event;
            }),
            toArray()
        ).subscribe(v => {
            this.events$.next(v);
            logger.info('Finished loading events');
        });
    }
}