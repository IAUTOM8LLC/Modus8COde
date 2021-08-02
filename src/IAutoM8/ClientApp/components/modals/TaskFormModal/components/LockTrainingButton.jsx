import axios from "axios";
import React from "react";
import { Field } from 'redux-form';
import { Form } from 'semantic-ui-react';

import { getAuthHeaders } from "@infrastructure/auth";

import { ModusButton } from "@components";

class LockTrainingButton extends React.Component {
    constructor(props) {
        super(props);

        this.renderIcon = this.renderIcon.bind(this);
    }

    handleLockStatusChange = async (lockStatus) => {
        if (this.props.taskId) {
            if (lockStatus) {
                await axios.put(
                    "/api/formulaTask/lock-training",
                    JSON.stringify(this.props.taskId),
                    getAuthHeaders()
                );
            } else {
                await axios.put(
                    "/api/formulaTask/unlock-training",
                    JSON.stringify(this.props.taskId),
                    getAuthHeaders()
                );
            }
        }
    };

    renderIcon(field) {
        const {
            input: { onChange },
            isTrainingLocked,
        } = field;

        return (
            <Form.Field>
                <ModusButton
                    circular
                    popup={
                        isTrainingLocked ? "Unlock Training" : "Lock Training"
                    }
                    icon={isTrainingLocked ? "iauto--unlock" : "iauto--lock"}
                    onClick={(e) => {
                        e.preventDefault();
                        onChange(!isTrainingLocked);
                        this.handleLockStatusChange(!isTrainingLocked);
                    }}
                />
            </Form.Field>
        );
    }

    render() {
        return (
            <div style={{ float: "right" }}>
                <Field
                    {...this.props}
                    type="checkbox"
                    component={this.renderIcon}
                />
            </div>
        );
    }
}

export default LockTrainingButton;
