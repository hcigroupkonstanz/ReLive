export abstract class Service {
    public static readonly Current: Service[] = [];

    public constructor() {
        Service.Current.push(this);
    }

    public async init() {}
}
