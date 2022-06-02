import { Tool } from './Tool';
import * as THREE from 'three';
import _ from 'lodash';
import { ReliveService } from './ReliveService';
import { MessageDistributor } from '../colibri';
import { filter } from 'rxjs/operators';
import logger from '../util/logger';
import { Study, StudyManager } from '../core/StudyManager';
import { Service } from '../core/Service';
import { LoggedState } from '../models';

const toolsModel = 'toolsModel';

const modelUpdate = 'modelUpdate';
const modelDelete = 'modelDelete';
const modelFetch = 'modelFetch';
const modelInit = 'Init';
const modelInitCmd = 'modelInit';
export class ToolProcessor extends Service {

    private tools: Tool[] = [];
    // FIXME: quick workaround to check if tool processing is still valid
    private toolProcessingTickets: {[toolId: string]: string} = {}

    private logStudy: Study;
    private logSessionId: string;

    public constructor(private reliveProcessor: ReliveService, private messageDistributor: MessageDistributor, private studyManager: StudyManager) {
        super();
        this.reliveProcessor.onStudyReset.subscribe(() => {
            while (this.tools.length > 0) {
                this.tools.pop();
            }
        });
        this.logSessionId = process.env.SESSION_ID;

        messageDistributor.messages$.pipe(filter(msg => msg.channel === toolsModel)).subscribe(msg => {
            switch (msg.command) {

                case modelUpdate:
                    let localTool = _.find(this.tools, t => t.id === msg.payload.id);
                    if (localTool) {
                        this.updateTool(localTool, msg.payload);
                    } else {
                        localTool = msg.payload;
                        this.tools.push(msg.payload);
                        this.updateTool(msg.payload);

                        if (this.logStudy) {
                            this.logStudy.entities.updateEntity({
                                entityId: localTool.id,
                                name: localTool.name + localTool.id,
                                sessionId: this.logSessionId
                            });
                        }
                    }
                    this.tools = _.sortBy(this.tools, t => t.notebookIndex);


                    if (this.logStudy) {
                        const state = (_.cloneDeep(localTool) as unknown) as LoggedState;
                        state.sessionId = this.logSessionId;
                        state.parentId = localTool.id;
                        state.stateType = 'entity';
                        state.timestamp = Date.now();
                        state.status = 'active';
                        delete state.data;
                        this.logStudy.states.addState(state);
                    }

                    // redirect to clients
                    messageDistributor.broadcast(msg);
                    break;

                case modelDelete:
                    // delete local
                    _.remove(this.tools, t => t.id === msg.payload.id);
                    // redirect to clients
                    messageDistributor.broadcast(msg);

                    if (this.logStudy) {
                        this.logStudy.states.addState({
                            parentId: msg.payload.id,
                            sessionId: this.logSessionId,
                            timestamp: Date.now(),
                            stateType: 'entity',
                            status: 'deleted'
                        });
                    }

                    break;

                case modelFetch:
                    messageDistributor.sendMessage({
                        channel: msg.channel + modelInit,
                        command: modelInitCmd,
                        payload: this.tools
                    }, msg.origin);
                    break;

                default:
                    logger.warn(`Unknown command '${msg.command}'`);
            }
        });
    }

    public async init() {
        this.logStudy = await this.studyManager.getStudy(process.env.STUDY_NAME);
    }

