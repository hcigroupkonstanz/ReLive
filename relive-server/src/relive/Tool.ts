export interface Tool {
    type: string;
    id: string;

    name: string;
    minEntities: number;
    maxEntities: number;
    renderVisualization: boolean;
    parameters: { [key: string]: any };

    entities: string[]; // name of shared entities
    instances: { sessionId: string, color: string }[];

    isLoading: boolean;
    notebookIndex: number; // FIXME: workaround to sync position
    data: any;
}
