import { Component, OnInit, ViewEncapsulation, Input, ViewChild, ElementRef, HostListener } from '@angular/core';
import * as d3 from 'd3';
import _ from 'lodash';
import { LoggerService, ReliveService } from 'src/app/services';
import { PlaybackEntity, PlaybackEvent, PlaybackSession } from './../../models';

type VisEvent = {
    posX: number,
    posXend?: number,
    posY: number,
    event: PlaybackEvent,
    color: string,
    isInterval: boolean
};

@Component({
    selector: 'app-timeslider',
    templateUrl: './timeslider.component.html',
    styleUrls: ['./timeslider.component.scss'],
    encapsulation: ViewEncapsulation.None // applies CSS to SVG
})
export class TimesliderComponent implements OnInit {
    @Input() session: PlaybackSession;
    @Input() scaleX: d3.ScaleLinear<number, number, never>;
    @Input() offsetX: number;

    @ViewChild('timesliderContainer', { static: true }) container: ElementRef<HTMLElement>;
    @ViewChild('timesliderEntities', { static: true }) entitySvg: ElementRef<HTMLElement>;
    @ViewChild('timesliderEvents', { static: true }) eventSvg: ElementRef<HTMLElement>;
    @ViewChild('timesliderVideos', { static: true }) videoSvg: ElementRef<HTMLElement>;

    @ViewChild('eventTooltip', { static: true }) eventTooltip: ElementRef<HTMLElement>;
    @ViewChild('entityTooltip', { static: true }) entityTooltip: ElementRef<HTMLElement>;
    @ViewChild('videoTooltip', { static: true }) videoTooltip: ElementRef<HTMLElement>;

    @ViewChild('dragImage', { static: true }) dragImage: ElementRef<HTMLImageElement>;

    private entityRoot: d3.Selection<HTMLElement, unknown, null, unknown>;
    private entityVis: d3.Selection<HTMLElement, unknown, null, unknown>;
    private eventRoot: d3.Selection<HTMLElement, unknown, null, unknown>;
    private eventVis: d3.Selection<HTMLElement, unknown, null, unknown>;
    private videoRoot: d3.Selection<HTMLElement, unknown, null, unknown>;
    private videoVis: d3.Selection<HTMLElement, unknown, null, unknown>;

    private logEvent: any;

    mouseOverEvent: PlaybackEvent = null;
    mouseOverEntity: PlaybackEntity = null;
    mouseOverVideo: any = null;

    constructor(private relive: ReliveService, private logger: LoggerService) {
    }

    ngOnInit(): void {
        this.entityRoot = d3.select(this.entitySvg.nativeElement)
            .append('div');
        this.entityVis = this.entityRoot
            .append('div');

        this.eventRoot = d3.select(this.eventSvg.nativeElement)
            .append('div');
        this.eventVis = this.eventRoot.append('div');

        this.videoRoot = d3.select(this.videoSvg.nativeElement)
            .append('div');
        this.videoVis = this.videoRoot.append('div');

        this.redrawEntities();
        this.redrawVideos();

        this.relive.events$.subscribe(() => this.redrawEvents());
        this.session.eventFilterChanged$.subscribe(() => this.redrawEvents());
        this.session.remoteUpdate$.subscribe(() => this.redrawEntities());
    }

    onDragStart(event: DragEvent): void {
        event.dataTransfer.setDragImage(this.dragImage.nativeElement, 0, 0);
        this.logger.log('events', {
            eventType: 'drag',
            dragType: (window as any).dragtype,
            dragFrom: 'timeline'
        });
    }

    toggleEntityExpansion(): void {
        this.session.isEntitiesExpanded = !this.session.isEntitiesExpanded;
        this.session.update();
    }

    toggleEventsExpansion(): void {
        this.session.isEventsExpanded = !this.session.isEventsExpanded;
        this.session.update();
    }