    private uuidv4(): string {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    private async updateTool(tool: Tool, updateData: Tool = null) {
        let requiresUpdate = false;

        if (updateData) {
            const hasEntityChanged = !_.isEqual(tool.entities, updateData.entities);
            const hasSessionChanged = !_.isEqual(_.map(tool.instances, i => i.sessionId), _.map(updateData.instances, i => i.sessionId));
            const allEntitiesAvailable = updateData.entities.length >= updateData.minEntities && updateData.instances.length > 0;
            requiresUpdate = allEntitiesAvailable && (hasEntityChanged || hasSessionChanged);

            if (tool.type === 'property' && !_.isEqual(tool.parameters, updateData.parameters)) {
                requiresUpdate = true;
            }

            _.assign(tool, updateData);
        } else {
            requiresUpdate = tool.entities.length >= tool.minEntities && tool.instances.length > 0;
        }

        if (tool.type === 'camera' || tool.type === 'eventTimer') {
            // all data already available on clients
            return;
        }

        if (requiresUpdate) {
            const processingTicket = this.uuidv4();
            this.toolProcessingTickets[tool.id] = processingTicket;

            // send out loading indicator that new data is coming
            delete tool.data;
            tool.isLoading = true;
            this.messageDistributor.broadcast({
                channel: toolsModel,
                command: modelUpdate,
                payload: tool
            });

            let data = {};

            try {
                switch (tool.type) {
                    case 'angle':
                        data = await this.updateAngle(tool);
                        break;

                    case 'trail':
                        data = await this.updateTrail(tool);
                        break;

                    case 'distance':
                        data = await this.updateDistance(tool);
                        break;

                    case 'property':
                        data = await this.updateProperty(tool);
                        break;

                    default:
                        console.warn('Could not provide tool data: Unknown tool name ' + tool.name);
                        break;
                }
            } catch {
                data = {};
                for (const instance of tool.instances) {
                    data[instance.sessionId] = 'Server error';
                }
            }

            tool.isLoading = false;

            const isToolStillActive = this.tools.indexOf(tool) >= 0;
            const isProcessingStillValid = this.toolProcessingTickets[tool.id] === processingTicket;

            if (isToolStillActive && isProcessingStillValid) {
                tool.data = data;
                this.messageDistributor.broadcast({
                    channel: toolsModel,
                    command: modelUpdate,
                    payload: tool
                });
            }
        }
    }

    private async updateDistance(tool: Tool) {
        const study = this.reliveProcessor.currentStudy;
        const result = {};

        for (const instance of tool.instances) {
            const sessionId = instance.sessionId;
            const entities = await study.entities.getEntities(sessionId);

            const entityId1 = _.find(entities, e => e.name === tool.entities[0])?.entityId;
            if (!entityId1) {
                result[sessionId] = 'No matching entity found';
                continue;
            }
            const states1 = await study.states.getEntityStates(sessionId, entityId1);

            const entityId2 = _.find(entities, e => e.name === tool.entities[1])?.entityId;
            if (!entityId2) {
                result[sessionId] = 'No matching entity found';
                continue;
            }
            const states2 = await study.states.getEntityStates(sessionId, entityId2);

            if (states1.length === 0 || states2.length === 0) {
                result[sessionId] = 'No states found';;
            } else {

                const p1 = new THREE.Vector3(0, 0, 0);
                const p2 = new THREE.Vector3(0, 0, 0);

                let i1 = 0;
                let i2 = 0;

                let currentTimestamp = Math.min(states1[0].timestamp, states2[0].timestamp);
                const maxTimestamp = Math.max(_.last(states1).timestamp, _.last(states2).timestamp);

                const data: any[] = [];

                while (currentTimestamp < maxTimestamp) {
                    if (states1[i1].position && states2[i2].position) {
                        // TODO: assumes that first state always has position (which *should* be true, in theory)
                        p1.x = states1[i1].position.x;
                        p1.y = states1[i1].position.y;
                        p1.z = states1[i1].position.z;

                        p2.x = states2[i2].position.x;
                        p2.y = states2[i2].position.y;
                        p2.z = states2[i2].position.z;

                        data.push({ x: currentTimestamp, y: p1.distanceTo(p2) });
                    }

                    // search for next state with position
                    let j1 = 1;
                    let foundPosition1 = false;
                    let nextTimestamp1 = 0;
                    while (i1 + j1 < states1.length && !foundPosition1) {
                        if (states1[i1 + j1].position) {
                            nextTimestamp1 = states1[i1 + j1].timestamp;
                            foundPosition1 = true;
                        } else {
                            j1++;
                        }
                    }

                    let j2 = 1;
                    let foundPosition2 = false;
                    let nextTimestamp2 = 0;
                    while (i2 + j2 < states2.length && !foundPosition2) {
                        if (states2[i2 + j2].position) {
                            nextTimestamp2 = states2[i2 + j2].timestamp;
                            foundPosition2 = true;
                        } else {
                            j2++;
                        }
                    }

                    if (foundPosition1 && foundPosition2) {
                        if (nextTimestamp1 <= nextTimestamp2) {
                            i1 += j1;
                            currentTimestamp = nextTimestamp1;
                        }

                        if (nextTimestamp2 <= nextTimestamp1) {
                            i2 += j2;
                            currentTimestamp = nextTimestamp2;
                        }
                    } else if (foundPosition1) {
                        i1 += j1;
                        currentTimestamp = nextTimestamp1;
                    } else if (foundPosition2) {
                        i2 += j2;
                        currentTimestamp = nextTimestamp2;
                    } else {
                        // give up
                        currentTimestamp = maxTimestamp;
                    }
                }

                result[sessionId] = data;
            }
        }

        return result;
    }

    public async updateAngle(tool: Tool): Promise<any> {
        const study = this.reliveProcessor.currentStudy;
        const result = {};

        for (const instance of tool.instances) {
            const sessionId = instance.sessionId;
            const entities = await study.entities.getEntities(sessionId);

            const entityId1 = _.find(entities, e => e.name === tool.entities[0])?.entityId;
            if (!entityId1) {
                result[sessionId] = 'No matching entity found';
                continue;
            }

            const states1 = await study.states.getEntityStates(sessionId, entityId1);

            const entityId2 = _.find(entities, e => e.name === tool.entities[1])?.entityId;
            if (!entityId2) {
                result[sessionId] = 'No matching entity found';
                continue;
            }
            const states2 = await study.states.getEntityStates(sessionId, entityId2);

            if (states1.length === 0 || states2.length === 0) {
                result[sessionId] = 'No states found';
            } else {
                const p1 = new THREE.Vector3(0, 0, 0);
                const p2 = new THREE.Vector3(0, 0, 0);

                let i1 = 0;
                let i2 = 0;

                let currentTimestamp = Math.min(states1[0].timestamp, states2[0].timestamp);
                const maxTimestamp = Math.max(_.last(states1).timestamp, _.last(states2).timestamp);

                const data: any[] = [];

                while (currentTimestamp < maxTimestamp) {
                    // TODO: assumes that first state always has position (which *should* be true, in theory)
                    p1.x = states1[i1]?.position?.x || 0;
                    p1.y = 0;
                    p1.z = states1[i1]?.position?.z || 0;

                    p2.x = states2[i2]?.position?.x || 0;
                    p2.y = 0;
                    p2.z = states2[i2]?.position?.z || 0;

                    data.push({ x: currentTimestamp, y: p1.angleTo(p2) });

                    // search for next state with position
                    let j1 = 1;
                    let foundPosition1 = false;
                    let nextTimestamp1 = 0;
                    while (i1 + j1 < states1.length && !foundPosition1) {
                        if (states1[i1 + j1].position) {
                            nextTimestamp1 = states1[i1 + j1].timestamp;
                            foundPosition1 = true;
                        } else {
                            j1++;
                        }
                    }

                    let j2 = 1;
                    let foundPosition2 = false;
                    let nextTimestamp2 = 0;
                    while (i2 + j2 < states2.length && !foundPosition2) {
                        if (states2[i2 + j2].position) {
                            nextTimestamp2 = states2[i2 + j2].timestamp;
                            foundPosition2 = true;
                        } else {
                            j2++;
                        }
                    }

                    if (foundPosition1 && foundPosition2) {
                        if (nextTimestamp1 <= nextTimestamp2) {
                            i1 += j1;
                            currentTimestamp = nextTimestamp1;
                        }

                        if (nextTimestamp2 <= nextTimestamp1) {
                            i2 += j2;
                            currentTimestamp = nextTimestamp2;
                        }
                    } else if (foundPosition1) {
                        i1 += j1;
                        currentTimestamp = nextTimestamp1;
                    } else if (foundPosition2) {
                        i2 += j2;
                        currentTimestamp = nextTimestamp2;
                    } else {
                        // give up
                        currentTimestamp = maxTimestamp;
                    }
                }

                result[sessionId] = data;
            }
        }

        return result;
    }

    public async updateProperty(tool: Tool): Promise<any> {
        const study = this.reliveProcessor.currentStudy;
        const result = {};
        for (const parameter of Object.keys(tool.parameters)) {
            // skip inactive parameters
            if (tool.parameters[parameter].value) {
                result[parameter] = {};
            }
        }

        for (const instance of tool.instances) {
            const sessionId = instance.sessionId;
            const entities = await study.entities.getEntities(sessionId);

            const entity = _.find(entities, e => e.name === tool.entities[0]);
            if (!entity) {
                for (const parameter of Object.keys(result)) {
                    result[parameter][sessionId] = 'Unknown entity';
                }
                continue;
            }

            const states = await study.states.getEntityStates(sessionId, entity.entityId);

            for (const parameter of Object.keys(result)) {
                if (entity[parameter]) {
                    result[parameter][sessionId] = entity[parameter];
                    result[parameter]['type'] = 'single';
                } else if (parameter === 'speed') {
                    result[parameter][sessionId] = [];
                    result[parameter]['type'] = 'line';
                    let lastState = null;
                    for (const state of states) {
                        if (state.position) {
                            if (lastState != null) {
                                const meters = Math.sqrt(
                                    Math.pow(state.position.x - lastState.position.x, 2) +
                                    Math.pow(state.position.y - lastState.position.y, 2) +
                                    Math.pow(state.position.z - lastState.position.z, 2));
                                
                                const seconds = (state.timestamp - lastState.timestamp) / 1000;

                                if (seconds !== 0) {
                                    const speed = meters / seconds;

                                    result[parameter][sessionId].push({
                                        'x': state.timestamp,
                                        'y': speed
                                    });
                                }
                            }

                            lastState = state;
                        }
                    }
                } else if (parameter === 'distanceMoved') {
                    result[parameter][sessionId] = [];
                    result[parameter]['type'] = 'line';
                    let lastPosition = null;
                    let distanceMoved = 0;
                    for (const state of states) {
                        if (state.position) {
                            if (lastPosition != null) {
                                const meters = Math.sqrt(
                                    Math.pow(state.position.x - lastPosition.x, 2) +
                                    Math.pow(state.position.y - lastPosition.y, 2) +
                                    Math.pow(state.position.z - lastPosition.z, 2));
                                distanceMoved += meters;

                                result[parameter][sessionId].push({
                                    'x': state.timestamp,
                                    'y': distanceMoved
                                });

                            }

                            lastPosition = state.position;
                        }

                    }
                } else if (_.includes(['position', 'rotation', 'scale'], parameter)) {
                    result[parameter]['type'] = '3d';
                    if (!result[parameter + 'X']) {
                        result[parameter + 'X'] = {};
                    }
                    result[parameter + 'X'][sessionId] = [];
                    if (!result[parameter + 'Y']) {
                        result[parameter + 'Y'] = {};
                    }
                    result[parameter + 'Y'][sessionId] = [];
                    if (!result[parameter + 'Z']) {
                        result[parameter + 'Z'] = {};
                    }
                    result[parameter + 'Z'][sessionId] = [];
                    for (const state of states) {
                        if (state[parameter]) {
                            result[parameter + 'X'][sessionId].push({
                                'x': state.timestamp,
                                'y': state[parameter]['x']
                            });
                            result[parameter + 'Y'][sessionId].push({
                                'x': state.timestamp,
                                'y': state[parameter]['y']
                            });
                            result[parameter + 'Z'][sessionId].push({
                                'x': state.timestamp,
                                'y': state[parameter]['z']
                            });
                        }
                    }

                } else {
                    result[parameter][sessionId] = [];
                    result[parameter]['type'] = 'line';
                    const data = result[parameter][sessionId];

                    for (const state of states) {
                        if (state[parameter]) {
                            if (data.length >= 2 && data[data.length - 2].y === data[data.length - 1].y && data[data.length - 1].y === state[parameter]) {
                                data[data.length - 1].x = state.timestamp;
                            } else {
                                data.push({
                                    'x': state.timestamp,
                                    'y': state[parameter]
                                });
                            }
                        } else {
                            if (data.length >= 2 && data[data.length - 2].y === data[data.length - 1].y) {
                                data[data.length - 1].x = state.timestamp;
                            } else if (data.length >= 1) {
                                data.push({
                                    'x': state.timestamp,
                                    'y': data[data.length - 1].y
                                });
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    public async updateTrail(tool: Tool): Promise<any> {
        const study = this.reliveProcessor.currentStudy;
        const result = {};

        for (const instance of tool.instances) {
            const sessionId = instance.sessionId;
            const entities = await study.entities.getEntities(sessionId);

            const entityId = _.find(entities, e => e.name === tool.entities[0])?.entityId;
            if (!entityId) {
                result[sessionId] = 'No matching entity found';
                continue;
            }

            const states = await study.states.getEntityStates(sessionId, entityId);

            if (states.length === 0) {
                result[sessionId] = 'No states found';
            } else {
                const data = [];
                let i = 0;
                let lastState = null;
                for (const state of states) {
                    if (state.position) {
                        if (lastState !== null) {
                            // FIXME: temp workaround to reduce amount of data
                            i++;
                            if (i % 2 == 0) {
                                continue;
                            }

                            const meters = Math.sqrt(
                                Math.pow(state.position.x - lastState.position.x, 2) +
                                Math.pow(state.position.y - lastState.position.y, 2) +
                                Math.pow(state.position.z - lastState.position.z, 2));
                            const seconds = (state.timestamp - lastState.timestamp) / 1000;

                            data.push({
                                'x': state.position.x,
                                'y': state.position.z,
                                'speed': meters / seconds,
                                'timestamp': state.timestamp
                            });
                        }

                        lastState = state;
                    }
                }

                result[sessionId] = data;
            }
        }

        return result;
    }
}
