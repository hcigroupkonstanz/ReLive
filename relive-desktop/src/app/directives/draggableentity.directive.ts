import { Directive, EventEmitter } from '@angular/core';

@Directive({
    selector: '[draggableEntity]'
})
export class DragEntityDirective {
    // not yet used: drag event emitter
    readonly dragStart: EventEmitter<DragEvent>;

}