    toggleAudioExpansion(): void {
        this.session.isAudioExpanded = !this.session.isAudioExpanded;
        this.session.update();
    }

    toggleVideoExpansion(): void {
        this.session.isVideoExpanded = !this.session.isVideoExpanded;
        this.session.update();
    }

    getSessionStyle(): any {
        const width = this.scaleX(this.session.endTime - this.session.startTime);
        return {
            width: width + 'px',
            'margin-left': this.offsetX + 'px'
        };
    }

    private redrawEntities(): void {
        const sessionDuration = this.session.endTime - this.session.startTime;

        const scaleX = d3.scaleLinear()
            .domain([this.session.startTime, this.session.endTime])
            .range([0, this.scaleX(sessionDuration)]);

        const entityHeight = 20;
        const entityMargin = 1;
        this.entityRoot
            .style('height', this.session.entities.length * (entityHeight + entityMargin) + 'px')
            .style('position', 'relative');

        // prepare tooltip
        const tooltip = d3.select(this.entityTooltip.nativeElement);
        const tooltipContainer = this.container.nativeElement;
        // workaround since 'function's in JS have different 'this'
        const setTooltip = (entity: PlaybackEntity) => this.mouseOverEntity = entity;

        const mouseover = (ev: MouseEvent, d: PlaybackEntity): void => {
            setTooltip(d);
            tooltip
                .style('visibility', 'visible');
            (window as any).dragtype = 'session-entity';
            (window as any).dragdata = {
                entityName: d.name,
                sessionId: d.sessionId
            };
        };

        const mousemove = (ev: MouseEvent): void => {
            const [x, y] = d3.pointer(ev, tooltipContainer);
            tooltip
                .style('left', null)
                .style('right', null)
                .style('top', y + 'px');

            if (x < tooltipContainer.clientWidth / 2) {
                tooltip
                    .style('left', (x + 20) + 'px');
            } else {
                tooltip
                    .style('right', (tooltipContainer.clientWidth - x + 20) + 'px');
            }
        };

        const mouseleave = (ev: MouseEvent): void => {
            tooltip.style('visibility', 'hidden');
        };

        const entityNodes = this.entityVis
            .selectAll('.timeslider-entity')
            .data(this.session.entities, (e, i) => i);

        entityNodes.enter()
            .append('div')
            .attr('class', 'timeslider-entity')
            .classed('inactive', e => !e.isVisible)
            .style('left', e => scaleX(e.timeStart) + 'px')
            .style('width', e => scaleX(e.timeEnd === e.timeStart ? this.session.endTime : e.timeEnd) - scaleX(e.timeStart) + 'px')
            .style('top', (s, i) => i * (entityHeight + entityMargin) + 'px')
            .style('height', entityHeight + 'px')
            .attr('draggable', true)
            .on('mouseover', mouseover)
            .on('mousemove', mousemove)
            .on('mouseleave', mouseleave)
            .on('click', (e, d) => {
                d.isVisible = !d.isVisible;
                d.update();
                this.redrawEntities();
            })
            .append('span')
                .text(e => e.name)
                .attr('class', 'timeslider-entity-text');

        entityNodes.join('div.timeslider-entity')
            .classed('inactive', e => !e.isVisible);
    }


