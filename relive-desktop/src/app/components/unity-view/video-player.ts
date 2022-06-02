// import Signaling, { WebSocketSignaling } from './signaling.js'
import Signaling from './signaling';
import StreamingUrl from './streamingUrl';

// enum type of event sending from Unity
const UnityEventType = {
    SWITCH_VIDEO: 0
};

export class VideoPlayer {
    private peerConnection: RTCPeerConnection = null;
    private cfg: any;
    private localStream: MediaStream;
    private channel: RTCDataChannel = null;
    private offerOptions: any;
    private video: any;
    private signaling: Signaling;

    private videoTrackList: any[];
    private maxVideoTrackLength: number;

    private _videoScale: number;
    private _videoOriginX: number;
    private _videoOriginY: number;

    public constructor(videoElement: HTMLVideoElement, config: any) {
        this.cfg = VideoPlayer.getConfiguration(config);
        this.offerOptions = {
            offerToReceiveAudio: true,
            offerToReceiveVideo: true,
        };

        // main video
        this.localStream = new MediaStream();
        this.video = videoElement;
        this.video.playsInline = true;
        this.video.addEventListener('loadedmetadata', () => {
            this.video.play();
            this.resizeVideo();
        }, true);

        this.videoTrackList = [];
        this.maxVideoTrackLength = 1;

    }

    private static getConfiguration(config: any = {}): any {
        config.sdpSemantics = 'unified-plan';
        config.iceServers = [{ urls: ['stun:stun.l.google.com:19302'] }];
        return config;
    }

    public async setupConnection(): Promise<void> {
        // close current RTCPeerConnection
        if (this.peerConnection) {
            console.log('Close current PeerConnection');
            this.peerConnection.close();
            this.peerConnection = null;
        }

        // Decide Signaling Protocol
        const protocolEndPoint = StreamingUrl + '/protocol';
        const createResponse = await fetch(protocolEndPoint);
        const res = await createResponse.json();

        if (res.useWebSocket) {
            console.error('nyi');
            // this.signaling = new WebSocketSignaling();
        } else {
            this.signaling = new Signaling();
        }

        // Create peerConnection with proxy server and set up handlers
        this.peerConnection = new RTCPeerConnection(this.cfg);
        this.peerConnection.onsignalingstatechange = (e) => {
            console.log('signalingState changed:', e);
        };
        this.peerConnection.oniceconnectionstatechange = (e) => {
            console.log('iceConnectionState changed:', e);
            console.log('pc.iceConnectionState:' + this.peerConnection.iceConnectionState);
            if (this.peerConnection.iceConnectionState === 'disconnected') {
                // this.ondisconnect();
            }
        };

        this.peerConnection.onicegatheringstatechange = (e) => {
            console.log('iceGatheringState changed:', e);
        };

        this.peerConnection.ontrack = (e) => {
            if (e.track.kind === 'video') {
                this.videoTrackList.push(e.track);
            }
            if (e.track.kind === 'audio') {
                this.localStream.addTrack(e.track);
            }
            if (this.videoTrackList.length === this.maxVideoTrackLength) {
                this.switchVideo();
            }
        };
        this.peerConnection.onicecandidate = (e) => {
            if (e.candidate != null) {
                this.signaling.sendCandidate(e.candidate.candidate, e.candidate.sdpMid, e.candidate.sdpMLineIndex);
            }
        };
        // Create data channel with proxy server and set up handlers
        this.channel = this.peerConnection.createDataChannel('data');
        this.channel.onopen = () => {
            console.log('Datachannel connected.');
        };
        this.channel.onerror = (e) => {
            console.log('The error ' + e.error.message + ' occurred\n while handling data with proxy server.');
        };
        this.channel.onclose = () => {
            console.log('Datachannel disconnected.');
        };
        this.channel.onmessage = async (msg) => {
            // receive message from unity and operate message
            let data;
            // receive message data type is blob only on Firefox
            if (navigator.userAgent.indexOf('Firefox') !== -1) {
                data = await msg.data.arrayBuffer();
            } else {
                data = msg.data;
            }
            const bytes = new Uint8Array(data);
            switch (bytes[0]) {
                case UnityEventType.SWITCH_VIDEO:
                    this.switchVideo();
                    break;
            }
        };

        this.signaling.addEventListener('answer', async (e: any) => {
            const answer = e.detail;
            const desc = new RTCSessionDescription({ sdp: answer.sdp, type: 'answer' });
            await this.peerConnection.setRemoteDescription(desc);
        });

        this.signaling.addEventListener('candidate', async (e: any) => {
            const candidate = e.detail;
            const iceCandidate = new RTCIceCandidate({ candidate: candidate.candidate, sdpMid: candidate.sdpMid, sdpMLineIndex: candidate.sdpMLineIndex });
            await this.peerConnection.addIceCandidate(iceCandidate);
        });

        // setup signaling
        await this.signaling.start();

        // Add transceivers to receive multi stream.
        // It can receive two video tracks and one audio track from Unity app.
        // This operation is required to generate offer SDP correctly.
        this.peerConnection.addTransceiver('video', { direction: 'recvonly' });
        this.peerConnection.addTransceiver('video', { direction: 'recvonly' });
        this.peerConnection.addTransceiver('audio', { direction: 'recvonly' });

        // create offer
        const offer = await this.peerConnection.createOffer(this.offerOptions);

        // set local sdp
        const rtcDesc = new RTCSessionDescription({ sdp: offer.sdp, type: 'offer' });
        await this.peerConnection.setLocalDescription(rtcDesc);
        await this.signaling.sendOffer(offer.sdp);
    }

