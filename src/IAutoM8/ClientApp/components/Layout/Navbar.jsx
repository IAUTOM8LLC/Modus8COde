import React, { Component } from "react";
import { connect } from "react-redux";
import { Menu, Input } from "semantic-ui-react";
import cn from "classnames";
import ReactHtmlParser from "react-html-parser";

import { setSearchQuery } from "@store/layout";
import {
    selectSearchQuery,
    selectSearchQueryTrimLowerCase,
} from "@selectors/layout";
import { logoff } from "@store/auth";

import { UserAcronym, TimeAgo } from "@components";

import ModusSocket from "@infrastructure/modusSocket";

import UserMenuItem from "./UserMenuItem";
import NotificationMenuItem from "./NotificationMenuItem";

import "./Navbar.less";

@connect(
    (state) => ({
        searchQuery: selectSearchQuery(state),
        searchQueryTrimLowerCase: selectSearchQueryTrimLowerCase(state),
        roles: state.auth.user.roles
    }),
    { setSearchQuery, logoff }
)
export default class Navbar extends Component {
    search = (e, d) => {
        this.props.setSearchQuery(d.value);
    };

    componentDidMount() {
        const {
            user: { id },
        } = this.props;
        this.createSocketForUser(id);
    }

    componentDidUpdate(prevProps) {
        const {
            user: { id },
        } = this.props;
        if (id !== prevProps.user.id) {
            this.createSocketForUser(id);
        }
    }

    componentWillUnmount() {
        this.createSocketForUser();
    }

    createSocketForUser = (id) => {
        if (!id && id === "") {
            this.socket && this.socket.stop();
            this.props.logoff();
            return;
        }
        this.socket = new ModusSocket({
            path: "login-hub",
            enableLogging: __DEV__,
            events: [
                {
                    name: "logOff",
                    action: () => {
                        this.props.logoff();
                    },
                },
            ],
            startEvent: { name: "SubscribeLogin", args: [id] },
        });
        this.socket.start();
    };

    handleClick = (message) => {
        message.url && this.props.push(message.url);
        this.props.readMessage(message.id);
    };

    render() {
        const {
            user = {},
            onLogoff,
            searchQuery,
            searchQueryTrimLowerCase,
            className,
            roles
        } = this.props;

        const role = roles.toString()
        //console.log('roleee', role);


        //console.log('userprops',this.props);


        const { fullName } = user;

        const searchClassName = cn([
            `${className}-search`,
            (searchQueryTrimLowerCase || searchQuery) &&
                `${className}-search--opened`,
        ]);

        return (
            <Menu borderless secondary className={className}>
                <Menu.Menu position="right">
                    <Menu.Item>
                        <Input
                            className={searchClassName}
                            icon="search"
                            value={searchQuery}
                            placeholder="Search..."
                            onChange={this.search}
                        />
                    </Menu.Item>
                    <NotificationMenuItem />

                    <UserMenuItem role={role} userName={fullName} onLogoff={onLogoff} />
                </Menu.Menu>
            </Menu>
        );
    }
}
