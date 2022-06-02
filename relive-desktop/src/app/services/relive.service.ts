import { Notebook, PlaybackEntity, PlaybackEvent, PlaybackSession, PlaybackStudy, SharedEntity } from './../models';
import { Injectable } from '@angular/core';
import _ from 'lodash';
import { SyncService } from './sync.service';
import { interval, Subject } from 'rxjs';
import { SharedEvent } from '../models/shared-event';

/**
 * Provides large, static study data that is not directly synchronized via websockets with VR
 */
@Injectable({
    providedIn: 'root'
})
export class ReliveService {
    public readonly notebook = new Notebook();

    public readonly events$ = new Subject<PlaybackEvent>();
    public readonly studies: PlaybackStudy[] = [];
    private activeStudy: PlaybackStudy;

    private hasInitializedStudies: Promise<any>;

    constructor(private sync: SyncService) {
        // for debugging
        (window as any).ReliveService = this;
        this.loadStudies();

        this.notebook.registerUpdateHandler(() => this.sync.updateModel('notebook', this.notebook.toJson()));
        this.sync.registerModel({
            name: 'notebook',
            onUpdate: notebook => this.notebook.applyUpdate(notebook),
            onDelete: () => console.warn('Unsupported delete on notebook')
        });


        // advance notebook playbacktime internally
        const intervalMs = 100;
        interval(intervalMs).subscribe(() => {
            if (!this.notebook.isPaused) {
                const longestSession = _.maxBy(this.activeStudy.sessions, s => s.endTime - s.startTime);
                const maxPlaybackTime = (longestSession.endTime - longestSession.startTime) / 1000;
                this.notebook.playbackTimeSeconds += (intervalMs / 1000) * this.notebook.playbackSpeed;
                this.notebook.playbackTimeSeconds =  Math.min(this.notebook.playbackTimeSeconds, maxPlaybackTime);
            }
        });
    }

    private loadStudies(): void {
        this.hasInitializedStudies = this.sync.registerModel({
            name: 'studies',

            onUpdate: study => {
                // TODO: use proper study id??
                const localStudy = _.find(this.studies, s => s.name === study.name);

                if (!localStudy) {
                    // create new local study
                    const ps = new PlaybackStudy(study);
                    this.studies.push(ps);
                    ps.registerUpdateHandler(() => this.sync.updateModel('studies', ps.toJson()));
                } else {
                    // update local study
                    localStudy.applyUpdate(study);
                }
            },

            onDelete: deletedStudy => {
                // TODO: use proper study id??
                _.remove(this.studies, s => s.name === deletedStudy.name);
            }
        });
    }


    // should only be called from ReliveResolver!
    public async initializeRelive(studyId: string): Promise<void> {
        await this.hasInitializedStudies;

        const study = _.find(this.studies, s => s.name === studyId);
        if (study) {
            this.activeStudy = study;
            study.isLoading = true;
            study.isActive = true;
            study.update();

            await this.initializeSessions();
            await Promise.all([
                this.initializeEntities(),
                this.initializeEvents()
            ]);
            study.isLoading = false;
        }
    }

    public getActiveStudy(): PlaybackStudy {
        return this.activeStudy;
    }

    public getSession(sessionId: string): PlaybackSession {
        return this.activeStudy.getSession(sessionId);
    }

    public getEntity(sessionId: string, entityId: string): PlaybackEntity {
        const session = this.getSession(sessionId);
        return session.getEntity(entityId);
    }

    private initializeSessions(): Promise<void> {
        return this.sync.registerModel({
            name: 'sessions',

            registrationData: {
                name: this.activeStudy.name
            },

            onUpdate: session => {
                let localSession = _.find(this.activeStudy.sessions, s => s.sessionId === session.sessionId);

                if (!localSession) {
                    localSession = new PlaybackSession(session.sessionId, session);
                    this.activeStudy.sessions.push(localSession);
                    localSession.registerUpdateHandler(() => this.sync.updateModel('sessions', localSession.toJson()));
                } else {
                    localSession.applyUpdate(session);
                }

                // add new tags
                const tags = this.activeStudy.tags;
                for (const tag of localSession.tags || []) {
                    if (tags.indexOf(tag) < 0) {
                        tags.push(tag);
                    }
                }
            },

            onDelete: deletedSession => {
                _.remove(this.activeStudy.sessions, s => s.sessionId === deletedSession.sessionId);
            }
        });
    }


    private initializeEntities(): Promise<void> {
        return this.sync.registerModel({
            name: 'entities',

            onUpdate: entity => {
                const session = _.find(this.activeStudy.sessions, s => s.sessionId === entity.sessionId);
                if (!session) {
                    console.warn('No session found for event:');
                    console.warn(entity);
                    return;
                }

                const localEntity = _.find(session.entities, e => e.entityId === entity.entityId);

                if (!localEntity) {
                    const pe = new PlaybackEntity(entity);
                    session.entities.push(pe);
                    pe.registerUpdateHandler(() => this.sync.updateModel('entities', pe.toJson()));

                    const sharedEntity = _.find(this.activeStudy.sharedEntities, se => se.name === pe.name);
                    if (!sharedEntity) {
                        this.activeStudy.sharedEntities.push(new SharedEntity(pe));
                    } else {
                        sharedEntity.entities.push(pe);
                    }
                } else {
                    localEntity.applyUpdate(entity);
                    session.remoteUpdate$.next(0);
                }
            },

            onDelete: deletedEntity => {
                const session = _.find(this.activeStudy.sessions, s => s.sessionId === deletedEntity.sessionId);
                if (session) {
                    _.remove(session.entities, e => e.entityId === deletedEntity.entityId && e.sessionId === deletedEntity.sessionId);
                }
            }
        });
    }


    private initializeEvents(): Promise<void> {
        return this.sync.registerModel({
            name: 'events',

            onUpdate: event => {
                const session = _.find(this.activeStudy.sessions, s => s.sessionId === event.sessionId);
                if (!session) {
                    console.warn('No session found for event:');
                    console.warn(event);
                    return;
                }
                const localEvent = _.find(session.events, e => e.eventId === event.eventId);

                if (!localEvent) {
                    const pe = new PlaybackEvent(event);
                    session.events.push(pe);
                    pe.registerUpdateHandler(() => this.sync.updateModel('events', pe.toJson()));

                    if (pe.eventType === 'task') {
                        const sharedEvent = _.find(this.activeStudy.sharedEvents, se => se.name === pe.name);

                        if (!sharedEvent) {
                            this.activeStudy.sharedEvents.push(new SharedEvent(pe));
                            this.activeStudy.sharedEvents = _.sortBy(this.activeStudy.sharedEvents, e => e.name);
                        } else {
                            sharedEvent.events.push(pe);
                        }
                    }
                } else {
                    localEvent.applyUpdate(event);
                }

                this.events$.next(event);
            },

            onDelete: deletedEvent => {
                const session = _.find(this.activeStudy.sessions, s => s.sessionId === deletedEvent.sessionId);
                if (session) {
                    _.remove(session.events, e => e.eventId === deletedEvent.eventId && e.sessionId === deletedEvent.sessionId);
                }
            }
        });
    }
}
