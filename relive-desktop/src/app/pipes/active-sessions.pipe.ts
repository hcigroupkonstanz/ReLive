import { PlaybackSession } from '../models/playback-session';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'activeSessions',
    pure: false
})
export class ActiveSessionsPipe implements PipeTransform {
    transform(sessions: PlaybackSession[], args?: any): PlaybackSession[] {
        if (!sessions) {
            return [];
        }
        return sessions.filter(s => s.isActive);
    }
}
