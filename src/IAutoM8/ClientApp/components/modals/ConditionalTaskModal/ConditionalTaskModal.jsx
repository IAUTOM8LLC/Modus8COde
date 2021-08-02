import React, { Component } from 'react'
import { Modal, Header, Checkbox, Form, Label } from 'semantic-ui-react'
import autobind from 'autobind-decorator'

import { ModusButton, Prompt } from '@components'

const initalState = {
    open: false,
    resolve: () => { },
    reject: () => { },

    condition: '',
    options: [],
    selectedOptionId: 0
}

export default class ConditionalTaskModal extends Component {
    state = initalState

    show(conditions) {
        const { condition, options } = conditions;
        const { id: selectedOptionId = 0 } = options.find(opt => opt.isSelected) || {};

        return new Promise((resolve, reject) => {
            this.setState({
                open: true,
                resolve,
                reject,

                options: options,
                condition: condition,
                selectedOptionId: selectedOptionId
            })
        })
    }

    handleClose = () => {
        this.setState(initalState);
        this.state.reject();
    }

    @autobind
    async handleApprove() {
        this.state.resolve(this.state.selectedOptionId);
        this.setState(initalState);
    }

    handleChange = (e, { value }) => {
        this.setState({ selectedOptionId: value })
    }

    render() {
        const {
            open,
            options,
            condition,
            selectedOptionId
        } = this.state;

        return (
            <Modal
                open={open}
                size="tiny"
                onClose={this.handleClose}
            >
                <Modal.Header>{condition}</Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        <Header as="h4">Select condition option</Header>
                        {
                            options.map(opt => (
                                <Form.Group key={opt.id}>
                                    <Form.Field >
                                        <Checkbox
                                            radio
                                            name="conditionOptionsRadioGroup"
                                            value={opt.id}
                                            label={opt.option}
                                            onChange={this.handleChange}
                                            checked={selectedOptionId === opt.id}
                                        />
                                    </Form.Field>
                                </Form.Group>
                            ))
                        }
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled
                        content="Select"
                        onClick={this.handleApprove}
                    />
                    <ModusButton
                        className="button-flex-order2"
                        grey type="button" content="Cancel" onClick={this.handleClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}
