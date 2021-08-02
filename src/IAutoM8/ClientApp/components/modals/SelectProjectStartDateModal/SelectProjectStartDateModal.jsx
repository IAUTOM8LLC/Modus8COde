import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reduxForm, formValueSelector} from 'redux-form'
import { Form, Modal, Tab } from 'semantic-ui-react'
import moment from 'moment'

import { loadTasks as loadFormulaTasks } from '@store/formulaTasks'
import { required, onlyFutureDate } from '@utils/validators'
import { selectRootTasks } from '@selectors/formulaTasks'

import { ModusButton } from '@components'

import GeneralPane from './components/SingleStartDatePane'
import ChainsPane from './components/ChainsStartDatesPane'

const validateRoots = (roots, projectStartDateTime) => {
    if (roots.length === 0) return 'undefined';
    return roots.map(t => required(projectStartDateTime) || onlyFutureDate(t.value))
        .filter(t => t);
}

const validate = ({ projectStartDateTime, rootStartDateTime }) => ({
    projectStartDateTime: required(projectStartDateTime) || onlyFutureDate(projectStartDateTime),
    rootStartDateTime: validateRoots(rootStartDateTime, projectStartDateTime)
})

@connect((state, props) => {
    const roots = selectRootTasks(state, props);
    let value = formValueSelector('selectProjectStartDateForm')(state, 'projectStartDateTime');
    if (!value) {
        value = moment().add(5, 'm').format('MMM D, YYYY h:mm A');
    }
    return {
        rootTasks: roots,
        initialValues: {
            rootStartDateTime: roots
                .map(t => (
                    {
                        key: t.id,
                        idDashed: t.idDashed,
                        value: value
                    })),
            projectStartDateTime: value
        }
    }
},
    { loadFormulaTasks }
)
@reduxForm({
    form: 'selectProjectStartDateForm',
    enableReinitialize: true,
    validate
})
export default class SelectProjectStartDateModal extends Component {
    state = { activeIndex: 0 }

    componentWillReceiveProps(nextProps) {
        if (this.props.formulaId !== nextProps.formulaId && nextProps.formulaId > 0) {
            this.props.loadFormulaTasks(nextProps.formulaId);
        }
    }

    handleTabChange = (e, { activeIndex }) => {
        this.setState({
            activeIndex: activeIndex
        });
    }
    render() {
        const {
            open,
            onClose,
            onBack,
            handleSubmit,
            loading,
            modalHeader,
            rootTasks = [],
        } = this.props;

        const panes = [
            {
                menuItem: { key: 'generalDate', content: 'General date' },
                render: () => <div className="content"><GeneralPane /></div>
            },
            {
                menuItem: { key: 'rootChains', content: 'Chains\' root task' },
                render: () => <div className="content"><ChainsPane rootTasks={rootTasks} /></div>
            }
        ];

        const menuOptions = { secondary: true, pointing: true };

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="select-project-start-date-modal tab-modal"
                size="small"
                onClose={onClose}
            >
                <Modal.Header>{modalHeader}</Modal.Header>

                <Form as="section">
                    <Tab
                        panes={panes}
                        renderActiveOnly
                        activeIndex={this.state.activeIndex}
                        onTabChange={this.handleTabChange}
                        menu={menuOptions}
                    />
                </Form>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled content="Import" type="submit" loading={loading} />
                    <ModusButton
                        className="button-flex-order2"
                        grey type="button" content="Cancel" onClick={onClose} />
                    {onBack &&
                        <ModusButton className="button-flex-order3"
                            grey content="Back" floated="left" onClick={onBack} />
                    }
                </Modal.Actions>
            </Modal>
        );
    }
}
