import _ from 'lodash';

export type UpdateHandler = () => void;
export abstract class Syncable {
    private readonly updateHandlers: UpdateHandler[] = [];

    public registerUpdateHandler(handler: UpdateHandler): void {
        this.updateHandlers.push(handler);
    }

    public update(): void {
        this.updateHandlers.slice(0).forEach(h => h());
    }

    public applyUpdate(update: any): void {
        _.assign(this, update);
    }
}
