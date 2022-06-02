import _ from 'lodash';
import { BehaviorSubject } from 'rxjs';
import { PlaybackEntity } from './playback-entity';
import { PlaybackEvent } from './playback-event';
import { Syncable } from './syncable';

export class PlaybackSession extends Syncable {
    public readonly entities: PlaybackEntity[] = [];
    public readonly events: PlaybackEvent[] = [];

    public isActive = false;
    public showVideos = false; // TODO: should most likely be handled differently
    public color: string;

    public isEntitiesExpanded = false;
    public isEventsExpanded = false;
    public isAudioExpanded = false;
    public isVideoExpanded = false;

    // Logged study data
    public sessionId: string;
    public startTime?: number;
    public endTime?: number;
    public name?: string;
    public description?: string;
    public tags: string[];

    public eventFilters: {[key: string]: boolean} = {};
    // FIXME: behaviorsubject is not ideal, but i don't want to deal with events right now
    public readonly eventFilterChanged$ = new BehaviorSubject(0);
    public readonly remoteUpdate$ = new BehaviorSubject(0);

    public constructor(public readonly studyId: string, data: any) {
        super();
        this.applyUpdate(data);
    }

    public getEntity(entityId: string): PlaybackEntity {
        return _.find(this.entities, e => e.entityId === entityId);
    }

    public getEvent(eventId: string): PlaybackEvent {
        return _.find(this.events, e => e.eventId === eventId);
    }

    public applyUpdate(update: any): void {
        let hasFilterUpdate = false;
        for (const key of Object.keys(update.eventFilters)) {
            if (update.eventFilters[key] !== this.eventFilters[key]) {
                hasFilterUpdate = true;
                break;
            }
        }

        super.applyUpdate(update);

        if (hasFilterUpdate) {
            this.eventFilterChanged$.next(0);
        }
    }


    public toJson(): any {
        return {
            sessionId: this.sessionId,
            // only sync properties that can actually change, ignore the rest
            isActive: this.isActive,
            isEntitiesExpanded: this.isEntitiesExpanded,
            isEventsExpanded: this.isEventsExpanded,
            isAudioExpanded: this.isAudioExpanded,
            isVideoExpanded: this.isVideoExpanded,
            showVideos: this.showVideos,
            eventFilters: this.eventFilters
        };
    }
}
