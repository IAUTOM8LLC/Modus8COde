import React, { Component } from 'react'
import { Modal, Header, Checkbox, Form, Label, Rating } from 'semantic-ui-react'
import autobind from 'autobind-decorator'

import { ModusButton, Prompt } from '@components'
import './RatingModal.less'

const initalState = {
    open: false,
    resolve: () => { },
    reject: () => { },

    rating: 0
}

export default class RatingModal extends Component {
    state = initalState

    show() {

        return new Promise((resolve, reject) => {
            this.setState({
                open: true,
                resolve,
                reject
            })
        })
    }

    handleClose = () => {
        this.setState(initalState);
        this.state.reject();
    }

    @autobind
    async handleApprove() {
        this.state.resolve(this.state.rating);
        this.setState(initalState);
    }

    handleRate = (e, { rating }) => this.setState({
        ...this.state,
        rating
    })

    render() {
        const {
            open,
        } = this.state;

        return (
            <Modal
                open={open}
                size="tiny"
                onClose={this.handleClose}
            >
                <Modal.Header>Rate your experience</Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        <Header as="h5" className="vendor-subheader">Select from 1 to 5</Header>
                        <Form.Group>
                            <Form.Field >
                                <Rating
                                    clearable
                                    maxRating="5"
                                    size="huge"
                                    onRate={this.handleRate}
                                    className="vendor-rating"
                                />
                            </Form.Field>
                        </Form.Group>
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled
                        content="Submit"
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
