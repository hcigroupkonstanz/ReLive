import { AfterViewChecked, Component, ElementRef, HostListener, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { LoggerService, ReliveService } from '../../services';
import { Notebook, PlaybackSession } from '../../models';
import * as d3 from 'd3';
import _ from 'lodash';

@Component({
    selector: 'app-timeline',
    templateUrl: './timeline.component.html',
    styleUrls: ['./timeline.component.scss'],
    encapsulation: ViewEncapsulation.None // applies CSS to SVG
})
export class TimelineComponent implements OnInit, AfterViewChecked {
    notebook: Notebook;
    sessions: PlaybackSession[];
    scaleX = d3.scaleLinear();
    @ViewChild('container', { static: true }) container: ElementRef<HTMLElement>;
    @ViewChild('timeline', { static: true }) timeline: ElementRef<HTMLElement>;
    @ViewChild('timelineContainer', { static: true }) timelineContainer: ElementRef<HTMLElement>;

    private isDraggingSlider = false;

    private svg: d3.Selection<SVGElement, unknown, null, unknown>;
    private timeAxis: d3.Selection<SVGElement, unknown, null, unknown>;
    private timeAxisSmall: d3.Selection<SVGElement, unknown, null, unknown>;
    private cursor: d3.Selection<HTMLElement, unknown, null, unknown>;

    private timelineDragEvent: any;

    constructor(private relive: ReliveService, private logger: LoggerService) {
        this.notebook = relive.notebook;
        this.sessions = relive.getActiveStudy().sessions;
    }

    ngOnInit(): void {
        this.svg = d3.select(this.timeline.nativeElement)
            .append('svg')
            .attr('position', 'absolute')
            .attr('pointer-events', 'none')
            .attr('viewBox', `0 0 20 20`);

        this.timeAxis = this.svg
            .append('g')
            .style('font', '14px "Roboto"')
            .attr('transform', `translate(0, 40)`);

        this.timeAxisSmall = this.svg
            .append('g')
            .style('font', '14px "Roboto"')
            .attr('transform', `translate(0, 40)`);


        this.cursor = d3.select(this.container.nativeElement)
            .append('div')
            .style('pointer-events', 'all')
            .attr('class', 'playback-cursor')
            .style('height', '100%')
            .on('mousedown', ev => {
                ev.preventDefault();
                this.isDraggingSlider = true;
                this.cursor.classed('grabbed', this.isDraggingSlider);

                this.timelineDragEvent = this.logger.log('events', {
                    eventType: 'mouse',
                    dragType: 'wind'
                });

                this.logger.log('states', {
                    parentId: this.timelineDragEvent.eventId,
                    stateType: 'event',
                    state: 'mouseDown',
                    time: this.notebook.playbackTimeSeconds
                });
            });

        this.render();
    }

    ngAfterViewChecked(): void {
        this.render();
    }

    private render(): void {
        const clientWidth = this.timeline.nativeElement.clientWidth;
        const clientHeight = this.timelineContainer.nativeElement.clientHeight;

        const longestSession = _.maxBy(this.sessions, s => s.endTime - s.startTime);
        this.scaleX
            .domain([0, longestSession.endTime - longestSession.startTime])
            .range([0, clientWidth]);

        // re-render axis
        this.svg
            .attr('viewBox', `0 0 ${clientWidth} ${clientHeight}`);

        const xAxis = d3.axisTop(this.scaleX)
            .ticks(20)
            .tickPadding(2)
            .tickSize(20)
            .tickFormat((d: number) => `${String(Math.floor(d / 60 / 60 / 1000)).padStart(2, '0')}:${String(Math.floor(d / 60 / 1000) % 60).padStart(2, '0')}:${String(Math.floor(d / 1000) % 60).padStart(2, '0')}`);
        this.timeAxis.call(xAxis);

        // add secondary axis with smaller ticks and without labels
        const xAxisSmall = d3.axisTop(this.scaleX)
            .ticks(60)
            .tickPadding(2)
            .tickSize(10)
            .tickFormat(() => '');

        this.timeAxisSmall.call(xAxisSmall);

        // Remove horizontal bar
        this.timeAxisSmall.select('.domain').attr('stroke-width', 0);
        this.timeAxis.select('.domain').attr('stroke-width', 0);

        // update cursor
        const cursorWidth = 8;
        const cursorOffset = this.svg.node().getBoundingClientRect().left - this.container.nativeElement.getBoundingClientRect().left;
        this.cursor
            .style('width', cursorWidth + 'px')
            .style('left', Math.min(Math.max(0, this.scaleX(this.notebook.playbackTimeSeconds * 1000) - (cursorWidth / 2)), this.scaleX(longestSession.endTime - longestSession.startTime)) + cursorOffset + 'px')
            .style('top', 0 + 'px');
    }

    @HostListener('window:mouseup', ['$event'])
    dropSlider(): void {
        this.isDraggingSlider = false;
        this.cursor.classed('grabbed', this.isDraggingSlider);

        if (this.timelineDragEvent) {
            this.logger.log('states', {
                stateType: 'event',
                parentId: this.timelineDragEvent.eventId,
                state: 'mouseUp'
            });
            this.timelineDragEvent.endTimestamp = Date.now();
            this.logger.log('events', this.timelineDragEvent);
            this.timelineDragEvent = null;
        }
    }


    @HostListener('window:mousemove', ['$event'])
    onMouseMove(ev: MouseEvent): void {
        if (this.isDraggingSlider) {
            this.setPlaybackFromMouse(ev);

            if (this.timelineDragEvent) {
                this.logger.log('states', {
                    parentId: this.timelineDragEvent.eventId,
                    stateType: 'event',
                    state: 'mouseMove',
                    time: this.notebook.playbackTimeSeconds
                });
            }
        }
    }

    onTimelineMouseDown(ev: MouseEvent): void {
        this.setPlaybackFromMouse(ev);
        this.isDraggingSlider = true;
        this.cursor.classed('grabbed', this.isDraggingSlider);

        this.timelineDragEvent = this.logger.log('events', {
            eventType: 'mouse',
            dragType: 'wind'
        });

        this.logger.log('states', {
            parentId: this.timelineDragEvent.eventId,
            stateType: 'event',
            state: 'mouseDown',
            time: this.notebook.playbackTimeSeconds
        });
    }

    setPlaybackFromMouse(ev: MouseEvent): void {
        const longestSession = _.maxBy(this.sessions, s => s.endTime - s.startTime);
        const maxPlaybackTime = (longestSession.endTime - longestSession.startTime) / 1000;

        const seconds = this.scaleX.invert(d3.pointer(ev, this.svg.node())[0]) / 1000;
        this.notebook.playbackTimeSeconds = Math.min(Math.max(0, seconds), maxPlaybackTime);
        this.notebook.update();
        this.render();
    }

    togglePlayback(): void {
        this.notebook.isPaused = !this.notebook.isPaused;
        this.notebook.update();
    }

    setPlaybackSpeed(speed: number): void {
        this.notebook.playbackSpeed = speed;
        this.notebook.update();
    }

    formatSeconds(seconds: number): string {
        return `${Math.floor(seconds / 60)}m ${Math.floor(seconds % 60)}s`;
    }

    toggleSession(session: PlaybackSession): void {
        session.isActive = !session.isActive;
        session.update();
    }

    toggleVideo(session: PlaybackSession): void {
        session.showVideos = !session.showVideos;
        session.update();
    }

    skipForward(): void {
        const longestSession = _.maxBy(this.relive.getActiveStudy().sessions, s => s.endTime - s.startTime);
        const longestDuration = longestSession.endTime - longestSession.startTime;
        this.notebook.playbackTimeSeconds = Math.min(longestDuration / 1000, this.notebook.playbackTimeSeconds + 0.02);
        this.notebook.update();
    }

    skipBackward(): void {
        this.notebook.playbackTimeSeconds = Math.max(0, this.notebook.playbackTimeSeconds - 0.02);
        this.notebook.update();
    }
}
