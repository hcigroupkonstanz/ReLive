import { ToolComponent } from './tool.component';
import { ChangeDetectionStrategy, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Tool } from '../../models';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import * as d3 from 'd3';
import _ from 'lodash';

type VisData = { duration: number, sessionName: string, color: string };

@Component({
    template: `<div class="noselect" #visContainer></div>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventTimerComponent implements ToolComponent, OnInit, OnDestroy {
    public tool: Tool;
    @ViewChild('visContainer', { static: true }) visContainer: ElementRef<HTMLElement>;

    private ngUnsubscribe = new Subject();

    private svg: d3.Selection<SVGElement, unknown, null, unknown>;
    private xAxis: d3.Selection<SVGElement, unknown, null, unknown>;
    private yAxis: d3.Selection<SVGElement, unknown, null, unknown>;
    private chartVis: d3.Selection<SVGElement, unknown, null, unknown>;
    private textVis: d3.Selection<SVGElement, unknown, null, unknown>;
    private errorVis: d3.Selection<SVGElement, unknown, null, unknown>;
    private layout = {
        marginTop: 50,
        marginBottom: 10,
        marginLeft: 100,
        marginRight: 80,
        getWidth: () => this.visContainer.nativeElement.clientWidth,
        barHeight: 40,
        getHeight: () => (this.tool.instances.length + 1) * this.layout.barHeight
    };

    constructor() {}

    ngOnInit(): void {
        this.initVis();

        this.tool.onInstancesChanged.pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => this.redrawVis());
        this.tool.onEventsChanged.pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => this.redrawVis());

        this.redrawVis();
    }

    ngOnDestroy(): void {
        this.ngUnsubscribe.next();
        this.ngUnsubscribe.complete();
    }

    private initVis(): void {
        this.svg = d3.select(this.visContainer.nativeElement).append('svg');
        this.chartVis = this.svg.append('g');
        this.textVis = this.svg.append('g');
        this.errorVis = this.svg.append('g');
        this.xAxis = this.svg.append('g').attr('transform', `translate(${this.layout.marginLeft}, ${this.layout.marginTop})`);
        this.yAxis = this.svg.append('g').attr('transform', `translate(${this.layout.marginLeft}, ${this.layout.marginTop})`);
    }

    private redrawVis(): void {
        const visWidth = this.layout.getWidth() + this.layout.marginLeft + this.layout.marginRight;
        const visHeight = this.layout.getHeight() + this.layout.marginTop + this.layout.marginBottom;
        this.svg
            .attr('viewBox', `0 0 ${visWidth} ${visHeight}`);

        let longestDuration = 0;
        const data: VisData[] = [];
        const errors: VisData[] = [];

        if (this.tool.events.length === 1) {
            for (const instance of this.tool.instances) {
                const event = _.find(this.tool.events[0].events, e => e.sessionId === instance.session.sessionId);
                if (event && event.endTimestamp) {
                    const duration = event.endTimestamp - event.timestamp;
                    longestDuration = Math.max(longestDuration, duration);
                    data.push({
                        sessionName: instance.session.name,
                        color: instance.color,
                        duration
                    });
                } else {
                    errors.push({
                        sessionName: instance.session.name,
                        color: instance.color,
                        duration: 0
                    });
                }
            }
        } else if (this.tool.events.length === 2) {
            for (const instance of this.tool.instances) {
                const ev1 = _.find(this.tool.events[0].events, e => e.sessionId === instance.session.sessionId);
                const ev2 = _.find(this.tool.events[1].events, e => e.sessionId === instance.session.sessionId);

                if (ev1 && ev2) {
                    const duration = Math.abs(ev1.timestamp - ev2.timestamp);
                    longestDuration = Math.max(longestDuration, duration);
                    data.push({
                        sessionName: instance.session.name,
                        color: instance.color,
                        duration
                    });
                } else {
                    errors.push({
                        sessionName: instance.session.name,
                        color: instance.color,
                        duration: 0
                    });
                }
            }
        }

        const scaleX = d3.scaleLinear()
            .domain([0, longestDuration])
            .range([0, this.layout.getWidth()])
            .nice();
        this.xAxis
            .call(d3.axisTop(scaleX)
                .ticks(10)
                .tickPadding(2)
                .tickSize(20)
                .tickFormat((d: number) => `${String(Math.floor(d / 60 / 60 / 1000)).padStart(2, '0')}:${String(Math.floor(d / 60 / 1000) % 60).padStart(2, '0')}:${String(Math.floor(d / 1000) % 60).padStart(2, '0')}`))
            .selectAll('text')
                .style('font', '14px "Roboto"');

        const scaleY = d3.scaleBand()
            .range([ 0, this.layout.getHeight() ])
            .domain(_.map(this.tool.instances, i => i.session.name))
            .padding(0.2);
        this.yAxis
            .call(d3.axisLeft(scaleY))
            .selectAll('text')
                .style('font', '14px "Roboto"')
                .style('text-anchor', 'end');


        const vis = this.chartVis.selectAll('rect')
            .data(data, (d: VisData) => d.sessionName);

        vis.exit().remove();

        // bars
        vis.enter().append('rect')
                .attr('x', this.layout.marginLeft)
                .attr('y', (d, i) => scaleY(d.sessionName) + this.layout.marginTop)
                .attr('height', this.layout.barHeight)
                .attr('width', d => scaleX(d.duration))
                .attr('fill', d => d.color);
        vis.join('rect')
            .attr('fill', d => d.color)
            .attr('x', this.layout.marginLeft)
            .attr('y', (d, i) => scaleY(d.sessionName) + this.layout.marginTop)
            .attr('height', this.layout.barHeight)
            .attr('width', d => scaleX(d.duration));

        // text
        const textVis = this.textVis.selectAll('text')
            .data(data, (d: VisData) => d.sessionName);

        textVis.enter().append('text')
            .style('vertical-align', 'center');

        textVis.join('text')
            .attr('x', d => this.layout.marginLeft + scaleX(d.duration) + 5)
            .attr('y', (d, i) => scaleY(d.sessionName) + this.layout.marginTop + scaleY.bandwidth() / 2 + 6)
            .text(d => d3.timeFormat('%M:%S')(new Date(d.duration)));


        // errors
        const err = this.errorVis.selectAll('text')
            .data(errors, (d: VisData) => d.sessionName);
        err.exit().remove();
        err.enter().append('text')
            .text('No data available')
            .attr('font', '14px "Roboto"');
        err.join('text')
            .attr('x', this.layout.marginLeft + 10)
            .attr('y', (d, i) => scaleY(d.sessionName) + this.layout.marginTop + scaleY.bandwidth() / 2 + 6);
    }
}
