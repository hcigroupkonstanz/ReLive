import { ToolComponent } from './tool.component';
import { Component, OnInit } from '@angular/core';
import { Tool } from '../../models';

@Component({
    template: `<app-line-vis [tool]="tool"></app-line-vis>`
})
export class DistanceToolComponent implements ToolComponent, OnInit {
    public tool: Tool;
    constructor() {
    }

    ngOnInit(): void {
    }
}
