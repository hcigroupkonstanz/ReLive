import { ReliveService } from 'src/app/services';
import { PlaybackEntity, PlaybackEvent, PlaybackSession } from '../../models';
import { Component, Input, ViewChild, AfterViewInit, ElementRef } from '@angular/core';
import { interval, merge } from 'rxjs';
import { filter } from 'rxjs/operators';

@Component({
    selector: 'app-video-attachment',
    templateUrl: './video-attachment.component.html',
    styleUrls: ['./video-attachment.component.scss']
})
export class VideoAttachmentComponent implements AfterViewInit {
    @ViewChild('vid') videoElement: ElementRef<HTMLVideoElement>;

    @Input() source: string;
    @Input() session: PlaybackSession;
    @Input() origin: PlaybackEntity | PlaybackEvent;
    @Input() attachment: any;

    canPlay = true;
    originName: string;

    constructor(private relive: ReliveService) { }

    ngAfterViewInit(): void {
        merge(
            this.relive.notebook.isPaused$,
            this.relive.notebook.playbackSpeed$
        ).subscribe(() => this.updateVideoStatus(false));

        this.relive.notebook.playbackTimeSeconds$
            .pipe(filter(() => this.relive.notebook.isPaused))
            .subscribe(() => this.updateVideoStatus(true));

        this.updateVideoStatus(true);
        interval(1000).subscribe(() => this.updateVideoStatus(false));

        if (this.origin instanceof PlaybackEntity) {
            this.originName = this.origin.name;
        } else {
            this.originName = this.origin.eventId;
        }
    }

    private updateVideoStatus(updateTime: boolean): void {
        const notebook = this.relive.notebook;
        const vid = this.videoElement.nativeElement;

        try {
            if (!notebook.isPaused) {
                vid.play();
            } else {
                vid.pause();
            }

            vid.playbackRate = notebook.playbackSpeed;
            if (updateTime) {
                vid.currentTime = Math.max(notebook.playbackTimeSeconds - this.attachment.startTime / 1000, 0);
            }
        } catch (e) {
            console.error(e);
        }

        this.canPlay = notebook.playbackTimeSeconds - this.attachment.startTime / 1000 < vid.duration || notebook.playbackTimeSeconds < this.attachment.startTime / 1000;
    }

    toggleFullscreen(): void {
        const el = this.videoElement.nativeElement;
        if (!this.isFullscreen()) {
            if (el.parentElement.requestFullscreen) {
                el.parentElement.requestFullscreen();
            } else if ((el as any).webkitRequestFullscreen) {
                (el as any).webkitRequestFullscreen((Element as any).ALLOW_KEYBOARD_INPUT);
            }
        } else {
            document.exitFullscreen();
        }
    }

    isFullscreen(): boolean {
        return !!document.fullscreenElement;
    }
}