    public resizeVideo(): void {
        const clientRect = this.video.getBoundingClientRect();
        const videoRatio = this.videoWidth / this.videoHeight;
        const clientRatio = clientRect.width / clientRect.height;

        this._videoScale = videoRatio > clientRatio ? clientRect.width / this.videoWidth : clientRect.height / this.videoHeight;
        const videoOffsetX = videoRatio > clientRatio ? 0 : (clientRect.width - this.videoWidth * this._videoScale) * 0.5;
        const videoOffsetY = videoRatio > clientRatio ? (clientRect.height - this.videoHeight * this._videoScale) * 0.5 : 0;
        this._videoOriginX = clientRect.left + videoOffsetX;
        this._videoOriginY = clientRect.top + videoOffsetY;
    }

    // replace video track related the MediaStream
    private replaceTrack(stream, newTrack): void {
        const tracks = stream.getVideoTracks();
        for (const track of tracks) {
            if (track.kind === 'video') {
                stream.removeTrack(track);
            }
        }
        stream.addTrack(newTrack);
    }

    private switchVideo(): void {
        this.video.srcObject = this.localStream;
        this.replaceTrack(this.localStream, this.videoTrackList[0]);
    }

    public get videoWidth(): number {
        return this.video.videoWidth;
    }

    public get videoHeight(): number {
        return this.video.videoHeight;
    }

    public get videoOriginX(): number {
        return this._videoOriginX;
    }

    public get videoOriginY(): number {
        return this._videoOriginY;
    }

    public get videoScale(): number {
        return this._videoScale;
    }

    public close(): void {
        if (this.peerConnection) {
            console.log('Close current PeerConnection');
            this.peerConnection.close();
            this.peerConnection = null;
        }
    }

    public sendMsg(msg): void {
        if (this.channel == null) {
            return;
        }
        switch (this.channel.readyState) {
            case 'connecting':
                console.log('Connection not ready');
                break;
            case 'open':
                this.channel.send(msg);
                break;
            case 'closing':
                console.log('Attempt to sendMsg message while closing');
                break;
            case 'closed':
                console.log('Attempt to sendMsg message while connection closed.');
                break;
        }
    }
}
