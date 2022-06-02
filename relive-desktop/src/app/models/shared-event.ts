import { PlaybackEvent } from './playback-event';

/**
 * Session-independent entity that may exist across multiple session. Extracted / merged based on name of entity across all sessions
 */
export class SharedEvent {
    public readonly name: string;
    public readonly events: PlaybackEvent[] = [];

    public constructor(event: PlaybackEvent) {
        this.name = event.name || event.eventId;
        this.events.push(event);
    }
}
