import { ToolComponent } from './tool.component';
import { Component } from '@angular/core';
import { Tool } from '../../models';

@Component({
    template: `<div> Unknown tool </div>`
})
export class DefaultToolComponent implements ToolComponent {
    public tool: Tool;
}
