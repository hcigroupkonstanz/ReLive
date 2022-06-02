import { VideoAttachmentComponent } from './components/video-attachment/video-attachment.component';
import { VideoViewComponent } from './components/video-view/video-view.component';
import { FrustumToolComponent } from './components/tools/frustum-tool.component';
import { TrailToolComponent } from './components/tools/trail-tool.component';
import { CameraToolComponent } from './components/tools/camera-tool.component';
import { EventTimerComponent } from './components/tools/event-timer.component';
import { ActiveSessionsPipe } from './pipes/active-sessions.pipe';
import { ToolDirective } from './directives/tool.directive';
import { DragEntityDirective } from './directives/draggableentity.directive';
import { HttpClientModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { NotebookComponent } from './components/notebook/notebook.component';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './components/app/app.component';
import { StudyListComponent } from './components/study-list/study-list.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ToolHostComponent } from './components/tool-host/tool-host.component';
import { TimesliderComponent } from './components/timeslider/timeslider.component';
import { SocketIoConfig, SocketIoModule } from 'ngx-socket-io';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { DndModule } from 'ngx-drag-drop';
import { MatTooltipModule } from '@angular/material/tooltip';

import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { UnityViewComponent } from './components/unity-view/unity-view.component';
import { AngleToolComponent } from './components/tools/angle-tool.component';
import { DefaultToolComponent } from './components/tools/default-tool.component';
import { DistanceToolComponent } from './components/tools/distance-tool.component';
import { PropertyToolComponent } from './components/tools/property-tool.component';
import { AngularResizedEventModule } from 'angular-resize-event';
import { TimelineComponent } from './components/timeline/timeline.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { NgScrollbarModule } from 'ngx-scrollbar';
import { AddToolComponent } from './components/add-tool/add-tool.component';
import { ChipComponent } from './components/chip/chip.component';
import { LineVisComponent } from './components/line-vis/line-vis.component';
import { environment } from './../environments/environment';

const apiUrl = `${window.location.protocol}//${window.location.hostname}:${environment.socketPort}`;
const config: SocketIoConfig = { url: apiUrl, options: { withCredentials: false } };

@NgModule({
    declarations: [
        AppComponent,
        StudyListComponent,
        ToolHostComponent,
        ToolDirective,
        DragEntityDirective,
        TimesliderComponent,
        ActiveSessionsPipe,
        NotebookComponent,
        UnityViewComponent,
        TimelineComponent,
        AddToolComponent,
        ChipComponent,
        VideoAttachmentComponent,
        VideoViewComponent,

        // Dynamically loaded tools
        AngleToolComponent,
        DefaultToolComponent,
        DistanceToolComponent,
        PropertyToolComponent,
        TrailToolComponent,
        FrustumToolComponent,
        CameraToolComponent,
        EventTimerComponent,
        LineVisComponent
    ],
    imports: [
        BrowserModule,
        HttpClientModule,
        AppRoutingModule,
        FormsModule,
        SocketIoModule.forRoot(config),
        BrowserAnimationsModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatChipsModule,
        MatExpansionModule,
        MatDividerModule,
        MatMenuModule,
        DndModule,
        AngularResizedEventModule,
        NgScrollbarModule,
        MatTooltipModule,
        MatCheckboxModule,

        // color picker
        ReactiveFormsModule,
        MatFormFieldModule
    ],
    providers: [ ],
    bootstrap: [AppComponent]
})
export class AppModule { }
