import { AfterViewChecked, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';

@Component({
    selector: 'app-chip',
    templateUrl: './chip.component.html',
    styleUrls: ['./chip.component.scss']
})
export class ChipComponent implements OnInit, AfterViewChecked {
    @Input() icon: string;
    @Input() color: string;

    @Output() close: EventEmitter<any> = new EventEmitter();
    @Output() colorChange: EventEmitter<string> = new EventEmitter();

    @ViewChild('colorInput', { static: true }) colorInput: ElementRef<HTMLInputElement>;

    chipStyle = {};

    constructor() { }

    ngOnInit(): void {
        if (this.colorChange.observers.length > 0) {
            this.chipStyle['cursor'] = 'pointer';
        }

        this.chipStyle['color'] = this.pickTextColorBasedOnBgColorAdvanced(this.color);
        this.chipStyle['stroke'] = this.chipStyle['color'];
    }

    ngAfterViewChecked(): void {
        this.chipStyle['color'] = this.pickTextColorBasedOnBgColorAdvanced(this.color);
        this.chipStyle['stroke'] = this.chipStyle['color'];
    }

    // see: https://stackoverflow.com/a/41491220/4090817
    private pickTextColorBasedOnBgColorAdvanced(bgColor: string): string {
        if (!bgColor) {
            return 'black';
        }

        const color = (bgColor.charAt(0) === '#') ? bgColor.substring(1, 7) : bgColor;
        const r = parseInt(color.substring(0, 2), 16); // hexToR
        const g = parseInt(color.substring(2, 4), 16); // hexToG
        const b = parseInt(color.substring(4, 6), 16); // hexToB
        const uicolors = [r / 255, g / 255, b / 255];
        const c = uicolors.map((col) => {
            if (col <= 0.03928) {
                return col / 12.92;
            }
            return Math.pow((col + 0.055) / 1.055, 2.4);
        });
        const L = (0.2126 * c[0]) + (0.7152 * c[1]) + (0.0722 * c[2]);
        return (L > 0.179) ? 'black' : 'white';
    }

    sendClose(): void {
        this.close.emit();
    }

    openColorInput(): void {
        if (this.colorChange.observers.length > 0) {
            const el = this.colorInput.nativeElement;
            el.focus();
            // el.value = this.color;
            el.click();
        }
    }

    onColorChange(): void {
        this.colorChange.emit(this.color);
        this.chipStyle['color'] = this.pickTextColorBasedOnBgColorAdvanced(this.color);
        this.chipStyle['stroke'] = this.chipStyle['color'];
    }
}
