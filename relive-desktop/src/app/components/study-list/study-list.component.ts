import { ReliveService } from './../../services/relive.service';
import { Component, OnInit } from '@angular/core';
import { PlaybackStudy } from '../../models';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';
import _ from 'lodash';
import { interval } from 'rxjs';

@Component({
    templateUrl: './study-list.component.html',
    styleUrls: ['./study-list.component.css']
})
export class StudyListComponent {
    studies: PlaybackStudy[] = [];

    constructor(public reliveService: ReliveService, private router: Router) {
        if (!environment.skipStudySelection) {
            this.studies = this.reliveService.studies;
        } else {
            const subscription = interval(100)
                .subscribe(() => {
                    const activeStudy = _.find(reliveService.studies, s => s.isActive);
                    if (activeStudy) {
                        this.router.navigate([`/studies/${activeStudy.name}`]);
                        subscription.unsubscribe();
                    }
                });
        }
        if (environment.skipStudySelection && !!this.reliveService.getActiveStudy()) {
        }
    }
}
