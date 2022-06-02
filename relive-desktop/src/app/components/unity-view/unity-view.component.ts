import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';

import { VideoPlayer } from './video-player';
import { registerKeyboardEvents, registerMouseEvents } from './register-events.js';
import { DndDropEvent } from 'ngx-drag-drop';
import { LoggerService, ReliveService } from 'src/app/services';
import { Notebook, PlaybackSession, SceneViewType } from 'src/app/models';
import _ from 'lodash';

@Component({
    selector: 'app-unity-view',
    templateUrl: './unity-view.component.html',
    styleUrls: ['./unity-view.component.scss']
})
export class UnityViewComponent implements OnInit {
    @ViewChild('unityPlayer', { static: true }) unityPlayer: ElementRef<HTMLVideoElement>;
    isPlaying = false;
    notebook: Notebook;
    sessions: PlaybackSession[];
    private videoPlayer: VideoPlayer;

    constructor(private reliveService: ReliveService, private logger: LoggerService) {
        this.sessions = reliveService.getActiveStudy().sessions;
        this.notebook = reliveService.notebook;
    }

    ngOnInit(): void {
        window.addEventListener('resize', () => this.videoPlayer?.resizeVideo(), true);
    }

    getActiveSessions(): PlaybackSession[] {
        return _.filter(this.sessions, s => s.isActive);
    }

    sessionSelectionChanged(): void {
        console.log('update');
        this.notebook.update();
    }

    isFullscreen(): boolean {
        return !!document.fullscreenElement;
    }

    removeSession(session: PlaybackSession): void {
        session.isActive = false;
        session.update();
    }

    isDropDisabled(): boolean {
        // FIXME: ugly workaround to show disabled dropzones based on type
        const dragType = (window as any).dragtype as string;
        return dragType === 'event' || dragType === 'session-event';
    }

    toggleFullscreen(): void {
        if (!this.isFullscreen()) {
            if (this.unityPlayer.nativeElement.parentElement.requestFullscreen) {
                this.unityPlayer.nativeElement.parentElement.requestFullscreen();
            } else if ((this.unityPlayer.nativeElement as any).webkitRequestFullscreen) {
                (this.unityPlayer.nativeElement as any).webkitRequestFullscreen((Element as any).ALLOW_KEYBOARD_INPUT);
            }
        } else {
            document.exitFullscreen();
        }
    }

    async onClickPlayButton(): Promise<void> {
        this.isPlaying = true;
        this.videoPlayer = await this.setupVideoPlayer();

        this.logger.log('events', {
            eventType: 'click',
            clickSource: 'unity-view'
        });
    }

    private async setupVideoPlayer(config?: any): Promise<VideoPlayer> {
        const videoPlayer = new VideoPlayer(this.unityPlayer.nativeElement, config);
        await videoPlayer.setupConnection();

        registerKeyboardEvents(videoPlayer);
        registerMouseEvents(videoPlayer, this.unityPlayer.nativeElement);

        return videoPlayer;
    }

    setView(view: SceneViewType): void {
        this.notebook.sceneView = view;
        this.notebook.update();
    }

    onDrop(event: DndDropEvent): void {
        const dragData = (window as any).dragdata;
        const dragType = (window as any).dragtype;

        this.logger.log('states', {
            parentId: (window as any).dragid,
            stateType: 'event',
            dragType: 'drop',
            dropzone: 'renderstreaming'
        });

        switch (dragType) {
            case 'tag':
                for (const s of this.reliveService.getActiveStudy().getSessionsFromTag(event.data)) {
                    s.isActive = true;
                    s.update();
                }
                break;

            case 'session':
                {
                    const session = _.find(this.sessions, s => s.sessionId === event.data);
                    session.isActive = true;
                    session.update();
                }
                break;

            case 'entity':
                {
                    this.notebook.sceneView = 'FollowEntity';
                    this.notebook.sceneViewOptions['follow-entity-name'] = event.data;

                    const session = _.find(this.sessions, s => s.isActive);
                    if (session) {
                        this.notebook.sceneViewOptions['follow-entity-sessionId'] = session.sessionId;
                    }

                    this.notebook.update();
                }
                break;

            case 'event':
                {
                    this.notebook.sceneView = 'FollowEvent';
                    this.notebook.sceneViewOptions['follow-event-name'] = event.data;
                    this.notebook.update();
                }
                break;

            case 'session-entity':
                {
                    const session = _.find(this.sessions, s => s.sessionId === dragData.sessionId);
                    session.isActive = true;
                    session.update();

                    this.notebook.sceneView = 'FollowEntity';
                    this.notebook.sceneViewOptions['follow-entity-name'] = dragData.entityName;
                    this.notebook.sceneViewOptions['follow-entity-sessionId'] = dragData.sessionId;
                    this.notebook.update();
                }
                break;

            default:
                console.warn('Unknown drag type:');
                console.debug(event);
        }

        // FIXME: ugly workaround to prevent dropzone from going red after dropping last entity
        (window as any).dragdata = '';
        window.setTimeout(() => {
            (window as any).dragtype = '';
        }, 200);
    }

}
