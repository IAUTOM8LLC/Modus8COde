import React, { Component } from 'react'
import { Modal } from 'semantic-ui-react'

import { ModusButton, Inline } from '@components'

import './QuickStartProjectModal.less'

export default class QuickStartProjectModal extends Component {
    render() {
        const {
            open,
            loading,
            onBack,
            onClose,
            onSkip,
            onSubmit
        } = this.props;

        return (
            <Modal
                as="form"
                open={open}
                className="tab-modal quick-start-project-modal"
                size="small"
                onClose={onClose}
            >
                <Modal.Header>
                    <Inline>
                        <ModusButton
                            grey
                            circular
                            icon="long arrow left"
                            onClick={onBack}
                            popup="Back"
                        />
                        <h3>Select start dates for tasks</h3>
                    </Inline>
                </Modal.Header>

                <Modal.Content>
                    <ModusButton
                        type="button"
                        wide
                        filled
                        className="quick-start-project-modal__start"
                        content="START TASKS NOW"
                        onClick={onSubmit}
                        loading={loading}
                    />
                    <div className="quick-start-project-modal__or" >or</div>
                    <a className="quick-start-project-modal__skip" onClick={onSkip}>select later date</a>
                </Modal.Content>

                <Modal.Actions>
                    <ModusButton grey type="button" content="Cancel" onClick={onClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}
