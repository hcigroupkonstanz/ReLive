import { WebServer } from './WebServer';
import { Router } from 'express';
import { from, forkJoin } from 'rxjs';
import logger from '../util/logger';
import fileUpload, { UploadedFile } from 'express-fileupload';
import fs from 'fs';
import _ from 'lodash';
import zip from 'adm-zip';
import path from 'path';
import { StudyManager } from '../core/StudyManager';
import { map, tap, timestamp } from 'rxjs/operators';
import zlib from 'zlib';
import Cache from 'streaming-cache';
import { Service } from '../core/Service';

export class RequestRestEndpoint extends Service {
    private stateCache = new Cache();

    public constructor(private webServer: WebServer, private studyManager: StudyManager, persistentFilePath: string) {
        super();

        webServer.use('/studies', Router()
            .get('/', async (req, res) =>  {
                res.status(200).json(await studyManager.getStudies());
            }));

        webServer.use('', Router()
            .get('/studies/:study/sessions', async (req, res) =>  {
                const study = await studyManager.getStudy(req.params.study);
                res.status(200).json(await study.sessions.listSessions());
            })
            .delete('/studies/:study/sessions/:sessionId', async (req, res) => {
                const studyName = req.params.study;
                const study = await studyManager.getStudy(studyName);

                const sessionId = req.params.sessionId;
                logger.info(`Deleting session ${sessionId} from study ${studyName}`);
                study.sessions.deleteSession(sessionId);
                study.entities.deleteSession(sessionId);
                study.events.deleteSession(sessionId);
                study.states.deleteSession(sessionId);

                res.status(200).end();
            })

            .get('/studies/:study/states', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);

                // GZip compression
                if (!this.stateCache.exists(req.params.study)) {
                    const gzipStream = zlib.createGzip();

                    gzipStream.pipe(this.stateCache.set(req.params.study));

                    // Udpate states to always contain *all* information from previous states
                    const statesDict = {};
                    let isFirst = true;
                    gzipStream.write('[\n');

                    study.states.streamStates().subscribe(state => {

                        if (!statesDict[state.sessionId]) {
                            statesDict[state.sessionId] = {};
                        }

                        if (!statesDict[state.sessionId][state.stateType + state.parentId]) {
                            state.speed = 0;
                            state.distanceMoved = 0;
                            statesDict[state.sessionId][state.stateType + state.parentId] = state;
                        } else {
                            var lastState = statesDict[state.sessionId][state.stateType + state.parentId];
                            if (lastState.position && state.position) {
                                // calculate speed & distanceMoved
                                const meters = Math.sqrt(
                                    Math.pow(state.position.x - lastState.position.x, 2) +
                                    Math.pow(state.position.y - lastState.position.y, 2) +
                                    Math.pow(state.position.z - lastState.position.z, 2));
                                const seconds = (state.timestamp - lastState.timestamp) / 1000;

                                state.distanceMoved = lastState.distanceMoved + meters;
                                if (seconds !== 0) {
                                    state.speed = (meters / seconds) || 0;
                                }
                            }

                            _.assign(statesDict[state.sessionId][state.stateType + state.parentId], state);
                        }

                        if (!isFirst) {
                            gzipStream.write(',\n');
                        }
                        isFirst = false;
                        gzipStream.write(JSON.stringify(statesDict[state.sessionId][state.stateType + state.parentId]));
                    }, err => {
                        logger.error(err);
                        res.status(500).end();
                    }, () => {
                        gzipStream.write('\n]');
                        gzipStream.flush();
                        gzipStream.end();
                    });
                }

                this.stateCache.get(req.params.study).pipe(res);
            })


