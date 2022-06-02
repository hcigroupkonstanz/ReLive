import { Component, OnInit } from '@angular/core';
import { NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Router, RouterEvent } from '@angular/router';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css'],
    selector: 'app-root'
})
export class AppComponent implements OnInit {

    isLoading = true;

    constructor(private router: Router, private iconRegistry: MatIconRegistry, private domSanitizer: DomSanitizer) {
        router.events.subscribe((routerEvent: RouterEvent) => {
            this.checkRouterEvent(routerEvent);
        });


        // add icons
        iconRegistry.addSvgIcon('plus', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light-outline/Plus.svg'));
        iconRegistry.addSvgIcon('edit', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Edit.svg'));
        iconRegistry.addSvgIcon('close', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Close Square.svg'));
        iconRegistry.addSvgIcon('close_nocolor', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Close Square_nocolor.svg'));
        iconRegistry.addSvgIcon('backarrow', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Arrow - Left Square.svg'));
        iconRegistry.addSvgIcon('export', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light-outline/Download.svg'));
        iconRegistry.addSvgIcon('delete', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Delete.svg'));
        iconRegistry.addSvgIcon('filter', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Filter 2.svg'));
        iconRegistry.addSvgIcon('user', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Profile.svg'));

        iconRegistry.addSvgIcon('mouse-left', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/nounproject/mouse_left.svg'));
        iconRegistry.addSvgIcon('mouse-right', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/nounproject/mouse_right.svg'));
        iconRegistry.addSvgIcon('mouse-scroll', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/nounproject/mouse_scroll.svg'));

        iconRegistry.addSvgIcon('vr', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/nounproject/vr.svg'));
        iconRegistry.addSvgIcon('isometric', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/nounproject/isometric.svg'));
        iconRegistry.addSvgIcon('perspective', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/nounproject/perspective.svg'));
        iconRegistry.addSvgIcon('focus', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/nounproject/focus.svg'));

        iconRegistry.addSvgIcon('duplicate', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/custom/Copy.svg'));

        iconRegistry.addSvgIcon('tag', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Folder.svg'));
        iconRegistry.addSvgIcon('task', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Paper.svg'));

        iconRegistry.addSvgIcon('expand-more', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light-outline/Arrow - Down 2.svg'));
        iconRegistry.addSvgIcon('expand-less', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light-outline/Arrow - Up 2.svg'));

        iconRegistry.addSvgIcon('audio-on', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Volume Up.svg'));
        iconRegistry.addSvgIcon('audio-off', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Volume Off.svg'));
        iconRegistry.addSvgIcon('video-on', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Video.svg'));
        iconRegistry.addSvgIcon('video-off', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/custom/Video-off.svg'));
        iconRegistry.addSvgIcon('show-on', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Show.svg'));
        iconRegistry.addSvgIcon('show-off', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/Light/Hide.svg'));

    }

    ngOnInit(): void {
    }


    checkRouterEvent(routerEvent: RouterEvent): void {
        if (routerEvent instanceof NavigationStart) {
            this.isLoading = true;
        }

        if (routerEvent instanceof NavigationEnd ||
            routerEvent instanceof NavigationCancel ||
            routerEvent instanceof NavigationError) {
            this.isLoading = false;
        }
    }

}
