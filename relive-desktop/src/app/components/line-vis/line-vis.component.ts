import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ReliveService } from 'src/app/services';
import { Tool } from '../../models';
import _ from 'lodash';
import * as d3 from 'd3';

@Component({
    selector: 'app-line-vis',
    templateUrl: './line-vis.component.html',
    styleUrls: ['./line-vis.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LineVisComponent implements OnInit, OnDestroy {
    @Input() tool: Tool;
    @Input() dataKey: string;
    @ViewChild('visContainer', { static: true }) visContainer: ElementRef;

    hasErrors = false;
    errors = {};

    stats: any = {};

    private ngUnsubscribe = new Subject();

    private svg: d3.Selection<SVGElement, unknown, null, unknown>;
    private scaleX: d3.ScaleLinear<number, number, never>;
    private xAxis: d3.Selection<SVGElement, unknown, null, unknown>;
    private xAxisSmall: d3.Selection<SVGElement, unknown, null, unknown>;
    private scaleY: d3.ScaleLinear<number, number, never> | d3.ScaleBand<string>;
    private yAxis: d3.Selection<SVGElement, unknown, null, unknown>;
    private lineVis: d3.Selection<SVGElement, unknown, null, unknown>;
    private pointVis: d3.Selection<SVGElement, unknown, null, unknown>;
    private cursor: d3.Selection<SVGElement, unknown, null, unknown>;
    private lineData: any[];
    private layout = {
        marginTop: 30,
        marginBottom: 0,
        marginLeft: 50,
        marginRight: 10,
        getWidth: () => this.visContainer.nativeElement.clientWidth,
        height: 600
    };


    constructor(private relive: ReliveService, private changeDetector: ChangeDetectorRef) {
    }


    ngOnInit(): void {
        this.initVis();
        this.tool.data
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(d => this.renderData(this.dataKey ? d[this.dataKey] : d));
        this.relive.notebook.playbackTimeSeconds$
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(() => this.redrawCursor());
        this.tool.onInstancesChanged
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(() => this.updateColor());
    }

    ngOnDestroy(): void {
        this.ngUnsubscribe.next();
        this.ngUnsubscribe.complete();
    }

    private initVis(): void {
        const visWidth = this.layout.getWidth();
        const visHeight = this.layout.height + this.layout.marginTop + this.layout.marginBottom;
        this.svg = d3.select(this.visContainer.nativeElement)
            .append('svg')
            .attr('viewBox', `0 0 ${visWidth} ${visHeight}`); // TODO: add padding better

        this.xAxis = this.svg.append('g')
            .attr('transform', `translate(0, ${this.layout.marginTop})`);

        this.xAxisSmall = this.svg.append('g')
            .attr('transform', `translate(0, ${this.layout.marginTop})`);

        this.yAxis = this.svg.append('g')
            .attr('transform', `translate(${this.layout.getWidth() - this.layout.marginRight}, 0)`);

        const longestSession = _.maxBy(this.relive.getActiveStudy().sessions, s => s.endTime - s.startTime);
        this.scaleX = d3.scaleLinear()
            .domain([0, longestSession.endTime - longestSession.startTime]).nice()
            .range([this.layout.marginLeft, this.layout.getWidth() - this.layout.marginRight]);

        this.lineVis = this.svg.append('g');
        this.pointVis = this.svg.append('g');
        this.cursor = this.svg
            .append('g')
            .append('rect')
            .attr('height', this.layout.height - this.layout.marginTop / 2)
            .attr('transform', `translate(0, ${this.layout.marginTop / 2})`);
    }

    private renderData(data: any): void {
        this.errors = {};
        this.hasErrors = false;

        this.stats = {};

        const xExtents: [any, any][] = [];
        const yExtents: [any, any][] = [];
        const yValues: string[] = [];
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
                const normalizedData = _.map(subdata, (d: any) => {
                    if (typeof d.y === 'string' && yValues.indexOf(d.y) < 0) {
                        yValues.push(d.y);
                    }

                    return {
                        x: d.x - session.startTime,
                        y: d.y || 0,
                        isLast: false
                    };
                });

                // FIXME: quick hacky workaround to get lines to extent to the end of the graph
                if (normalizedData.length > 2) {
                    const last = normalizedData[normalizedData.length - 1];
                    normalizedData.push({
                        x: last.x,
                        y: last.y,
                        isLast: true
                    });
                }

                this.lineData.push({ session, data: normalizedData, instance });

                xExtents.push(d3.extent(normalizedData, d => d.x));
                if (typeof normalizedData[0].y !== 'string') {
                    yExtents.push(d3.extent(normalizedData, d => d.y));
                }
            }
        }


        if (xExtents.length > 0) {
            this.scaleX = d3.scaleLinear()
                .domain([_.minBy(xExtents, d => d[0])[0], _.maxBy(xExtents, d => d[1])[1]])
                .range([this.layout.marginLeft, this.layout.getWidth() - this.layout.marginRight]);

            this.xAxis.call(d3.axisTop(this.scaleX)
                .ticks(20)
                .tickSize(12)
                .tickPadding(2)
                .tickFormat((d: number) => `${String(Math.floor(d / 60 / 60 / 1000)).padStart(2, '0')}:${String(Math.floor(d / 60 / 1000) % 60).padStart(2, '0')}:${String(Math.floor(d / 1000) % 60).padStart(2, '0')}`)
            );

            this.xAxisSmall.call(d3.axisTop(this.scaleX)
                .ticks(50)
                .tickPadding(2)
                .tickFormat(() => '')
            );

            // Remove horizontal bar
            this.xAxis.select('.domain').attr('stroke-width', 0);

            let line;
            if (typeof this.lineData[0].data[0].y === 'string') {
                this.scaleY = d3.scaleBand()
                    .domain(yValues)
                    .range([this.layout.height - this.layout.marginBottom, this.layout.marginTop]);
                this.yAxis.call(d3.axisLeft(this.scaleY)
                    .tickSize(this.layout.getWidth() - this.layout.marginLeft - this.layout.marginRight));

                line = d3.line()
                    .x((datum: any) => this.scaleX(datum.x))
                    .y((datum: any) => this.scaleY(datum.y) + (this.scaleY as d3.ScaleBand<string>).bandwidth() / 2);


                for (const instance of this.tool.instances) {
                    const stat: any = {};
                    this.stats[instance.session.sessionId] = stat;
                    stat.type = 'string';
                }
            } else {
                const min = _.minBy(yExtents, d => d[0])[0];
                const max = _.maxBy(yExtents, d => d[1])[1];
                this.scaleY = d3.scaleLinear()
                    .domain([min, max]).nice()
                    .range([this.layout.height - this.layout.marginBottom, this.layout.marginTop]);
                this.yAxis.call(d3.axisLeft(this.scaleY)
                    .tickSize(this.layout.getWidth() - this.layout.marginLeft - this.layout.marginRight));

                // Add the line
                line = d3.line()
                    .x((datum: any) => datum.isLast ? this.layout.getWidth() : this.scaleX(datum.x))
                    .y((datum: any) => this.scaleY(datum.y));


                for (const instance of this.tool.instances) {
                    const stat: any = {};
                    this.stats[instance.session.sessionId] = stat;
                    stat.type = 'number';
                    const d = _.find(this.lineData, l => l.instance === instance)?.data;
                    if (d) {
                        const x = this.stdev(d);
                        stat.sd = x.stdev || 0;
                        stat.mean = x.avg || 0;
                        stat.median = this.median(d) || 0;
                    }
                }

                let dataCounter = 0;
                const totalData = [];
                for (const instance of this.tool.instances) {
                    const ds = _.find(this.lineData, l => l.instance === instance)?.data;
                    if (ds) {
                        dataCounter++;
                        for (const d of ds || []) {
                            totalData.push(d);
                        }
                    }
                }

                if (dataCounter > 1) {
                    if (!this.stats.total) {
                        this.stats.total = {};
                    }

                    const x = this.stdev(totalData);
                    this.stats.total.sd = x.stdev || 0;
                    this.stats.total.mean = x.avg || 0;
                    this.stats.total.median = this.median(totalData) || 0;
                }

            }



            // Styling
            this.yAxis.select('.domain').attr('stroke-width', 0);
            this.yAxis.selectAll('.tick line')
                .attr('stroke', '#DFE0EB');

            this.lineVis
                .selectAll('path')
                .data(this.lineData, (d: any) => d.session.sessionId)
                .join('path')
                .attr('fill', 'none')
                .attr('stroke', (d: any) => d.instance.color)
                .attr('stroke-width', 1.5)
                .attr('d', (d: any) => line(d.data));

            // add dots
            this.pointVis.selectAll('circle').remove();
            for (const ld of this.lineData) {
                if (ld.data.length === 1) {
                    const d = ld.data[0];
                    this.pointVis.append('circle')
                        .attr('cx', this.scaleX(d.x))
                        .attr('cy', this.scaleY(d.y))
                        .attr('r', 10)
                        .attr('stroke', 'black')
                        .attr('fill', ld?.instance?.color);
                }
            }

        }

        this.redrawCursor();
        this.changeDetector.detectChanges();
    }

    private updateColor(): void {
        if (this.lineVis && this.lineData) {
            this.lineVis
                .selectAll('path')
                .data(this.lineData, (d: any) => d.session.sessionId)
                .join('path')
                .attr('stroke', (d: any) => d.instance.color);
            this.changeDetector.detectChanges();

            // add dots
            this.pointVis.selectAll('circle').remove();
            for (const ld of this.lineData) {
                if (ld.data.length === 1) {
                    const d = ld.data[0];
                    this.pointVis.append('circle')
                        .attr('cx', this.scaleX(d.x))
                        .attr('cy', this.scaleY(d.y))
                        .attr('r', 10)
                        .attr('stroke', 'black')
                        .attr('fill', ld?.instance?.color);
                }
            }
        }
    }

    private redrawCursor(): void {
        const cursorWidth = 1;
        this.cursor
            .attr('width', cursorWidth)
            .attr('x', this.scaleX(this.relive.notebook.playbackTimeSeconds * 1000) - (cursorWidth / 2))
            .attr('y', 0);

        // process current values for UI
        for (const instance of this.tool.instances) {
            const stats = this.stats[instance.session.sessionId];
            const line = _.find(this.lineData, ld => ld.instance === instance);
            if (stats && line) {
                const playbackTime = this.relive.notebook.playbackTimeSeconds * 1000;
                const currentDataIndex = _.sortedIndexBy(line.data, { x: playbackTime }, d => d.x);
                if (currentDataIndex - 1 < line.data.length) {
                    const data = line.data[Math.max(currentDataIndex - 1, 0)];
                    if (data) {
                        stats.current = data.y.toLocaleString(undefined, { minimumFractionDigits: 2 });
                    } else {
                        stats.current = 'N/A';
                    }
                } else {
                    stats.current = 'N/A';
                }
            }
        }

        this.changeDetector.detectChanges();
    }

    private median(data: any[]): number {
        if (data.length < 2) {
            return 0;
        }

        const sortedData = _.sortBy(data, 'y');
        const half = Math.floor(data.length / 2);

        if (data.length % 2) {
            return sortedData[half].y;
        }

        return (sortedData[half - 1].y + sortedData[half].y) / 2.0;
    }

    private stdev(data: any[]): { avg: number, stdev: number} {
        const avg = _.sumBy(data, d => d.y) / data.length;
        return {
            avg,
            stdev: Math.sqrt(_.sumBy(_.map(data, (i) => Math.pow((i.y - avg), 2))) / data.length)
        };
    }
}
