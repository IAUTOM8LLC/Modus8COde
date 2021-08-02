import React, { Component } from 'react'
import ReactDOM from 'react-dom'
import { Button, Modal, Icon, Header } from 'semantic-ui-react'

import createDefferedPromise from '@utils/createDefferedPromise'

class Prompt extends Component {

    close(result) {
        this.props.deferred.resolve(result);
        this.props.cleanup();
    }

    renderPrompt() {
        const { icon, header, message } = this.props;

        return (
            <Modal
                basic
                open
                size="tiny"
            >
                <Header icon={icon} content={header} />

                <Modal.Content>
                    {message}
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <Button
                        className="button-flex-order1"
                        positive color="green" inverted onClick={() => this.close(true)}>
                        <Icon name="checkmark" /> Yes
                    </Button>
                    <Button
                        className="button-flex-order2"
                        color="red" inverted onClick={() => this.close(false)}>
                        <Icon name="remove" /> No
                    </Button>
                </Modal.Actions>
            </Modal>
        )
    }

    render() {
        return ReactDOM.createPortal(
            this.renderPrompt(),
            this.props.container,
        );
    }
}



Prompt.confirm = async function (header = '', message = '', icon = '') {
    const container = document.createElement('div');
    document.body.appendChild(container);
    const deferred = createDefferedPromise();

    const cleanup = () => {
        ReactDOM.unmountComponentAtNode(container);
        document.body.removeChild(container);
    }

    const props = { header, message, icon, container, deferred, cleanup };
    ReactDOM.render(<Prompt {...props} />, container);

    const result = await deferred;
    return result
}

export default Prompt;