    private redrawEvents(): void {
        const sessionDuration = this.session.endTime - this.session.startTime;

        const scaleX = d3.scaleLinear()
            .domain([this.session.startTime, this.session.endTime])
            .range([0, this.scaleX(sessionDuration)]);

        // calculate event layout
        const singleEventPoints: VisEvent[] = [];
        const intervalEventPoints: VisEvent[] = [];

        let tmpEvents: VisEvent[] = [];
        const eventSize = 12;
        const eventPadding = 6;

        let maxEventHeight = 0;

        // code for testing layout
        // const testEvents: any[] = [];
        // for (let i = 0; i < 200; i++) {
        //     const e: any = {
        //         timestamp: this.session.startTime + Math.random() * sessionDuration
        //     };

        //     if (Math.random() < 0.1) {
        //         e.endTimestamp = Math.min(this.session.endTime, e.timestamp + Math.random() * sessionDuration / 2);
        //     }

        //     while (Math.random() < 0.6 && (!e.entityIds || (e.entityIds && e.entityIds.length < 5))) {
        //         if (!e.entityIds) {
        //             e.entityIds = [];
        //         }
        //         e.entityIds.push(this.session.entities[e.entityIds.length].entityId);
        //     }

        //     const rnd = Math.random();
        //     if (rnd < 0.2) {
        //         e.eventType = 'other';
        //     } else if (rnd < 0.4) {
        //         e.eventType = 'criticalincident';
        //     } else if (rnd < 0.6) {
        //         e.eventType = 'click';
        //     } else if (rnd < 0.8) {
        //         e.eventType = 'log';
        //         e.message = 'Test message: Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\n Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.\n Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.\n Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\n\n\nShould appear at the bottom';
        //     } else {
        //         e.eventType = 'task';
        //     }

        //     testEvents.push(e);
        // }

        // const filteredEvents = _(testEvents).filter(e => this.session.eventFilters[e.eventType]).sortBy(e => e.timestamp).value();
        const filteredEvents = _(this.session.events).filter(e => this.session.eventFilters[e.eventType]).sortBy(e => e.timestamp).value();

        for (const event of filteredEvents) {
            const visEvent: VisEvent = {
                event,
                isInterval: event.endTimestamp !== undefined,
                posX: scaleX(event.timestamp),
                posY: 1,
                color: this.getEventColor(event.eventType)
            };

            // remove events that are not in vicinity of current events - so that we'll have a list of nearby events
            const padding = eventSize / 2;
            for (let i = tmpEvents.length - 1; i >= 0; i--) {
                const tmpEvent = tmpEvents[i];
                const removeInterval = tmpEvent.isInterval && tmpEvent.posXend + eventSize + padding < visEvent.posX;
                const removeSingle = !tmpEvent.isInterval && tmpEvent.posX + eventSize + padding < visEvent.posX;

                if (removeInterval || removeSingle) {
                    tmpEvents.splice(i, 1);
                }
            }

            // use first 'available' position
            tmpEvents = _.sortBy(tmpEvents, e => e.posY);
            visEvent.posY = tmpEvents.length + 1; // in case no free slot is found
            for (let i = 0; i < tmpEvents.length; i++) {
                if (tmpEvents[i].posY > i + 1) {
                    visEvent.posY = i + 1;
                    break;
                }
            }


            if (visEvent.isInterval) {
                visEvent.posXend = scaleX(event.endTimestamp);
                intervalEventPoints.push(visEvent);
            } else {
                singleEventPoints.push(visEvent);
            }

            maxEventHeight = Math.max(maxEventHeight, visEvent.posY);
            tmpEvents.push(visEvent);
        }

        // resize svg to fit all events
        const eventHeight = eventSize + eventPadding;
        this.eventRoot
            .style('height', (maxEventHeight + 4) * eventHeight + 'px')
            .style('position', 'relative');


        // prepare tooltip
        const tooltip = d3.select(this.eventTooltip.nativeElement);
        const tooltipContainer = this.container.nativeElement;
        // workaround since 'function's in JS have different 'this'
        const setTooltip = (ev: PlaybackEvent) => this.mouseOverEvent = ev;
        const mouseover = (ev: MouseEvent, d: VisEvent): void => {
            setTooltip(d.event);
            tooltip
                .style('opacity', 1)
                .style('visibility', 'visible');
            (window as any).dragtype = 'session-event';
            (window as any).dragdata = {
                eventName: d.event.name,
                eventId: d.event.eventId,
                sessionId: d.event.sessionId
            };
        };

        const mousemove = (ev: MouseEvent, d: VisEvent): void => {
            const [x, y] = d3.pointer(ev, tooltipContainer);
            tooltip
                .style('left', null)
                .style('right', null)
                .style('top', y + 'px');

            if (x < tooltipContainer.clientWidth / 2) {
                tooltip
                    .style('left', (x + 20) + 'px');
            } else {
                tooltip
                    .style('right', (tooltipContainer.clientWidth - x + 20) + 'px');
            }
        };

        const mouseleave = (ev: MouseEvent): void => {
            tooltip
                .style('opacity', 0)
                .style('visibility', 'hidden');
        };

        // draw single events
        const singleEventNodes = this.eventVis
            .selectAll('.single-event')
            .data(singleEventPoints, (e: VisEvent) => e.event.eventId);

        singleEventNodes.exit().remove();

        singleEventNodes.enter()
            .append('div')
            .attr('class', 'single-event')
            .attr('draggable', true)
            .style('width', eventSize + 'px')
            .style('height', eventSize + 'px')
            .style('background', ev => ev.color)
            .style('left', ev => ev.posX - eventSize / 2 + 'px')
            .style('top', ev => ev.posY * eventHeight + 'px')
            .on('mouseover', mouseover)
            .on('mousemove', mousemove)
            .on('mouseleave', mouseleave);

        // draw interval events
        const intervalEventNodes = this.eventVis
            .selectAll('.interval-event')
            .data(intervalEventPoints, (e: VisEvent) => e.event.eventId);

        intervalEventNodes.exit().remove();

        const intervalEventNodesEnter = intervalEventNodes
            .enter()
            .append('div')
            .attr('class', 'interval-event')
            .attr('opacity', 0.8);

        // line
        const lineHeight = 2;
        intervalEventNodesEnter
            .append('div')
            .attr('class', 'line')
            .style('width', ev => (scaleX(ev.event.endTimestamp) - scaleX(ev.event.timestamp) - eventSize) + 'px')
            .style('height', lineHeight / 2 + 'px')
            .style('border-width', lineHeight / 2 + 'px')
            .style('left', ev => ev.posX + eventSize / 2 + 'px')
            .style('top', ev => ev.posY * eventHeight + eventSize / 2 - lineHeight / 2 + 'px')
            .on('mouseover', mouseover)
            .on('mousemove', mousemove)
            .on('mouseleave', mouseleave);

        // start
        intervalEventNodesEnter
            .append('div')
            .attr('class', 'node')
            .attr('draggable', true)
            .style('width', eventSize + 'px')
            .style('height', eventSize + 'px')
            .style('background', ev => ev.color)
            .style('left', ev => ev.posX - (eventSize / 2) + 'px')
            .style('top', ev => ev.posY * eventHeight + 'px')
            .on('mouseover', mouseover)
            .on('mousemove', mousemove)
            .on('mouseleave', mouseleave);

        // end
        intervalEventNodesEnter
            .append('div')
            .attr('class', 'node')
            .attr('draggable', true)
            .style('width', eventSize + 'px')
            .style('height', eventSize + 'px')
            .style('background', ev => ev.color)
            .style('left', ev => ev.posXend - (eventSize / 2) + 'px')
            .style('top', ev => ev.posY * eventHeight + 'px')
            .on('mouseover', mouseover)
            .on('mousemove', mousemove)
            .on('mouseleave', mouseleave);

        intervalEventNodes.join('.node')
            .style('top', ev => ev.posY * eventHeight + 'px');
        intervalEventNodes.join('.line')
            .style('top', ev => ev.posY * eventHeight + 'px');
        singleEventNodes
            .style('top', ev => ev.posY * eventHeight + 'px');
    }

