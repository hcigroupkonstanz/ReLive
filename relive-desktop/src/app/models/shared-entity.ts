import { PlaybackEntity } from './playback-entity';

/**
 * Session-independent entity that may exist across multiple session. Extracted / merged based on name of entity across all sessions
 */
export class SharedEntity {
    public readonly name: string;
    public readonly entities: PlaybackEntity[] = [];

    public constructor(entity: PlaybackEntity) {
        this.name = entity.name;
        this.entities.push(entity);
    }
}
