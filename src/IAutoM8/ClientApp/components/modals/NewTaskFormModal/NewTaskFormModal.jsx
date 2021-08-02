import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reduxForm } from 'redux-form'
import { Modal, Tab, Icon } from 'semantic-ui-react'

import { FilesPane, LinksPane, ModusButton } from '@components'

import { required } from '@utils/validators'

import InformationPane from './components/InformationPane'

import './NewTaskFormModal.less'

// TODO: field validtion not fires after form submit or reset
// seems like bug in redux-form
const validate = (values) => ({
    name: required(values.name)
})
//console.log('state.vendor.newTaskData[0].companyPrice ',state.vendor.newTaskData[0].companyPrice );
@connect(state => {
    const initialValues = {
        companyPrice: state.vendor.newTaskData[0].companyPrice,
        //gigPayout: state.vendor.newTaskData[0].companyWorkerPrice
    }
    state.vendor.newTaskData.forEach(item => {
        initialValues[item.companyWorkerId] = item.companyWorkerPrice
    })
    return ({ userPermissions: state.auth.permissions,
    
    initialValues: initialValues})})
    
@reduxForm({
    form: 'newTaskFormModal',
    validate,
    enableReinitialize: true
})
export default class NewTaskFormModal extends Component {
    render() {
        const {
            initialValues,
            open,
            // users,
            onClose,
            handleSubmit,
            submitting,
            // clients
            newTaskModalData,
            toggleAddButtonModal
        } = this.props;

        if (!open)
            return null;

        // const clientOptions = clients.map(c => ({
        //     key: c.id,
        //     value: c.id,
        //     text: c.companyName
        // }));
        // clientOptions.splice(0, 0, { key: 'empty', value: null, text: '' });

        const panes = [
            {
                menuItem: { key: 'formulaName', content: ` Formula -${newTaskModalData[0].formula}`  },
                pane: {
                    key: 'formulaName',
                    content:
                        newTaskModalData !== undefined && newTaskModalData && (<InformationPane
                            // users={users}
                            // clients={clientOptions}
                            modalItems={newTaskModalData}
                        />)
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
                <Modal.Header>
                    <div className="iauto-vendor-task-header">
                        <div>Task-{newTaskModalData[0].task}</div>
                        <Icon name="cancel" onClick={onClose} />
                    </div>


                </Modal.Header>

                <Tab
                    panes={panes}
                    renderActiveOnly={false}
                    menu={{ secondary: true, pointing: true }}
                />

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled content="Save" type="submit" loading={submitting} />
                    {toggleAddButtonModal ? <ModusButton
                        className="button-flex-order2"
                        grey type="button"
                        content="+Add Users"
                        onClick={() => toggleAddButtonModal(true)} />
                        : null}
                </Modal.Actions>
            </Modal>
        );
    }
}