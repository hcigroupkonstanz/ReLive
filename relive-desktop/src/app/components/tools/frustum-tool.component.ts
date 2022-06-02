import { ToolComponent } from './tool.component';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Tool } from '../../models';
import * as d3 from 'd3';
import _ from 'lodash';

@Component({
    template: ` <div> Only available in scene and VR view </div>`
})
export class FrustumToolComponent implements ToolComponent, OnInit {
    public tool: Tool;

    ngOnInit(): void {
    }
}
