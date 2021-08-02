import React, { Component } from 'react'
import { Popup } from 'semantic-ui-react'
import moment from 'moment'
import cn from 'classnames'

import splitToLowerCase from '@utils/splitToLowerCase'

import { ModusButton, TaskHeader, TaskDescription, TaskFooter, WhenPermitted } from '@components'


import './EventPopup.less'

export default class EventPopup extends Component {

    constructor(props) {
        super(props);

        this.state = {
            popupOpen: false
        }
    }

    handleOpen = () => {
        this.setState({ popupOpen: true })
    }

    handleClose = () => {
        this.setState({ popupOpen: false })
    }

    handleEditProxy = () => {
        this.handleClose();
        this.props.onEdit(this.props.event.id);
    }

    handleDeleteProxy = () => {
        this.handleClose();
        this.props.onDelete(this.props.event.id);
    }
    handleOutsourceTabProxy = () => {
        this.handleClose();
        this.props.onOutsourceTabOpen();
    }

    render() {
        const { children, task, onOutsourceTabOpen } = this.props;

        const {
            title,
            status,
            startDate,
            duration,
            description,
            proccessingUserName,
            reviewingUserName,
            isOverdue,
            isConditional,
            isInterval,
            isRecurrent,

            hasVendor,
            hasAssignedVendor
        } = task;

        const dueDate = moment(startDate).add(duration, 'm').toDate();
        const statusClass = splitToLowerCase(status);
        const buttonsClassName = cn([
            'calendar-event__buttons',
            `calendar-event__buttons--${statusClass}`]);

        return (
            <Popup
                flowing
                className={`calendar-event-popup ${statusClass}`}
                trigger={children}
                on="click"
                position="bottom center"
                open={this.state.popupOpen}
                onOpen={this.handleOpen}
                onClose={this.handleClose}
            >
                <div className="calendar-event">
                    <div className="calendar-event__body">
                        <div className="calendar-event__item-header">
                            <TaskHeader
                                title={title}
                                status={status}
                                username={reviewingUserName || proccessingUserName}
                                doing={proccessingUserName !== null}
                                reviewing={reviewingUserName !== null}
                            />
                        </div>

                        <TaskDescription description={description} />

                        <TaskFooter
                            dueDate={dueDate}
                            isOverdue={isOverdue}
                            isConditional={isConditional}
                            isRecurrent={isRecurrent}
                            isInterval={isInterval}
                            hasVendor={hasVendor}
                            hasAssignedVendor={hasAssignedVendor}
                            onOutsourceTabOpen={this.handleOutsourceTabProxy}
                        />
                    </div>

                    <div className={buttonsClassName}>
                        <WhenPermitted rule="canChangeProcessingUser">
                            {(permitted) =>
                                <ModusButton
                                    icon="edit"
                                    onClick={this.handleEditProxy}
                                    content={permitted ? "Edit" : "View"}
                                />
                            }
                        </WhenPermitted>
                        <WhenPermitted rule="canChangeProcessingUser">
                            <ModusButton
                                icon="delete"
                                onClick={this.handleDeleteProxy}
                                content="Delete"
                            />
                        </WhenPermitted>
                    </div>
                </div>
            </Popup>
        )
    }
}
