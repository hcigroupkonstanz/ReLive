import { ToolComponent } from './tool.component';
import { Component, OnInit } from '@angular/core';
import { Tool } from '../../models';

@Component({
    template: `
    <div *ngFor="let attribute of attributes">
        <h3 class="title">
            <span>Property:</span>
            <app-chip>{{ attribute }}</app-chip>
        </h3>
        <ng-container *ngIf="tool?.data.value[attribute]?.type === 'line'">
            <app-line-vis [tool]="tool" [dataKey]="attribute"></app-line-vis>
        </ng-container>
        <ng-container *ngIf="tool?.data.value[attribute]?.type === '3d'">
            <h3 class="subtitle">X</h3>
            <app-line-vis [tool]="tool" [dataKey]="attribute + 'X'"></app-line-vis>
            <h3 class="subtitle">Y</h3>
            <app-line-vis [tool]="tool" [dataKey]="attribute + 'Y'"></app-line-vis>
            <h3 class="subtitle">Z</h3>
            <app-line-vis [tool]="tool" [dataKey]="attribute + 'Z'"></app-line-vis>
        </ng-container>
        <ng-container *ngIf="tool?.data.value[attribute]?.type === 'single'">
            <div class="property-list">
                <app-chip *ngFor="let instance of tool.instances" class="property" [color]="instance.color">
                    <b>{{instance.session.name}}:</b>
                    {{ tool.data.value[attribute][instance.session.sessionId] }}
                </app-chip>
            </div>
        </ng-container>
    </div>`,
    styles: [
        `.title {
            font-weight: bold;
            margin-left: 15px;
            margin-top: 10px;
            display: flex;
            flex-direction: row;
            align-items: center;
        }`,
        `.subtitle {
            text-align: center;
            font-weight: bold;
            font-size: 22px;
        }`,
        `.title app-chip {
            margin-left: 20px;
        }`,
        `.property-list {
            display: flex;
            flex-direction: row;
            align-items: center;
            margin-left: 5px;
        }`,
        `.property {
            margin-left: 10px;
            margin-bottom: 10px;
        }`
    ]
})
export class PropertyToolComponent implements ToolComponent, OnInit {
    public tool: Tool;
    attributes: string[];

    constructor() {
    }

    ngOnInit(): void {
        this.tool.onEntitiesChanged
            .subscribe(() => {
                this.attributes = [];
                for (const attribute of Object.keys(this.tool.parameters)) {
                    if (this.tool.parameters[attribute].value) {
                        this.attributes.push(attribute);
                    }
                }
            });
    }
}
