import { Component, HostListener, OnInit } from '@angular/core';
import _ from 'lodash';
import { PlaybackStudy, Tool } from '../../models';
import { LoggerService, ReliveService, ToolsService } from '../../services';

@Component({
    templateUrl: './notebook.component.html',
    styleUrls: ['./notebook.component.scss']
})
export class NotebookComponent implements OnInit {

    study: PlaybackStudy;
    tools: Tool[];

    style = { 'grid-template-rows': '[content] auto [drag] 5px [timeline] 400px' };
    private isDraggingTimeline = false;
    private timelineResizeEvent: any;

    constructor(
        public toolsService: ToolsService,
        public reliveService: ReliveService,
        private logger: LoggerService) {

        this.study = reliveService.getActiveStudy();
        this.tools = toolsService.activeTools;

    }


    ngOnInit(): void {
    }

    startTimelineDrag(ev: MouseEvent): void {
        ev.preventDefault();
        this.isDraggingTimeline = true;

        this.timelineResizeEvent = this.logger.log('events', {
            eventType: 'mouse',
            dragType: 'timeline'
        });

        this.logger.log('states', {
            parentId: this.timelineResizeEvent.eventId,
            stateType: 'event',
            state: 'mouseDown',
        });
    }

    @HostListener('window:mousemove', ['$event'])
    dragTimelineSize(ev: MouseEvent): void {
        if (this.isDraggingTimeline) {
            const maxHeight = Math.ceil(window.innerHeight * 0.8);
            const mouseHeight = window.innerHeight - ev.clientY;
            const height = Math.min(Math.max(200, mouseHeight), maxHeight);
            this.style['grid-template-rows'] = `[content] auto [drag] 5px [timeline] ${height}px`;

            if (this.timelineResizeEvent) {
                this.logger.log('states', {
                    parentId: this.timelineResizeEvent.eventId,
                    stateType: 'event',
                    state: 'mouseMove',
                    height
                });
            }
        }
    }

    @HostListener('window:mouseup', ['$event'])
    endTimelineDrag(ev: MouseEvent): void {
        this.isDraggingTimeline = false;

        if (this.timelineResizeEvent) {
            this.logger.log('states', {
                stateType: 'event',
                parentId: this.timelineResizeEvent.eventId,
                state: 'mouseUp'
            });
            this.timelineResizeEvent.endTimestamp = Date.now();
            this.logger.log('events', this.timelineResizeEvent);
            this.timelineResizeEvent = null;
        }
    }

    onDragStart(type: string, data: string): void {
        // FIXME: ugly workaround to show disabled dropzones based on type
        (window as any).dragtype = type;
        (window as any).dragdata = data;

        const ev = this.logger.log('events', {
            eventType: 'drag',
            dragType: type,
            dragFrom: 'notebook'
        });

        (window as any).dragid = ev.eventId;
    }

    clearTools(): void {
        if (confirm('Are you sure? This will remove all tools from this notebook.')) {
            this.toolsService.clearTools();
        }
    }
}
