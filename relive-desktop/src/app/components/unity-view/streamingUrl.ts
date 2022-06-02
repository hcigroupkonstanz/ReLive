import { environment } from '../../../environments/environment';

const StreamingUrl = `${window.location.protocol}//${window.location.hostname}:${environment.restPort}/renderstreaming`;
export default StreamingUrl;
