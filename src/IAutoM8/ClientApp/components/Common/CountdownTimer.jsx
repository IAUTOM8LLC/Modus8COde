import React from "react";
import moment from 'moment';

import Axios from "axios";

import { getAuthHeaders } from "@infrastructure/auth";

class CountdownTimer extends React.Component {
    constructor(props) {
        super(props);

        this.secondsRemaining = this.props.secondsRemaining;
        const hours = Math.floor(this.secondsRemaining / (60 * 60));
        const minutes = Math.floor((this.secondsRemaining / 60) % 60);
        const seconds = Math.floor(this.secondsRemaining % 60);

        this.state = {
            hours: String(hours < 10 ? `0${hours}` : hours),
            minutes: String(minutes < 10 ? `0${minutes}` : minutes),
            seconds: String(seconds < 10 ? `0${seconds}` : seconds),
            color: this.props.color != undefined ? this.props.color : "#c3922e",
            eta80: this.props.eta80,
        };
    }


    componentDidMount() {
        this.interval = setInterval(() => this.tick(), 1000);
    }

    componentDidUpdate(prevProps) {
        // To update the counter after API response
        if (prevProps.secondsRemaining != this.props.secondsRemaining) {
            this.secondsRemaining = this.props.secondsRemaining
        }
    }

    componentWillUnmount() {
        clearInterval(this.interval);
    }

    sendNotification = () => {
        Axios.get(`/api/Vendor/sendetanotification/${this.props.id}`, getAuthHeaders())
            .then((response) => {
            })
            .catch((error) => {

            });
    };

    tick() {

        if (this.props.iS_PAST80_ETA_10 == true) {
            this.setState({ color: "red" });
        }

        const todaysDate = moment(new Date().toISOString());

        const sDate = moment(this.props.startDate);

        const dateDiffInSecs = todaysDate.diff(sDate, "seconds");

        if (dateDiffInSecs > this.state.eta80) {
            this.setState({ color: "red" });
        }


        const hours = Math.floor(this.secondsRemaining / (60 * 60));
        const minutes = Math.floor((this.secondsRemaining / 60) % 60);
        const seconds = Math.floor(this.secondsRemaining % 60);

        this.setState({
            hours: hours,
            minutes: minutes,
            seconds: seconds,
        });

        if (seconds < 10) {
            this.setState({
                seconds: `0${this.state.seconds}`,
            });
        }

        if (minutes < 10) {
            this.setState({
                minutes: `0${this.state.minutes}`,
            });
        }

        if (hours < 10) {
            this.setState({
                hours: `0${this.state.hours}`,
            });
        }

        if (this.secondsRemaining === 0) {
            clearInterval(this.interval);
            this.setState({ color: "#c3922e" });
            this.sendNotification();
            if (this.props.onPendingInviteRefresh != undefined) {
                this.props.onPendingInviteRefresh(this.props.taskId);
            }
        }

        this.secondsRemaining--;
    }

    render() {
        return (
            <div style={{ fontWeight: "900", fontSize: "1.2em", color: this.state.color }}>
                {this.state.hours}:{ this.state.minutes}:{ this.state.seconds}
            </div >
        );
    }
}

export default CountdownTimer;
