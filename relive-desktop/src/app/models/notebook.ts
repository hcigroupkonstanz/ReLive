import { BehaviorSubject } from 'rxjs';
import { Syncable } from './syncable';

export type SceneViewType = 'FreeFly' | 'FollowEntity' | 'FollowEvent' | 'ViewEntity' | 'VR' | 'Isometric';

export class Notebook extends Syncable {
    name: string;

    public readonly playbackTimeSeconds$ = new BehaviorSubject<number>(0);
    public get playbackTimeSeconds(): number {
        return this.playbackTimeSeconds$.value;
    }
    public set playbackTimeSeconds(val: number) {
        this.playbackTimeSeconds$.next(val);
    }

    public readonly playbackSpeed$ = new BehaviorSubject<number>(1);
    public get playbackSpeed(): number {
        return this.playbackSpeed$.value;
    }
    public set playbackSpeed(val: number) {
        this.playbackSpeed$.next(val);
    }

    public readonly isPaused$ = new BehaviorSubject<boolean>(true);
    public get isPaused(): boolean {
        return this.isPaused$.value;
    }
    public set isPaused(val: boolean) {
        this.isPaused$.next(val);
    }

    sceneView: SceneViewType;
    sceneViewOptions: any;


    public constructor() {
        super();
    }

    public toJson(): any {
        return {
            name: this.name,
            isPaused: this.isPaused,
            playbackTimeSeconds: this.playbackTimeSeconds,
            playbackSpeed: this.playbackSpeed,
            sceneView: this.sceneView,
            sceneViewOptions: this.sceneViewOptions
        };
    }
}
