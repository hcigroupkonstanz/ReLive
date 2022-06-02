import { Component, ElementRef, HostListener, Input, OnInit, ViewChild } from '@angular/core';
import _ from 'lodash';
import { DndDropEvent } from 'ngx-drag-drop';
import { SharedEvent } from 'src/app/models/shared-event';
import { BoolToolParameter, NumberToolParameter, Tool, ToolInfo } from '../../models';
import { LoggerService, ReliveService, ToolsService } from '../../services';

@Component({
    selector: 'app-add-tool',
    templateUrl: './add-tool.component.html',
    styleUrls: ['./add-tool.component.scss']
})
export class AddToolComponent implements OnInit {
    @ViewChild('container', { static: true }) container: ElementRef<HTMLElement>;
    @Input() index: number;

    isChoosingType = false;
    dragType = '';
    dragData: any;
    // FIXME: small workaround due to HTML structure.. (openToolSelection() is always called when adding a tool)
    private addedTool = false;

    constructor(private toolsService: ToolsService, private relive: ReliveService, private logger: LoggerService) { }

    ngOnInit(): void {
    }

    @HostListener('window:mouseup', ['$event'])
    onMouseUp(ev: MouseEvent): void {
        if (!this.container.nativeElement.contains(ev.target as HTMLElement)) {
            // user clicked outside of 'add tool' dialog
            this.isChoosingType = false;
            this.dragData = '';
            this.dragType = '';
        }
    }

    openToolSelection(): void {
        if (this.addedTool) {
            this.addedTool = false;
        } else {
            this.isChoosingType = true;
        }
    }

    hasCameraTool(): boolean {
        return _.find(this.toolsService.activeTools, t => t.type === 'camera') != null;
    }

    addTrailTool(): void {
        this.addTool({
            type: 'trail',
            name: 'Trail Visualizer',
            minEvents: 0,
            maxEvents: 0,
            minEntities: 1,
            maxEntities: 1,
            parameters: {
                speed: {
                    type: 'bool',
                    name: 'Map Speed to Color',
                    value: false
                } as BoolToolParameter,
                fulltrail: {
                    type: 'bool',
                    name: 'Show Full Trail',
                    value: false
                } as BoolToolParameter,
                traillength: {
                    type: 'number',
                    name: 'Trail Length (s)',
                    value: 60,
                    minValue: 1,
                    stepSize: 1,
                    unit: 's'
                } as NumberToolParameter,
            },
            renderVisualization: true
        });
    }

    addFrustumTool(): void {
        this.addTool({
            type: 'frustum',
            name: 'Frustum Visualizer',
            minEvents: 0,
            maxEvents: 0,
            minEntities: 1,
            maxEntities: 1,
            parameters: { },
            renderVisualization: true
        });
    }

    addDistanceTool(): void {
        this.addTool({
            type: 'distance',
            name: 'Measuring Tape',
            minEvents: 0,
            maxEvents: 0,
            minEntities: 2,
            maxEntities: 2,
            parameters: {},
            renderVisualization: true
        });
    }

    addPropertyTool(): void {
        this.addTool({
            type: 'property',
            name: 'Property Tool',
            minEvents: 0,
            maxEvents: 0,
            minEntities: 1,
            maxEntities: 1,
            parameters: {},
            renderVisualization: true
        });
    }

    addAngleTool(): void {
        this.addTool({
            type: 'angle',
            name: 'Angle Calculator',
            minEvents: 0,
            maxEvents: 0,
            minEntities: 2,
            maxEntities: 2,
            parameters: {},
            renderVisualization: true
        });
    }

    addEventTimer(): void {
        this.addTool({
            type: 'eventTimer',
            name: 'Event Timer',
            minEvents: 2,
            maxEvents: 2,
            minEntities: 0,
            maxEntities: 0,
            parameters: {},
            renderVisualization: true
        });
    }

    addCameraTool(): void {
        this.addTool({
            type: 'camera',
            name: 'Camera',
            minEvents: 0,
            maxEvents: 0,
            minEntities: 0,
            maxEntities: 0,
            parameters: {},
            renderVisualization: true
        });
    }

    private addTool(template: ToolInfo): void {
        this.isChoosingType = false;
        this.addedTool =  true;
        const tool = Tool.fromTemplate(template);

        this.logger.log('events', {
            inputType: 'addTool',
            toolName: template.name,
            eventType: 'click'
        });

        const study = this.relive.getActiveStudy();
        switch (this.dragType) {
            case 'entity':
                {
                    const entity = _.find(study.sharedEntities, { name: this.dragData });
                    if (tool.entities.length < tool.maxEntities && tool.entities.indexOf(entity) < 0) {
                        tool.entities.push(entity);
                    }
                }
                break;

            case 'session':
                {
                    const session = _.find(study.sessions, { sessionId: this.dragData });
                    tool.instances.push({
                        session,
                        color: session.color
                    });
                }
                break;

            case 'tag':
                for (const s of study.getSessionsFromTag(this.dragData)) {
                    tool.instances.push({
                        session: s,
                        color: s.color
                    });
                }
                break;

            case 'event':
                {
                    const pe = _.find(study.sharedEvents, { name: this.dragData });
                    if (tool.events.length < tool.maxEvents && tool.events.indexOf(pe) < 0) {
                        tool.events.push(pe);
                    }
                }
                break;

            case 'session-entity':
                {
                    const session = _.find(study.sessions, { sessionId: this.dragData.sessionId });
                    tool.instances.push({
                        session,
                        color: session.color
                    });
                    const entity = _.find(study.sharedEntities, { name: this.dragData.entityName });
                    if (tool.entities.length < tool.maxEntities && tool.entities.indexOf(entity) < 0) {
                        tool.entities.push(entity);
                    }

                    if (tool.entities.length < tool.maxEntities && tool.entities.indexOf(entity) < 0) {
                        tool.entities.push(entity);
                    }

                }
                break;

            case 'session-event':
                {
                    const session = _.find(study.sessions, { sessionId: this.dragData.sessionId });
                    tool.instances.push({
                        session,
                        color: session.color
                    });

                    let pe = _.find(study.sharedEvents, { name: this.dragData.eventName });
                    if (!pe) {
                        pe = new SharedEvent(_.find(session.events, s => s.eventId === this.dragData.eventId));
                    }

                    if (tool.events.length < tool.maxEvents && tool.events.indexOf(pe) < 0) {
                        tool.events.push(pe);
                    }
                }
                break;
        }

        this.toolsService.addTool(tool, this.index);
        this.dragData = '';
        this.dragType = '';
    }

    onDrop(event: DndDropEvent): void {
        this.isChoosingType = true;
        this.dragType = (window as any).dragtype;

        this.logger.log('states', {
            parentId: (window as any).dragid,
            stateType: 'event',
            dragType: 'drop',
            dropzone: 'add-tool'
        });

        if (this.dragType === 'session-entity' || this.dragType === 'session-event') {
            this.dragData = (window as any).dragdata;
        } else {
            this.dragData = event.data;
        }

        // FIXME: ugly workaround to prevent dropzone from going red after dropping last entity
        (window as any).dragdata = '';
        window.setTimeout(() => {
            (window as any).dragtype = '';
        }, 200);
    }
}
