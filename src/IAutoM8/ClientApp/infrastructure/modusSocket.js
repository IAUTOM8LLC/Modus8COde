/* eslint-disable no-console */
import { HubConnection } from '@aspnet/signalr';

export default class ModusSocket {

    hubConnection = {};
    events = [];
    startEvent = {};

    constructor(config) {
        const { path, enableLogging, events, startEvent } = config;
        
        this.hubConnection = new HubConnection(`${location.protocol}//${location.host}/${path}`);
        this.hubConnection.logging = enableLogging;
        this.events = events;
        this.startEvent = startEvent;
    }

    start() {
        if (this.startEvent) {
            const { name, args } = this.startEvent;
            if (name === 'SubscribeToProjects') {
                this.hubConnection.start()
                    .then(() => {
                        this.hubConnection.invoke(name, args);
                    });
            } else {
                this.hubConnection.start()
                    .then(() => {
                        this.hubConnection.invoke(name, ...args);
                    });
            }
        }
        else {
            this.hubConnection.start();
        }
        this.events.forEach(e => {
            this.hubConnection.on(e.name, e.action);
        })
    }

    stop() {
        this.hubConnection && this.hubConnection.stop();
    }
}