    getEventColor(type: string): string {
        switch (type) {
            case 'criticalincident':
                return '#ef5350';
            case 'cubes':
            case 'voice':
                return '#673ab7';
            case 'log':
                return '#42a5f5';
            case 'task':
                return '#ffca28';
            case 'click':
            case 'action':
                return '#66bb6a';
            case 'screenshot':
                return '#9c27b0';
            default:
                console.log(`Unknown event type '${type}'`);
                return '#78909c';
        }
    }

    getEntityName(id: string): string {
        const entity = _.find(this.session.entities, e => e.entityId === id);
        if (entity) {
            return entity.name;
        } else {
            return 'Unknown entity';
        }
    }

    getEntityColor(type: string): string {
        switch (type) {
            case 'object':
                return '#ef5350';
            case 'zero':
                return '#42a5f5';
            case 'task':
                return '#ffca28';
            case 'click':
                return '#66bb6a';
            default:
                console.log(`Unknown event type '${type}'`);
                return '#78909c';
        }
    }

    toggleFilter(ev: Event, name: string): void {
        this.session.eventFilters[name] = !this.session.eventFilters[name];
        // FIXME: workaround for more STREAM events
        if (name === 'click') {
            this.session.eventFilters.action = this.session.eventFilters.click;
        }

        this.session.update();
        ev.stopPropagation();
        this.redrawEvents();
    }



