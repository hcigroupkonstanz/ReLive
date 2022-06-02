import { ToolComponent } from './tool.component';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Tool } from '../../models';
import * as d3 from 'd3';
import _ from 'lodash';
import { ReliveService } from 'src/app/services';
import { takeUntil, throttleTime } from 'rxjs/operators';
import { Subject } from 'rxjs';

@Component({
    template: `
    <div [style.height.px]="layout.height + layout.marginBottom + layout.marginTop">
        <div id="vis" #visContainer></div>
        <div id="error-container" *ngIf="hasErrors">
            <mat-icon id="error-icon">warning</mat-icon>
            <div id="errors">
                Could not process data correctly:
                <div class="error" *ngFor='let error of errors | keyvalue'>
                    <b>{{ error.key }}:</b>
                    {{ error.value }}
                </div>
            </div>
        </div>
    </div>`,
    styles: [
        `#vis { position: relative; }`,
        `#error-container {
            position: absolute;
            right: 20px;
            top: 20px;
        }`,
        `#error-icon {
            color: #ff5722;
            font-size: 30px;
        }`,
        `#errors {
            position: absolute;
            right: 20px;
            visibility: hidden;
            width: 300px;
            color: black;
            background: white;
            border: 1px solid #DFE0EB;
            border-radius: 2px;
            padding: 15px;
            pointer-events: none;
        }`,
        `#error-container:hover #errors {
            visibility: visible;
        }`
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TrailToolComponent implements ToolComponent, OnInit, OnDestroy {
    public tool: Tool;
    @ViewChild('visContainer', { static: true }) visContainer: ElementRef;

    hasErrors = false;
    errors = {};

    private canvas: d3.Selection<HTMLCanvasElement, unknown, null, unknown>;
    private svg: d3.Selection<SVGElement, unknown, null, unknown>;
    private legend: d3.Selection<SVGElement, unknown, null, unknown>;
    private defs: d3.Selection<SVGElement, unknown, null, unknown>;
    private context: CanvasRenderingContext2D;
    private scaleX: d3.ScaleLinear<number, number, never>;
    private xAxis: d3.Selection<SVGElement, unknown, null, unknown>;
    private scaleY: d3.ScaleLinear<number, number, never>;
    private yAxis: d3.Selection<SVGElement, unknown, null, unknown>;
    private positionVis: d3.Selection<SVGElement, unknown, null, unknown>;
    private lineData: any[];

    private speedExtents: { [sessionId: string]: [number, number] } = {};

    layout = {
        marginTop: 30,
        marginBottom: 10,
        marginLeft: 30,
        marginRight: 10,
        getWidth: () => this.visContainer.nativeElement.clientWidth,
        height: 900
    };

    // performance optimization
    private hasRenderedFullTrail = false;
    private ngUnsubscribe = new Subject();

    constructor(private reliveService: ReliveService, private changeDetector: ChangeDetectorRef) {
    }

    ngOnInit(): void {
        this.initVis();
        this.tool.data
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(d => this.renderData(d));
        this.reliveService.notebook.playbackTimeSeconds$
            .pipe(throttleTime(50), takeUntil(this.ngUnsubscribe))
            .subscribe(() => this.redraw());
        this.tool.onInstancesChanged
            .pipe(throttleTime(100), takeUntil(this.ngUnsubscribe))
            .subscribe(() => {
                this.hasRenderedFullTrail = false;
                this.redraw();
            });
    }

    ngOnDestroy(): void {
        this.ngUnsubscribe.next();
        this.ngUnsubscribe.complete();
    }

    private initVis(): void {
        const visWidth = this.layout.getWidth() + this.layout.marginLeft + this.layout.marginRight;
        const visHeight = this.layout.height + this.layout.marginTop + this.layout.marginBottom;
        this.canvas = d3.select(this.visContainer.nativeElement)
            .append('canvas')
            .style('position', 'absolute')
            .attr('width', this.layout.getWidth())
            .attr('height', this.layout.height)
            .style('left', '0px')
            .style('top', '0px');
        this.context = this.canvas.node().getContext('2d');

        this.svg = d3.select(this.visContainer.nativeElement)
            .append('svg')
            .style('position', 'absolute')
            .attr('viewBox', `0 0 ${visWidth} ${visHeight}`);

        this.legend = this.svg.append('g');
        this.defs = this.svg.append('defs');

        this.xAxis = this.svg.append('g')
            .attr('transform', `translate(0, ${this.layout.marginTop})`);

        this.yAxis = this.svg.append('g')
            .attr('transform', `translate(${this.layout.marginLeft}, 0)`);

        this.positionVis = this.svg.append('g');
    }

    private renderData(data: any): void {
        this.errors = {};
        this.hasErrors = false;

        this.hasRenderedFullTrail = false;

        const xExtents: [any, any][] = [];
        const yExtents: [any, any][] = [];
        this.lineData = [];

        for (const instance of this.tool.instances) {
            const session = instance.session;
            if (!data || !data[session.sessionId]) {
                continue;
            }

            const subdata: any[] = data[session.sessionId];

            if (typeof subdata === 'string') {
                this.errors[session.name] = data[session.sessionId];
                this.hasErrors = true;
            } else if (subdata && subdata.length > 0) {
                this.speedExtents[instance.session.sessionId] = d3.extent(subdata, d => d.speed);
                xExtents.push(d3.extent(subdata, d => d.x));
                yExtents.push(d3.extent(subdata, d => d.y));
                this.lineData.push({ session, data: subdata, instance });
            }
        }

        if (xExtents.length > 0) {
            let minX = _.minBy(xExtents, d => d[0])[0];
            let maxX = _.maxBy(xExtents, d => d[1])[1];
            let rangeX = Math.abs(maxX - minX);

            let minY = _.minBy(yExtents, d => d[0])[0];
            let maxY = _.maxBy(yExtents, d => d[1])[1];
            let rangeY = Math.abs(maxY - minY);

            let ratio = rangeY / rangeX;

            // add padding if ratio is too low
            if (ratio < 0.4) {
                const diff = Math.abs(rangeY - (rangeX * 0.5));
                rangeY = rangeY + diff;
                minY -= diff / 2;
                maxY += diff / 2;

                ratio = rangeY / rangeX;
            } else if (ratio > 0.8) {
                const diff = Math.abs(rangeX - (rangeY * 0.1));
                rangeX = rangeX + diff;
                minX -= diff / 2;
                maxX += diff / 2;

                ratio = rangeY / rangeX;
            }

            this.layout.height = this.layout.getWidth() * ratio;

            this.scaleX = d3.scaleLinear()
                .domain([minX, maxX])
                .nice()
                .range([this.layout.marginLeft, this.layout.getWidth() - this.layout.marginRight]);

            this.xAxis.call(d3.axisTop(this.scaleX));

            this.scaleY = d3.scaleLinear()
                .domain([minY, maxY])
                .nice()
                .range([this.layout.height + this.layout.marginBottom, this.layout.marginTop]);

            this.yAxis.call(d3.axisLeft(this.scaleY));

            const visWidth = this.layout.getWidth();
            const visHeight = this.layout.height + this.layout.marginTop + this.layout.marginBottom;
            this.canvas
                .style('position', 'absolute')
                .attr('width', visWidth)
                .attr('height', visHeight);

            this.svg
                .attr('viewBox', `0 0 ${visWidth} ${visHeight}`);
        }

        this.redraw();
        this.changeDetector.detectChanges();
    }

    private redraw(): void {
        const showFullTrail = this.tool.parameters.fulltrail.value;

        this.positionVis
            .selectAll('circle')
            .data(this.lineData, (d: any) => d.session.sessionId)
            .join('circle')
            .attr('fill', d => d.instance.color + 'AA')
            .attr('stroke', 'black')
            .attr('stroke-width', 2)
            .attr('cx', d => this.scaleX(this.getCurrentPosition(d.session, d.data).x))
            .attr('cy', d => this.scaleY(this.getCurrentPosition(d.session, d.data).y))
            .attr('r', '6');


        // clear legend
        this.legend.selectAll('rect').remove();
        this.legend.selectAll('text').remove();
        this.defs.selectAll('linearGradient').remove();

        // style legend
        const legendBarWidth = 10;
        const legendBarMargin = 1;
        const legendTextWidth = 50;
        if (this.tool.parameters.speed.value) {
            const legendWidth = legendTextWidth + this.tool.instances.length * (legendBarMargin + legendBarWidth) + 10;
            this.legend
                .attr('transform', `translate(${this.layout.getWidth() - this.layout.marginRight - legendWidth - 10}, ${this.layout.marginTop + 10})`);

            this.legend.append('rect')
                .attr('stroke', 'black')
                .attr('width', legendWidth)
                .attr('height', 200)
                .attr('fill', 'white');
            this.legend.append('text')
                .attr('transform', `translate(10, 20)`)
                .text('fast');
            this.legend.append('text')
                .attr('transform', `translate(10, 190)`)
                .text('slow');
        }


        if (showFullTrail && this.hasRenderedFullTrail) {
            return;
        }

        const ctx = this.context;
        ctx.clearRect(0, 0, this.layout.getWidth(), this.layout.height);
        let legendBarCounter = 0;
        for (const sessionData of this.lineData) {
            const data = this.getData(sessionData.session, sessionData.data);
            const extent = this.speedExtents[sessionData.session.sessionId];
            const totalRange = extent[1] - extent[0];

            for (let i = 1; i < data.length; i++) {
                if (this.tool.parameters.speed.value) {
                    const speedPercent = (data[i].speed - extent[0]) / totalRange;
                    // tslint:disable-next-line: no-bitwise
                    const opacity = (1 << 2) + Math.ceil(speedPercent * 16 * 16).toString(16).slice(1);
                    ctx.strokeStyle = sessionData.instance.color + opacity;
                    ctx.fillStyle = sessionData.instance.color + opacity;

                } else {
                    ctx.strokeStyle = sessionData.instance.color;
                    ctx.fillStyle = sessionData.instance.color;
                }
                ctx.beginPath();
                const x0 = this.scaleX(data[i - 1].x);
                const y0 = this.scaleY(data[i - 1].y);
                ctx.moveTo(x0, y0);
                const x1 = this.scaleX(data[i].x);
                const y1 = this.scaleY(data[i].y);
                ctx.lineTo(x1, y1);
                ctx.stroke();

                // draw direction indicator
                if (i % Math.ceil(this.tool.parameters.traillength.value / 10) === 0) {
                    ctx.beginPath();
                    ctx.moveTo(x1, y1);
                    ctx.lineTo(x0 - 5, y0 - 5);
                    ctx.lineTo(x0 + 5, y0 + 5);
                    ctx.fill();
                }
            }

            // draw legend
            if (this.tool.parameters.speed.value) {
                const gradient = this.defs
                    .append('linearGradient')
                    .attr('id', sessionData.instance.session.sessionId)
                    .attr('x1', '0%')
                    .attr('x2', '0%')
                    .attr('y1', '100%')
                    .attr('y2', '0%');
                gradient.append('stop')
                    .attr('offset', '0%')
                    .attr('stop-color', sessionData.instance.color + '00');
                gradient.append('stop')
                    .attr('offset', '100%')
                    .attr('stop-color', sessionData.instance.color);

                this.legend.append('rect')
                    .attr('width', legendBarWidth)
                    .attr('height', 180)
                    .attr('y', 10)
                    .attr('x', legendTextWidth + (legendBarCounter * (legendBarWidth + legendBarMargin)))
                    .style('fill', `url(#${sessionData.instance.session.sessionId})`);
                legendBarCounter++;
            }
        }
    }

    private getCurrentPosition(session: any, data: any[]): any {
        const notebook = this.reliveService.notebook;
        const playbackTime = Math.min(notebook.playbackTimeSeconds * 1000 + session.startTime, session.endTime);
        const currentTrailIndex = _.sortedIndexBy(data, { timestamp: playbackTime }, d => d.timestamp);
        return data[currentTrailIndex - 1] || { x: 0, y: 0 };
    }

    private getData(session: any, data: any[]): any[] {
        const showFullTrail = this.tool.parameters.fulltrail.value;
        const trailLength = this.tool.parameters.traillength.value * 1000;

        if (showFullTrail) {
            this.hasRenderedFullTrail = true;
            return data;
        } else {
            this.hasRenderedFullTrail = false;
            const notebook = this.reliveService.notebook;
            const playbackTime = notebook.playbackTimeSeconds * 1000 + session.startTime;
            const minTrailIndex = _.sortedIndexBy(data, { timestamp: playbackTime  - trailLength }, d => d.timestamp);
            const maxTrailIndex = _.sortedIndexBy(data, { timestamp: playbackTime }, d => d.timestamp) + 1;
            return data.slice(minTrailIndex, maxTrailIndex - 1);
        }
    }
}
