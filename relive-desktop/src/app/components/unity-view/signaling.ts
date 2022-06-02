import StreamingUrl from './streamingUrl';

export default class Signaling extends EventTarget {

    private interval = 3000;
    private sessionId: string;
    private connectionId: any;
    private sleep = msec => new Promise(resolve => setTimeout(resolve, msec));

    constructor() {
        super();
    }

    private headers(): any {
        if (this.sessionId !== undefined) {
            return { 'Content-Type': 'application/json', 'Session-Id': this.sessionId };
        }
        else {
            return { 'Content-Type': 'application/json' };
        }
    }

    private url(method): any {
        return StreamingUrl + '/signaling/' + method;
    }

    public async start(): Promise<void> {
        const createResponse = await fetch(this.url(''), { method: 'PUT', headers: this.headers() });
        const session = await createResponse.json();
        this.sessionId = session.sessionId;

        const res = await this.createConnection();
        const connection = await res.json();
        this.connectionId = connection.connectionId;

        this.loopGetOffer();
        this.loopGetAnswer();
        this.loopGetCandidate();
    }

    private async loopGetOffer(): Promise<void> {
        let lastTimeRequest = Date.now() - 30000;

        while (true) {
            const res = await this.getOffer(lastTimeRequest);
            lastTimeRequest = Date.parse(res.headers.get('Date'));

            const data = await res.json();
            const offers = data.offers;

            offers.forEach(offer => {
                this.dispatchEvent(new CustomEvent('offer', { detail: offer }));
            });

            await this.sleep(this.interval);
        }
    }

    private async loopGetAnswer(): Promise<void> {
        // receive answer message from 30secs ago
        let lastTimeRequest = Date.now() - 30000;

        while (true) {
            const res = await this.getAnswer(lastTimeRequest);
            lastTimeRequest = Date.parse(res.headers.get('Date'));

            const data = await res.json();
            const answers = data.answers;

            answers.forEach(answer => {
                this.dispatchEvent(new CustomEvent('answer', { detail: answer }));
            });

            await this.sleep(this.interval);
        }
    }

    private async loopGetCandidate(): Promise<void> {
        // receive answer message from 30secs ago
        let lastTimeRequest = Date.now() - 30000;

        while (true) {
            const res = await this.getCandidate(lastTimeRequest);
            lastTimeRequest = Date.parse(res.headers.get('Date'));

            const data = await res.json();
            const candidates = data.candidates.filter(v => v.connectionId = this.connectionId);

            if (candidates.length > 0) {
                for (const candidate of candidates[0].candidates) {
                    this.dispatchEvent(new CustomEvent('candidate', { detail: candidate }));
                }
            }

            await this.sleep(this.interval);
        }
    }

    public async stop(): Promise<void> {
        await this.deleteConnection();
        this.connectionId = null;
        await fetch(this.url(''), { method: 'DELETE', headers: this.headers() });
        this.sessionId = null;
    }

    private async createConnection(): Promise<any> {
        return await fetch(this.url('connection'), { method: 'PUT', headers: this.headers() });
    }
    private async deleteConnection(): Promise<any> {
        const data = { connectionId: this.connectionId };
        return await fetch(this.url('connection'), { method: 'DELETE', headers: this.headers(), body: JSON.stringify(data) });
    }

    public async sendOffer(sdp): Promise<any> {
        const data = { sdp, connectionId: this.connectionId };
        await fetch(this.url('offer'), { method: 'POST', headers: this.headers(), body: JSON.stringify(data) });
    }

    public async sendAnswer(sdp): Promise<any> {
        const data = { sdp, connectionId: this.connectionId };
        await fetch(this.url('answer'), { method: 'POST', headers: this.headers(), body: JSON.stringify(data) });
    }

    public async sendCandidate(candidate, sdpMid, sdpMLineIndex): Promise<any> {
        const data = {
            candidate,
            sdpMLineIndex,
            sdpMid,
            connectionId: this.connectionId
        };
        await fetch(this.url('candidate'), { method: 'POST', headers: this.headers(), body: JSON.stringify(data) });
    }

    private async getOffer(fromTime = 0): Promise<any> {
        return await fetch(this.url(`offer?fromtime=${fromTime}`), { method: 'GET', headers: this.headers() });
    }
    private async getAnswer(fromTime = 0): Promise<any> {
        return await fetch(this.url(`answer?fromtime=${fromTime}`), { method: 'GET', headers: this.headers() });
    }
    private async getCandidate(fromTime = 0): Promise<any> {
        return await fetch(this.url(`candidate?fromtime=${fromTime}`), { method: 'GET', headers: this.headers() });
    }
}

// export class WebSocketSignaling extends EventTarget {

//     constructor() {
//         super();

//         if (location.protocol === "https:") {
//             var websocketUrl = "wss://" + location.host;
//         } else {
//             var websocketUrl = "ws://" + location.host;
//         }

//         this.websocket = new WebSocket(websocketUrl);
//         this.connectionId = null;

//         this.websocket.onopen = () => {
//             this.websocket.send(JSON.stringify({ type: "connect" }));
//         }

//         this.websocket.onmessage = (event) => {
//             const msg = JSON.parse(event.data);
//             if (!msg || !this) {
//                 return;
//             }

//             console.log(msg);

//             switch (msg.type) {
//                 case "connect":
//                     this.connectionId = msg.connectionId;
//                     break;
//                 case "disconnect":
//                     break;
//                 case "offer":
//                     this.dispatchEvent(new CustomEvent('offer', { detail: msg.data }));
//                     break;
//                 case "answer":
//                     this.dispatchEvent(new CustomEvent('answer', { detail: msg.data }));
//                     break;
//                 case "candidate":
//                     this.dispatchEvent(new CustomEvent('candidate', { detail: msg.data }));
//                     break;
//                 default:
//                     break;
//             }
//         }
//     }

//     async start() {
//         const sleep = msec => new Promise(resolve => setTimeout(resolve, msec));
//         while (this.connectionId == null) {
//             await sleep(100);
//         }
//     }

//     stop() {
//         this.websocket.send(JSON.stringify({ type: "disconnect", from: this.connectionId }));
//     }

//     sendOffer(sdp) {
//         const data = { 'sdp': sdp, 'connectionId': this.connectionId };
//         const sendJson = JSON.stringify({ type: "offer", from: this.connectionId, data: data });
//         console.log(sendJson);
//         this.websocket.send(sendJson);
//     }

//     sendAnswer(sdp) {
//         const data = { 'sdp': sdp, 'connectionId': this.connectionId };
//         const sendJson = JSON.stringify({ type: "answer", from: this.connectionId, data: data });
//         console.log(sendJson);
//         this.websocket.send(sendJson);
//     }

//     sendCandidate(candidate, sdpMLineIndex, sdpMid) {
//         const data = {
//             'candidate': candidate,
//             'sdpMLineIndex': sdpMLineIndex,
//             'sdpMid': sdpMid,
//             'connectionId': this.connectionId
//         };
//         const sendJson = JSON.stringify({ type: "candidate", from: this.connectionId, data: data });
//         console.log(sendJson);
//         this.websocket.send(sendJson);
//     }
// }
