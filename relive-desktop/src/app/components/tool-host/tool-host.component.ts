import { FrustumToolComponent } from './../tools/frustum-tool.component';
import { TrailToolComponent } from './../tools/trail-tool.component';
import { PropertyToolComponent } from './../tools/property-tool.component';
import { DistanceToolComponent } from './../tools/distance-tool.component';
import { AngleToolComponent } from './../tools/angle-tool.component';
import { DefaultToolComponent } from './../tools/default-tool.component';
import { ToolComponent } from './../tools/tool.component';
import { LoggerService, ReliveService, ToolsService } from '../../services';
import { ToolDirective } from '../../directives/tool.directive';
import { Component, ComponentFactoryResolver, Input, OnInit, ViewChild, Type, OnDestroy, ComponentRef, AfterViewInit } from '@angular/core';
import { PlaybackSession, SharedEntity, Tool, ToolInstance, ToolParameter, ToolType } from '../../models';
import _ from 'lodash';
import { DndDropEvent } from 'ngx-drag-drop';
import { CameraToolComponent } from '../tools/camera-tool.component';
import { EventTimerComponent } from '../tools/event-timer.component';
import { SharedEvent } from 'src/app/models/shared-event';

@Component({
    selector: 'app-tool-host',
    templateUrl: './tool-host.component.html',
    styleUrls: ['./tool-host.component.scss']
})
export class ToolHostComponent implements OnInit, AfterViewInit, OnDestroy {
    @Input() tool: Tool;
    @ViewChild(ToolDirective) toolHost: ToolDirective;

    hasProperties = true;
    icon = '';

    sessions: PlaybackSession[];
    events: SharedEvent[];
    entities: SharedEntity[];

    private componentRef: ComponentRef<ToolComponent>;
    private toolComponent: ToolComponent;

    constructor(
        private componentFactoryResolver: ComponentFactoryResolver,
        public reliveService: ReliveService,
        private logger: LoggerService,
        private toolsService: ToolsService) {
        this.sessions = reliveService.getActiveStudy().sessions;
        this.events = reliveService.getActiveStudy().sharedEvents;
        this.entities = reliveService.getActiveStudy().sharedEntities;
    }

    ngOnInit(): void {
        this.hasProperties = Object.keys(this.tool.parameters).length > 0;
        this.icon = this.toolsService.getIconFromType(this.tool.type);
        setTimeout(() => {
            this.loadComponent();
        });
    }

    ngAfterViewInit(): void {
    }

    ngOnDestroy(): void {
        this.componentRef.destroy();
    }

    removeTool(): void{
        this.toolsService.deleteTool(this.tool);
    }

    toggleToolExpansion(): void {
        this.tool.isExpanded = !this.tool.isExpanded;
        if (this.tool.isExpanded) {
            setTimeout(() => {
                this.loadComponent();
            });
        }
    }

    toggleVisibility(): void {
        this.tool.renderVisualization = !this.tool.renderVisualization;
        this.tool.update();
    }

    exportData(): void {
        // dummy method; see: https://stackoverflow.com/a/14966131/4090817
        const rows = [ [ 'Dummy data' ] ];
        const csvContent = 'data:text/csv;charset=utf-8,' + rows.map(e => e.join(',')).join('\n');

        const encodedUri = encodeURI(csvContent);
        const link = document.createElement('a');
        link.setAttribute('href', encodedUri);
        link.setAttribute('download', this.tool.name + '.csv');
        document.body.appendChild(link); // Required for FF

        link.click();
    }

    duplicateTool(): void {
        const duplicate = Tool.fromTemplate({
            name: this.tool.name,
            type: this.tool.type,
            minEvents: this.tool.minEvents,
            maxEvents: this.tool.maxEvents,
            minEntities: this.tool.minEntities,
            maxEntities: this.tool.maxEntities,
            renderVisualization: this.tool.renderVisualization,
            parameters: _.clone(this.tool.parameters)
        });

        for (const e of this.tool.entities) {
            duplicate.entities.push(e);
        }
        for (const e of this.tool.events) {
            duplicate.events.push(e);
        }
        for (const i of this.tool.instances) {
            duplicate.instances.push(_.clone(i));
        }

        duplicate.data.next(this.tool.data.value);
        duplicate.isExpanded = this.tool.isExpanded;

        this.toolsService.addTool(duplicate, this.tool.notebookIndex + 1);
    }

    onParameterChanged(): void {
        this.tool.update();
    }

    removeEntity(entity: SharedEntity): void {
        _.pull(this.tool.entities, entity);

        if (this.tool.type === 'property') {
            this.tool.parameters = {};
            this.hasProperties = false;
        }

        this.tool.update();
    }

    removeEvent(event: SharedEvent): void {
        _.pull(this.tool.events, event);
        this.tool.update();
    }

    removeInstance(instance: ToolInstance): void {
        _.pull(this.tool.instances, instance);
        this.tool.update();
    }

    loadComponent(): void {
        const component = this.getToolComponent(this.tool.type);
        const componentFactory = this.componentFactoryResolver.resolveComponentFactory(component);

        if (this.toolHost) {
            const viewContainerRef = this.toolHost.viewContainerRef;
            viewContainerRef.clear();

            this.componentRef = viewContainerRef.createComponent<ToolComponent>(componentFactory);
            this.toolComponent = this.componentRef.instance;
            this.toolComponent.tool = this.tool;
        }
    }

