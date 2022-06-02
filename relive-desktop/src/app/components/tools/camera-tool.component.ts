import { ToolComponent } from './tool.component';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { PlaybackEvent, Tool } from '../../models';
import { ReliveService, SyncService } from 'src/app/services';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

type EventImage = { id: string, source: SafeResourceUrl };

@Component({
    template: `
    <div class="container">
        <button mat-flat-button class="screenshot-btn" (click)="takeScreenshot()">
            <mat-icon>photo_camera</mat-icon>
            Take screenshot from scene view
        </button>
        <ng-scrollbar>
            <div class="img-container">
                <ng-container *ngFor="let image of images">
                    <img [src]="image.source">
                </ng-container>
            </div>
        </ng-scrollbar>
</div>
    `,
    styles: [
        `.container {
            height: 500px;
        }`,
        `.screenshot-btn {
            width: 100%;
            background: #81A1C133;
            margin-bottom: 4px;
        }
        `,
        `.img-container {
            display: flex;
            flex-direction: row;
            flex-wrap: wrap;
        }`,
        `img {
            width: 33%;
            padding: 0.5%;
        }`,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CameraToolComponent implements ToolComponent, OnInit, OnDestroy {
    public tool: Tool;
    images: EventImage[] = [];
    private ngUnsubscribe = new Subject();

    constructor(
        private relive: ReliveService,
        private sync: SyncService,
        private sanitizer: DomSanitizer,
        private changeDetector: ChangeDetectorRef) {
    }

    ngOnInit(): void {
        for (const session of this.relive.getActiveStudy().sessions) {
            for (const ev of session.events) {
                if (ev.eventType === 'screenshot') {
                    this.addEventAttachment(ev);
                }
            }
        }

        this.relive.events$
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(ev => {
                if (ev.eventType === 'screenshot') {
                    this.addEventAttachment(ev);
                    this.changeDetector.detectChanges();
                }
            });
    }

    ngOnDestroy(): void {
        this.ngUnsubscribe.next();
        this.ngUnsubscribe.complete();
    }

    private addEventAttachment(event: PlaybackEvent): void {
        for (const attachment of event.attachments || []) {
            if (attachment.type === 'image') {
                this.images.push({
                    id: attachment.id,
                    source: this.sanitizer.bypassSecurityTrustResourceUrl('data:image/png;base64, ' + attachment.content)
                });
            }
        }
    }

    takeScreenshot(): void {
        this.sync.sendCommand('take-screenshot', 'bool', true);
    }
}
