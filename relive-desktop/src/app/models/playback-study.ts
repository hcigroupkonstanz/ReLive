import _ from 'lodash';
import { PlaybackSession } from './playback-session';
import { SharedEntity } from './shared-entity';
import { SharedEvent } from './shared-event';
import { Syncable } from './syncable';

const TAG_ACTIVE_SESSIONS = 'Active Sessions';
const TAG_INACTIVE_SESSIONS = 'Inactive Sessions';
const TAG_ALL_SESSIONS = 'All Sessions';

export class PlaybackStudy extends Syncable {
    public readonly sessions: PlaybackSession[] = [];
    public readonly sharedEntities: SharedEntity[] = [];
    public sharedEvents: SharedEvent[] = [];
    // prepopulated with 'system' tags
    public readonly tags: string[] = [ TAG_ALL_SESSIONS, TAG_ACTIVE_SESSIONS, TAG_INACTIVE_SESSIONS ];

    public isLoading = false;

    // TODO: name is currently used as ID
    public name: string;
    public displayName: string;
    public startTime?: number;
    public endTime?: number;
    public description?: string;

    public isActive: boolean;


    public constructor(data: any) {
        super();
        this.applyUpdate(data);
    }

    public getSession(sessionId: string): PlaybackSession {
        return _.find(this.sessions, s => s.sessionId === sessionId);
    }

    public getSessionsFromTag(tag: string): PlaybackSession[] {
        if (tag === TAG_ALL_SESSIONS) {
            return this.sessions;
        } else if (tag === TAG_ACTIVE_SESSIONS) {
            return _.filter(this.sessions, s => s.isActive);
        } else if (tag === TAG_INACTIVE_SESSIONS) {
            return _.filter(this.sessions, s => !s.isActive);
        } else {
            return _.filter(this.sessions, s => s.tags && s.tags.indexOf(tag) >= 0);
        }
    }

    public toJson(): any {
        return {
            name: this.name,
            // only sync properties that can actually change, ignore the rest
            isActive: this.isActive
        };
    }
}
