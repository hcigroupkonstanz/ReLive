import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { ReliveService, ToolsService } from './services';

@Injectable({ providedIn: 'root' })
export class ReliveResolver implements Resolve<any> {
    constructor(private relive: ReliveService, private tools: ToolsService) { }

    async resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<any> {
        const studyId = route.paramMap.get('studyId');
        console.log(`resolving ${studyId}`);

        await this.relive.initializeRelive(studyId);
        await this.tools.initialize();
    }
}
