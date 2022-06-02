import { LoggerService, ReliveService } from 'src/app/services';
import { Component, OnInit } from '@angular/core';
import { PlaybackEntity, PlaybackEvent } from '../../models';
import { DndDropEvent } from 'ngx-drag-drop';
import _ from 'lodash';
import { environment } from '../../../environments/environment';

@Component({
    selector: 'app-video-view',
    templateUrl: './video-view.component.html',
    styleUrls: ['./video-view.component.scss']
})
export class VideoViewComponent implements OnInit {

    videos = [];

    constructor(private reliveService: ReliveService, private logger: LoggerService) { }

    ngOnInit(): void {
        this.reliveService.events$.subscribe(ev => {
            for (const attachment of ev.attachments || []) {
                this.addAttachment(ev, attachment);
            }
        });

        for (const session of this.reliveService.getActiveStudy().sessions) {
            for (const entity of session.entities) {
                for (const attachment of entity.attachments || []) {
                    this.addAttachment(entity, attachment);
                }
            }

            for (const event of session.events) {
                for (const attachment of event.attachments || []) {
                    this.addAttachment(event, attachment);
                }
            }
        }
    }

    // FIXME: currently hardcoded to fetch from server
    private addAttachment(origin: PlaybackEntity | PlaybackEvent, attachment: any): void {
        const study = this.reliveService.getActiveStudy();
        const baseUrl = `${window.location.protocol}//${window.location.hostname}:${environment.restPort}`;
        if (attachment.type === 'video' && origin instanceof PlaybackEntity) {
            this.videos.push({
                id: attachment.id,
                attachment,
                session: this.reliveService.getSession(origin.sessionId),
                origin,
                source: baseUrl + `/studies/${study.name}/sessions/${origin.sessionId}/entities/${origin.entityId}/attachments/${attachment.id}`,
                type: 'persistent'
            });
        }
        // TODO: videos from events? (probably not necessary atm)
    }

    isDropDisabled(): boolean {
        // FIXME: ugly workaround to show disabled dropzones based on type
        const dragType = (window as any).dragtype as string;
        return dragType === 'entities' || dragType === 'event' || dragType === 'session-entity' || dragType === 'session-event';
    }

    onDrop(event: DndDropEvent): void {
        // FIXME: ugly workaround to prevent dropzone from going red after dropping last entity
        (window as any).dragdata = '';
        window.setTimeout(() => {
            (window as any).dragtype = '';
        }, 200);

        this.logger.log('states', {
            parentId: (window as any).dragid,
            stateType: 'event',
            dragType: 'drop',
            dropzone: 'renderstreaming'
        });

        switch (event.type) {
            case 'tag':
                for (const s of this.reliveService.getActiveStudy().getSessionsFromTag(event.data)) {
                    s.showVideos = true;
                    s.update();
                }
                break;

            case 'session':
                const session = _.find(this.reliveService.getActiveStudy().sessions, s => s.sessionId === event.data);
                session.showVideos = true;
                session.update();
                break;

            case 'event':
            case 'entity':
                break;

            default:
                console.warn('Unknown drag type:');
                console.debug(event);
        }

    }
}
