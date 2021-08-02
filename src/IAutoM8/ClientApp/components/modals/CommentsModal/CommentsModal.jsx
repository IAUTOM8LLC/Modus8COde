import React from 'react';
import { Modal } from 'semantic-ui-react';
import differenceBy from "lodash/differenceBy";

import { ModusButton } from '@components';

import CommentPane from '../TaskFormModal/components/CommentPane';

class CommentsModal extends React.Component {
    
    componentDidUpdate(prevProps) {
        const diff = differenceBy(this.props.taskId, prevProps.taskId);
        if (diff.length) {
            this.renderComments();
        }
    }

    renderComments() {
        return <CommentPane taskId={this.props.taskId} />;
    }

    render() {
        return (
            <Modal
                open={this.props.open}
                className="task-details-modal tab-modal"
                onClose={this.props.onClose}
            >
                <Modal.Header>Task Details</Modal.Header>
                <Modal.Content>
                    <Modal.Description>{this.renderComments()}</Modal.Description>
                </Modal.Content>

                <Modal.Actions>
                    <ModusButton
                        grey
                        className="button-flex-order2"
                        type="button" 
                        content="Close" 
                        onClick={this.props.onClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}

export default CommentsModal;