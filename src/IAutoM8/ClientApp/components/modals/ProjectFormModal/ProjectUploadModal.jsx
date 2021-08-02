import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reduxForm } from 'redux-form'
import { Modal, Tab } from 'semantic-ui-react'

import { FilesPane, LinksPane, ModusButton } from '@components'

import { required } from '@utils/validators'

import InformationPane from './components/InformationPane'

import './ProjectFormModal.less'

// TODO: field validtion not fires after form submit or reset
// seems like bug in redux-form
const validate = (values) => ({
    name: required(values.name)
})

@connect(state => ({ userPermissions: state.auth.permissions }))
@reduxForm({
    form: 'projectUploadModal',
    validate,
    enableReinitialize: true
})
export default class ProjectUploadModal extends Component {
    render() {
        const {
            open,
            users,
            onClose,
            handleSubmit,
            submitting,
            userPermissions,
            clients,
            isBulkModelForm,
        } = this.props;
        console.log('modal',isBulkModelForm);

        if (!open)
            return null;

        const clientOptions = clients.map(c => ({
            key: c.id,
            value: c.id,
            text: c.companyName
        }));
        clientOptions.splice(0, 0, { key: 'empty', value: null, text: '' });

        const panes = [
            userPermissions.projectModalWindow &&
            {
                menuItem: { key: 'information', content: 'Information' },
                pane: {
                    key: 'information',
                    content:
                    <InformationPane
                        users={users}
                        clients={clientOptions}
                    />
                }
            }
        ].filter(t => t);

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="project-details-modal tab-modal"
                onClose={onClose}
            >
                <Modal.Header>Bulk Project Upload</Modal.Header>

                <Tab
                    //panes={panes}
                    renderActiveOnly={false}
                    menu={{ secondary: true, pointing: true }}
                />
                <FilesPane isBulkModelForm ={isBulkModelForm}/>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled content="Save" type="submit" loading={submitting} />
                    <ModusButton
                        className="button-flex-order2"
                        grey type="button" content="Cancel" onClick={onClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}