    private redrawVideos(): void {
        const sessionDuration = this.session.endTime - this.session.startTime;

        const scaleX = d3.scaleLinear()
            .domain([this.session.startTime, this.session.endTime])
            .range([0, this.scaleX(sessionDuration)]);

        const videos: any[] = [];
        for (const entity of _.filter(this.session.entities, e => !!e.attachments)) {
            for (const attachment of entity.attachments) {
                if (attachment.type === 'video') {
                    videos.push({
                        timeStart: attachment.startTime ? this.session.startTime + attachment.startTime : this.session.startTime,
                        timeEnd: attachment.startTime ? this.session.startTime + attachment.startTime + attachment.duration : this.session.endTime,
                        name: attachment.id
                    });
                }
            }
        }
        const videoHeight = 20;
        const videoMargin = 1;
        this.videoRoot
            .style('height', videos.length * (videoHeight + videoMargin) + 'px')
            .style('position', 'relative');

        // prepare tooltip
        const tooltip = d3.select(this.videoTooltip.nativeElement);
        const tooltipContainer = this.container.nativeElement;
        // workaround since 'function's in JS have different 'this'
        const setTooltip = (video: any) => this.mouseOverVideo = video;

        const mouseover = (ev: MouseEvent, d: any): void => {
            setTooltip(d);
            tooltip
                .style('visibility', 'visible');
        };

        const mousemove = (ev: MouseEvent): void => {
            const [x, y] = d3.pointer(ev, tooltipContainer);
            tooltip
                .style('left', null)
                .style('right', null)
                .style('top', y + 'px');

            if (x < tooltipContainer.clientWidth / 2) {
                tooltip
                    .style('left', (x + 20) + 'px');
            } else {
                tooltip
                    .style('right', (tooltipContainer.clientWidth - x + 20) + 'px');
            }
        };

        const mouseleave = (ev: MouseEvent): void => {
            tooltip
                .style('visibility', 'hidden');
        };

        const videoNodes = this.videoVis
            .selectAll('.timeslider-video')
            .data(videos, (e, i) => i);

        videoNodes.enter()
            .append('div')
            .attr('class', 'timeslider-video')

        .join('.timeslider-video')
            .attr('class', 'timeslider-video')
            .style('left', e => scaleX(e.timeStart) + 'px')
            .style('top', (s, i) => i * (videoHeight + videoMargin) + 'px')
            .style('width', e => scaleX(e.timeEnd) - scaleX(e.timeStart) + 'px')
            .style('height', videoHeight + 'px')
            .on('mouseover', mouseover)
            .on('mousemove', mousemove)
            .on('mouseleave', mouseleave)
            .append('span')
                .text(e => e.name);
    }

}