            .get('/studies/:study/sessions/:sessionId/entities', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);
                res.status(200).json(await study.entities.getEntities(req.params.sessionId));
            })

            .get('/studies/:study/sessions/:sessionId/entities/:entityId', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);
                res.status(200).json(await study.entities.getEntity(req.params.sessionId, req.params.entityId));
            })

            .delete('/studies/:study/sessions/:sessionId/entities/:entityId', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);

                await study.entities.deleteEntity(req.params.sessionId, req.params.entityId);
                const deleted = await study.states.deleteEntity(req.params.sessionId, req.params.entityId);

                res.status(200).json({ status: 'success', deleted: deleted })
            })

            .get('/studies/:study/sessions/:sessionId/entities/:entityId/states', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);
                res.status(200).json(await study.states.getEntityStates(req.params.sessionId, req.params.entityId));
            })

            .delete('/studies/:study/sessions/:sessionId/entities/:entityId/attachments/:attachmentId', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);
                const entity = await study.entities.getEntity(req.params.sessionId, req.params.entityId);
                if (entity) {
                    const attachment = _.find(entity.attachments, a => a.id === req.params.attachmentId);
                    logger.debug(`Removing attachment ${attachment?.id} from ${entity.entityId}`);
                    _.pull(entity.attachments, attachment);

                    study.entities.updateEntity({
                        entityId: entity.entityId,
                        sessionId: entity.sessionId,
                        attachments: entity.attachments
                    });
                }

                res.status(200).end();
            })
            .get('/studies/:study/sessions/:sessionId/entities/:entityId/attachments/:attachmentId', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);
                const entity = await study.entities.getEntity(req.params.sessionId, req.params.entityId);
                if (entity) {
                    const attachment = _.find(entity.attachments, a => a.id === req.params.attachmentId);
                    if (attachment && attachment.contentType === 'persistent' && fs.existsSync(attachment.content)) {
                        if (attachment.type === 'video') {
                            // warning: terrible code ahead
                            const vidStreamer = require('./vid-streamer').settings({
                                rootFolder: path.dirname(attachment.content),
                                rootPath: ''
                            });
                            req.url = '//' + path.basename(attachment.content);
                            vidStreamer(req, res);
                        } else {
                            res.set({ 'Content-Disposition': `attachment; filename=${attachment.id}`, });
                            res.status(200);
                            // TODO: possible security issue..
                            fs.createReadStream(attachment.content).pipe(res);
                        }
                    } else {
                        res.status(404).end();
                    }
                } else {
                    res.status(404).end();
                }

            })
            .get('/studies/:study/sessions/:sessionId/states', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);
                res.status(200).json(await study.states.getStates(req.params.sessionId));
            })
            .get('/studies/:study/sessions/:sessionId/events', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);
                res.status(200).json(await study.events.getEvents(req.params.sessionId));
            })
            .get('/studies/:study/sessions/:sessionId/events/:eventId/states', async (req, res) => {
                const study = await studyManager.getStudy(req.params.study);
                res.status(200).json(await study.states.getEventStates(req.params.sessionId, req.params.eventId));
            })
            .get('/studies/:study/sessions/:sessionId/events/:eventId/attachments/:attachmentId', async (req, res) => {
                // NYI
                res.status(501).end();
            }));


        webServer.use('', Router()
            .get('/studies/:study/export/:sessionId', async (req, res) => {
                req.setTimeout(1000 * 60 * 60);
                const sessionId = req.params.sessionId;

                logger.debug(`Exporting session ${sessionId} from study ${req.params.study}`);
                const study = await studyManager.getStudy(req.params.study);

                forkJoin([
                    from(study.sessions.getSession(sessionId)).pipe(map(s => s.toJson())),
                    from(study.entities.getEntities(sessionId)),
                    from(study.states.getStates(sessionId)).pipe(tap(states => _.map(states, state => {
                        if (state.rotation && state.rotation.x === null) {
                            delete state.rotation;
                        }
                        if (state.status === null) {
                            state.status = 'active';
                        }
                        if (state.entityId === null) {
                            delete state.entityId;
                        }
                        return state;
                    }))),
                    from(study.events.getEvents(sessionId))
                ]).subscribe(results => {
                    if (req.params.study === 'imaxes') {
                        // TODO: minor workaround
                        results[0].startTime = _.minBy(results[2], state => state.timestamp).timestamp;
                        results[0].endTime = _.maxBy(results[2], state => state.timestamp).timestamp;
                    }

                    res.set({
                        'Content-Disposition': `attachment; filename=${sessionId}.json`,
                        'Content-Type': 'application/json'
                    });
                    res.json({
                        sessions: results[0],
                        entities: results[1],
                        states: results[2],
                        events: results[3]
                    });
                });
            })
        );




        webServer.use('/import', fileUpload({
            limits: { fileSize: 1024 * 1024 * 1024 },
            debug: false
        }));

        webServer.use('/import', Router()
            .post('', async (req, res) => {
                const studyName = req.body.studyName || 'outdated';
                const study = await studyManager.getStudy(studyName);
                logger.info(`Importing study ${studyName}`);

                const sessionFile = req.files.sessions as UploadedFile;

                if (sessionFile) {
                    try {
                        logger.debug('Got session file: ' + sessionFile.data);
                        for (const session of JSON.parse(sessionFile.data.toString())) {
                            logger.silly('Processing session: ' + JSON.stringify(session));
                            const s = await study.sessions.getSession(session.sessionId);
                            s.update(session);
                            s.dispose();
                        }

                        const entityFile = req.files.entities as UploadedFile;
                        for (const entity of JSON.parse(entityFile.toString())) {
                            logger.silly('Processing entity: ' + JSON.stringify(entity));
                            study.entities.updateEntity(entity);
                        }

                        const eventsFile = req.files.events as UploadedFile;
                        for (const event of JSON.parse(eventsFile.toString())) {
                            logger.silly('Processing events: ' + JSON.stringify(event));
                            study.events.addEvent(event);
                        }

                        const statesFile = req.files.states as UploadedFile;
                        for (const state of JSON.parse(statesFile.toString())) {
                            logger.silly('Processing state: ' + JSON.stringify(state));
                            study.states.addState(state);
                        }
                    } catch (e) {
                        logger.error(e);
                    }
                } else if (req.files.zip) {
                    const zipFile = req.files.zip as UploadedFile;
                    const zipStream = new zip(zipFile.data);
                    const zipEntries = zipStream.getEntries();

                    for (const zipEntry of zipEntries) {
                        logger.debug(`Processing zip entry ${zipEntry.entryName}`);

                        if (zipEntry.entryName.match(/entities.json/)) {
                            const entities = JSON.parse(zipEntry.getData().toString('utf8'));

                            for (const entity of entities) {
                                if (entity.attachments) {
                                    for (const attachment of entity.attachments) {
                                        if (attachment.contentType === 'file') {
                                            const contentDir = path.resolve(path.join(persistentFilePath, entity.sessionId, entity.entityId));
                                            const contentFileName = attachment.content;
                                            const contentFilePath = path.join(contentDir, contentFileName);

                                            const zipFilePath = path.join(entity.sessionId, 'entities', entity.entityId, attachment.content).replace(/\\/g, '/');
                                            logger.debug(`Reading zip attachment ${zipFilePath}`);

                                            fs.mkdirSync(contentDir, { recursive: true });
                                            fs.writeFileSync(contentFilePath, zipStream.getEntry(zipFilePath).getData());
                                            attachment.content = contentFilePath;
                                            // TODO: needs urgent refactoring
                                            attachment.contentType = 'persistent';
                                            logger.info(`imported entity attachment ${attachment.id} to ${contentFilePath}`);
                                        }
                                    }
                                }

                                study.entities.updateEntity(entity);
                            }
                            logger.info(`Imported ${entities.length} entities`);

                        } else if (zipEntry.entryName.match(/events.json/)) {

                            const events = JSON.parse(zipEntry.getData().toString('utf8'));

                            for (const event of events) {
                                if (event.attachments) {
                                    for (const attachment of event.attachments) {
                                        if (attachment.contentType === 'file') {
                                            const contentDir = path.resolve(path.join(persistentFilePath, event.sessionId, event.eventId));
                                            const contentFileName = attachment.content;
                                            const contentFilePath = path.join(contentDir, contentFileName);

                                            const zipFilePath = path.join(event.sessionId, 'events', event.entityId, attachment.content).replace(/\\/g, '/');
                                            logger.debug(`Reading zip attachment ${zipFilePath}`);

                                            fs.mkdirSync(contentDir, { recursive: true });
                                            fs.writeFileSync(contentFilePath, zipStream.getEntry(zipFilePath).getData());
                                            attachment.content = contentFilePath;
                                            // TODO: needs urgent refactoring
                                            attachment.contentType = 'persistent';
                                            logger.info(`imported event attachment ${attachment.id}`);
                                        }
                                    }
                                }

                                study.events.addEvent(event);
                            }
                            logger.info(`Imported ${events.length} events`);

                        } else if (zipEntry.entryName.match(/states.json/)) {

                            const states = JSON.parse(zipEntry.getData().toString('utf8'));

                            for (const state of states) {
                                study.states.addState(state);
                            }
                            logger.info(`Imported ${states.length} states`);

                        } else if (zipEntry.entryName.match(/sessions.json/)) {

                            const sessions = JSON.parse(zipEntry.getData().toString('utf8'));

                            for (const session of sessions) {
                                const s = await study.sessions.getSession(session.sessionId);
                                s.update(session);
                                s.dispose();
                            }
                            logger.info(`Imported ${sessions.length} sessions`);
                        }
                    }
                }
                res.status(200).end();
            })
        );


        webServer.use('/temp-attachment', fileUpload({
            limits: { fileSize: 1024 * 1024 * 1024 },
            debug: false
        }));

        webServer.use('/temp-attachment', Router()
            .post('', async (req, res) => {
                const study = await studyManager.getStudy(req.body.study);
                const entity = await study.entities.getEntity(req.body.sessionId, req.body.entityId);
                if (!entity) {
                    res.status(500).end();
                    return;
                }

                logger.info(`Importing attachment for ${study.info.name} on session ${entity.sessionId} for entity ${entity.entityId}`);

                if (!entity.attachments) {
                    entity.attachments = [];
                }
                const attachment: any = {
                    id: req.body.id,
                    type: req.body.type,
                    contentType: req.body.contentType
                };
                entity.attachments.push(attachment);

                if (req.files) {
                    const contentFile = req.files.contentFile as UploadedFile;
                    const contentDir = path.resolve(path.join(persistentFilePath, entity.sessionId, entity.entityId));
                    const contentFileName = req.body.id;
                    const contentFilePath = path.join(contentDir, contentFileName);
                    attachment.content = contentFilePath;
                    logger.debug(contentFilePath);

                    fs.mkdir(contentDir, { recursive: true }, err => { if (err) { logger.error(err); } });
                    contentFile.mv(contentFilePath);
                } else {
                    attachment.content = req.body.content;
                }

                // FIXME: due to possible timing issues that could override some newer values on entity, we create a new entity. could still cause timing issues...
                study.entities.updateEntity({
                    entityId: entity.entityId,
                    sessionId: entity.sessionId,
                    attachments: entity.attachments
                });

                res.status(200).end();
            })
        );
    }

    public async init() {
        if (process.env.AUTOLOAD_STUDY) {
            this.preloadStudy(process.env.AUTOLOAD_STUDY);
        }
    }

    private async preloadStudy(name: string): Promise<void> {
        logger.info('Automatically loading study ' + name);
        const study = await this.studyManager.getStudy(name);

        const gzipStream = zlib.createGzip();

        gzipStream.pipe(this.stateCache.set(name));

        // Udpate states to always contain *all* information from previous states
        const statesDict = {};
        let isFirst = true;
        gzipStream.write('[\n');

        study.states.streamStates().subscribe(state => {

            if (!statesDict[state.sessionId]) {
                statesDict[state.sessionId] = {};
            }

            if (!statesDict[state.sessionId][state.stateType + state.parentId]) {
                state.speed = 0;
                state.distanceMoved = 0;
                statesDict[state.sessionId][state.stateType + state.parentId] = state;
            } else {
                var lastState = statesDict[state.sessionId][state.stateType + state.parentId];
                if (lastState.position && state.position) {
                    // calculate speed & distanceMoved
                    const meters = Math.sqrt(
                        Math.pow(state.position.x - lastState.position.x, 2) +
                        Math.pow(state.position.y - lastState.position.y, 2) +
                        Math.pow(state.position.z - lastState.position.z, 2));
                    const seconds = (state.timestamp - lastState.timestamp) / 1000;

                    state.distanceMoved = lastState.distanceMoved + meters;
                    if (seconds !== 0) {
                        state.speed = (meters / seconds) || 0;
                    }
                }

                _.assign(statesDict[state.sessionId][state.stateType + state.parentId], state);
            }

            if (!isFirst) {
                gzipStream.write(',\n');
            }
            isFirst = false;
            gzipStream.write(JSON.stringify(statesDict[state.sessionId][state.stateType + state.parentId]));
        }, err => {
            logger.error(err);
        }, () => {
            gzipStream.write('\n]');
            gzipStream.flush();
            gzipStream.end();
            logger.info('Preload complete');
        });
    }
}
