import { Study, StudyManager } from '../core/StudyManager';
import { WebSocketServer } from './WebSocketServer';
import { Service } from '../core/Service';
import logger from '../util/logger';
import path from 'path';
import fs from 'fs';
import { spawn } from 'child_process';
import { LoggedEvent } from '../models';

interface Attachment {
    id: string;
    type: string;
    content: string;
    contentType: string;
}

export class LoggingWebSocketEndpoint extends Service {
    public constructor(ws: WebSocketServer, studyManager: StudyManager, private persistentPath: string) {
        super();

        ws.messages$.subscribe(async msg => {
            try {
                const study = await studyManager.getStudy(msg.study || 'outdated_study');

                switch (msg.channel) {
                    case 'sessions':
                        const session = await study.sessions.getSession(msg.data.sessionId);
                        session.update(msg.data);
                        break;
                    case 'entities':
                        study.entities.updateEntity(msg.data);
                        break;
                    case 'events':
                        study.events.addEvent(msg.data);
                        break;
                    case 'states':
                        study.states.addState(msg.data);
                        break;
                    case 'attachments-entities':
                        // FIXME: awful workaround due to timing issues...
                        setTimeout(() => this.addEntityAttachment(study, msg.id, msg.sessionId, msg.data), 1000);
                        break;

                    case 'attachments-events':
                        // FIXME: awful workaround due to timing issues...
                        setTimeout(() => this.addEventAttachment(study, msg.id, msg.sessionId, msg.data), 1000);
                        break;

                    default:
                        logger.error(`Unknown channel ${msg.channel}`);
                }
            } catch (e) {
                logger.error(e);
            }
        });
    }

    // TODO: this should be handled by EntityManager!
    private async addEntityAttachment(study: Study, entityId: string, sessionId: string, attachment: Attachment) {
        logger.debug('New attachment for entity ' + entityId);
        const entity = await study.entities.getEntity(sessionId, entityId);

        if (!entity.attachments) {
            entity.attachments = [];
        }

        if (attachment.type === 'rtsp') {
            const rtspUrl = attachment.content;
            const videoDir = path.resolve(path.join(this.persistentPath, sessionId, entityId));
            attachment.id += '.mp4';
            const videoFilePath = path.join(videoDir, attachment.id);
            fs.mkdir(videoDir, { recursive: true }, err => logger.error(err));

            // TODO: stops automatically after a few minutes/seconds since there's no mechanism to couple this with session start/stop yet
            const ffmpeg = spawn('ffmpeg', ['-rtsp_transport', 'tcp', '-i', rtspUrl, '-b', '900k', '-t', '30', '-acodec', 'copy', '-vcodec', 'copy', videoFilePath]);

            ffmpeg.stdout.on('data', data => logger.debug(data.toString()));
            ffmpeg.stderr.on('data', data => logger.error(data.toString()));

            // update attahcment with path to file for later export
            attachment.content = videoFilePath;
            attachment.type = 'video';
            attachment.contentType = 'persistent';
        }

        entity.attachments.push(attachment);

        // FIXME: due to possible timing issues that could override some newer values on entity, we create a new entity. could still cause timing issues...
        study.entities.updateEntity({
            entityId: entityId,
            sessionId: sessionId,
            attachments: entity.attachments
        });
    }

    private async addEventAttachment(study: Study, eventId: string, sessionId: string, attachment: Attachment) {
        logger.debug('New attachment for event ' + eventId);
        const event = await study.events.getEvent(sessionId, eventId) || { eventId, sessionId } as LoggedEvent;

        if (!event.attachments) {
            event.attachments = [];
        }

        event.attachments.push(attachment);

        // FIXME: due to possible timing issues that could override some newer values on entity, we create a new entity. could still cause timing issues...
        study.events.addEvent({
            eventId: eventId,
            eventType: event.eventType || 'screenshot',
            timestamp: event.timestamp || 0,
            sessionId: sessionId,
            attachments: event.attachments
        });
    }
}
