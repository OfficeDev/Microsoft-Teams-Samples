import Constants from "../constants";

export default class StatusRefresh{
    constructor(tokenService){
        this.tokenService = tokenService;
        this.cancelToken = null;
    }

    start(cb) {
        this.cancelToken = setInterval(async () => {
            cb(await this.tokenService.getMeetingStatusAsync());
        }, Constants.Service.refreshInterval);
    }
    
    stop() {
        clearInterval(this.cancelToken)
    }
}