    public getToolComponent(toolType: ToolType): Type<ToolComponent> {
        switch (toolType) {
            case 'angle':
                return AngleToolComponent;
            case 'distance':
                return DistanceToolComponent;
            case 'trail':
                return TrailToolComponent;
            case 'frustum':
                return FrustumToolComponent;
            case 'property':
                return PropertyToolComponent;
            case 'eventTimer':
                return EventTimerComponent;
            case 'camera':
                return CameraToolComponent;
            default:
                return DefaultToolComponent;
        }
    }

    changeColor(color: string, instance: ToolInstance): void {
        instance.color = color;
        this.tool.update();
    }

    moveDown(): void {
        this.toolsService.moveTool(this.tool, 1);
    }

    moveUp(): void {
        this.toolsService.moveTool(this.tool, -1);
    }

    createRange(to: number): number[] {
        const items: number[] = [];
        for (let i = 0; i < to; i++) {
            items.push(i);
        }
        return items;
    }

    isDropDisabled(): boolean {
        // FIXME: ugly workaround to show disabled dropzones based on type
        const dragType = (window as any).dragtype as string;
        const dragData = (window as any).dragdata as string;
        const isEntityDropDisabled = (dragType === 'entity' || dragType === 'session-entity') && (this.tool.entities.length >= this.tool.maxEntities || !!_.find(this.tool.entities, e => e.name === dragData));
        const isEventDropDisabled = (dragType === 'event' || dragType === 'session-event') && (this.tool.events.length >= this.tool.maxEvents || !!_.find(this.tool.events, e => e.name === dragData));
        const isCameraTool = this.tool.type === 'camera';
        return isEntityDropDisabled || isEventDropDisabled || isCameraTool;
    }

    onDrop(event: DndDropEvent): void {
        // FIXME: ugly workaround to prevent dropzone from going red after dropping last entity
        window.setTimeout(() => {
            (window as any).dragtype = '';
        }, 200);
        const dragData = (window as any).dragdata;
        const dragType = (window as any).dragtype;
        (window as any).dragdata = '';

        this.logger.log('states', {
            parentId: (window as any).dragid,
            stateType: 'event',
            dragType: 'drop',
            dropzone: 'tool'
        });
        this.logger.log('events', {
            entityIds: [ this.tool.id ],
            eventType: 'drop'
        });


        const study = this.reliveService.getActiveStudy();
        switch (dragType) {
            case 'entity':
                {
                    const entity = _.find(study.sharedEntities, { name: event.data });
                    this.addEntity(entity);
                }
                break;

            case 'session':
                {
                    const session = _.find(study.sessions, { sessionId: event.data });
                    this.addSession(session, true);
                }
                break;

            case 'tag':
                {
                    for (const s of study.getSessionsFromTag(event.data)) {
                        this.addSession(s, false);
                    }

                    this.tool.update();
                }
                break;

            case 'event':
                {
                    const pe = _.find(study.sharedEvents, { name: event.data });
                    this.addEvent(pe);
                }
                break;

            case 'session-entity':
                {
                    const session = _.find(study.sessions, { sessionId: dragData.sessionId });
                    this.addSession(session, false);
                    const entity = _.find(study.sharedEntities, { name: dragData.entityName });
                    this.addEntity(entity);
                    this.tool.update();
                }
                break;

            case 'session-event':
                {
                    const session = _.find(study.sessions, { sessionId: dragData.sessionId });
                    this.addSession(session, false);
                    const pe = _.find(study.sharedEvents, { name: dragData.eventName });
                    if (pe) {
                        this.addEvent(pe);
                    } else {
                        this.addEvent(new SharedEvent(_.find(session.events, s => s.eventId === dragData.eventId)));
                    }
                    this.tool.update();
                }
                break;

            default:
                console.warn('Unknown drag data:');
                console.debug(event.data);
                break;

        }
    }

    addSession(session: PlaybackSession, update: boolean): void {
        if (!this.hasSession(session)) {
            this.tool.instances.push({
                session,
                color: session.color
            });

            if (update) {
                this.tool.update();
            }
        }
    }

    hasSession(session: PlaybackSession): boolean {
        return !!_.find(this.tool.instances, i => i.session.sessionId === session.sessionId);
    }

    addEntity(entity: SharedEntity): void {
        if (this.tool.entities.length < this.tool.maxEntities && !this.hasEntity(entity)) {
            this.tool.entities.push(entity);

            if (this.tool.type === 'property') {
                for (const attribute of entity.entities[0].attributes) {
                    this.tool.parameters[attribute] = {
                        type: 'bool',
                        name: attribute,
                        value: false
                    };
                }
                this.hasProperties = true;
            }

            this.tool.update();
        }
    }

    hasEntity(entity: SharedEntity): boolean {
        return !!_.find(this.tool.entities, e => e?.name === entity?.name);
    }

    addEvent(event: SharedEvent): void {
        if (this.tool.events.length < this.tool.maxEvents && !this.hasEvent(event)) {
            this.tool.events.push(event);
            this.tool.update();
        }
    }

    hasEvent(event: SharedEvent): boolean {
        return !!_.find(this.tool.events, e => e.name === event.name);
    }
}
