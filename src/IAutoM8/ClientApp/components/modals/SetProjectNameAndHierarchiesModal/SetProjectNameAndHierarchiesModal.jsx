import React from 'react';
import { connect } from 'react-redux';
import { reduxForm } from 'redux-form';
import { Form, Header, Modal, Segment } from 'semantic-ui-react';

import { 
    loadProjects,
    loadChildProjects
} from '@store/project';

import { 
    getProjectOptions,
    getChildProjectOptions
} from '@selectors/project';

import { required } from '@utils/validators'

import {
    DropdownInput,
    Inline,
    ModusButton,
    RadioInput,
    TextInput 
} from '@components';

import './SetProjectNameAndHierarchiesModal.less';

const validate = ({ viewExistingProjects, parentProjectId }) => {
    const errors = {};
    if (viewExistingProjects) {
        if (!parentProjectId) {
            errors.parentProjectId = 'Please select a project';
        }
    }
    return errors;
}

class SetProjectNameAndHierarchiesModal extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            selectedRadioValue: "newProjectRadio",
            // showParentProjects: false,
            showChildProjects: false,
        }
    }

    handleRadioChange = (e, value) => {
        this.props.reset("setProjectNameAndHierarchiesForm");

        if (value === "existingProjectRadio") {
            this.props.loadProjects();
        }

        if (value === "newProjectRadio") {
            this.setState({
                showChildProjects: false,
            });
        }

        this.setState({ selectedRadioValue: value });
    };

    handleParentProjectChange = (event, parentProjectId) => {
        this.props.loadChildProjects(parentProjectId)
            .then(() => { this.setState({ showChildProjects: true }) });
    }

    render() {
        const {
            open,
            onBack,
            onClose,
            handleSubmit,
            loading,
            modalHeader,
            formulaName,
            projectOptions,
            childProjectOptions,
        } = this.props;

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="task-details-modal"
                size="small"
                onClose={onClose}
            >
                <Modal.Header>{modalHeader}</Modal.Header>

                <Modal.Content>
                    <Header as="h4">
                        <div className="create-project">
                            <i aria-hidden="true" className="icon formulas" />
                            {formulaName}
                        </div>
                    </Header>

                    <Form as="section" style={{ paddingTop: "15px", paddingBottom: "20px" }}>
                        <Inline style={{ paddingBottom: "25px" }}>
                            <RadioInput
                                style={{ paddingLeft: "15px", fontWeight: "bold" }}
                                label="New Project"
                                value="newProjectRadio"
                                name="radioGroup"
                                checked={
                                    this.state.selectedRadioValue ===
                                    "newProjectRadio"
                                }
                                onChange={this.handleRadioChange}
                            />

                            <RadioInput
                                style={{ fontWeight: "bold" }}
                                label="Add To Existing Project"
                                value="existingProjectRadio"
                                name="radioGroup"
                                checked={
                                    this.state.selectedRadioValue ===
                                    "existingProjectRadio"
                                }
                                onChange={this.handleRadioChange}
                            />
                        </Inline>

                        {this.state.selectedRadioValue ===
                            "newProjectRadio" && (
                            <TextInput
                                fluid
                                required
                                name="projectName"
                                label="New Project"
                            />
                        )}

                        {this.state.selectedRadioValue ===
                            "existingProjectRadio" && (
                            <React.Fragment>
                                <DropdownInput
                                    search
                                    fluid
                                    selection
                                    name="parentProjectId"
                                    label="Select parent project"
                                    options={projectOptions}
                                    onChange={this.handleParentProjectChange}
                                    validate={[required]}
                                />

                                {this.state.showChildProjects &&
                                    childProjectOptions.length > 0 && (
                                        <DropdownInput
                                            search
                                            fluid
                                            selection
                                            name="childProjectId"
                                            label="Select child project"
                                            // options={this.state.childProjectOptions}
                                            options={childProjectOptions}
                                        />
                                    )}
                            </React.Fragment>
                        )}
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled
                        content="NEXT"
                        type="submit"
                        loading={loading}
                    />
                    <ModusButton
                        className="button-flex-order2"
                        grey
                        type="button"
                        content="Cancel"
                        onClick={onClose}
                    />
                    {onBack && (
                        <ModusButton
                            className="button-flex-order3"
                            grey
                            content="Back"
                            floated="left"
                            onClick={onBack}
                        />
                    )}
                </Modal.Actions>
            </Modal>
        );
    }
}

export default connect(
    (state) => ({
        projectOptions: getProjectOptions(state),
        childProjectOptions: getChildProjectOptions(state)
    }), 
    {
        loadProjects,
        loadChildProjects
    } 
)(reduxForm({
    form: 'setProjectNameAndHierarchiesForm',
    enableReinitialize: true,
    validate,
})(SetProjectNameAndHierarchiesModal));
