import { Directive, ViewContainerRef } from '@angular/core';

@Directive({
    selector: '[toolHost]'
})
export class ToolDirective {

    constructor(public viewContainerRef: ViewContainerRef) { }


}
