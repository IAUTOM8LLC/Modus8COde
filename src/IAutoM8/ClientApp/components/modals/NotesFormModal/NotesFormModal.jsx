import React, { Component } from "react";
import { connect } from "react-redux";
import { reduxForm } from "redux-form";
import { Modal, Tab } from "semantic-ui-react";

import { FilesPane, LinksPane, ModusButton } from "@components";

import { required } from "@utils/validators";

import SharePane from "./components/SharePane";
import InformationPane from "./components/InformationPane";

import "./FormulaFormModal.less";

// TODO: field validtion not fires after form submit or reset
// seems like bug in redux-form
const validate = (values) => ({
    name: required(values.name),
});

@connect((state) => ({ userPermissions: state.auth.permissions }))
@reduxForm({
    form: "NotesFormModal",
    validate,
    enableReinitialize: true,
})
export default class NotesFormModal extends Component {
    static tabMenuOptions = {
        fluid: true,
        vertical: true,
        tabular: "left",
    };

    render() {
        const {
            initialValues,
            open,
            handleNotesSubmit,
            handleSubmit,
            submitting,
            onClose,
            userPermissions,
            categories,
            loggedInUserRole,
            showCopyFormula,
            id,
            projectToEdit
        } = this.props;
        const projectName = initialValues.name
        //console.log('projectName',projectName);
        
        const panes = [
            //userPermissions.editFormulaInfo &&
        {
                menuItem: { key: "Project name",  content: (projectName) },
                pane: {
                    key: "tab1",
                    content: <InformationPane  projectToEdit ={projectToEdit}/>,
                },
            },
            // userPermissions.shareFormula &&
            // initialValues.id &&
            // loggedInUserRole !== "Admin" && !showCopyFormula && {
            //     menuItem: { key: "share", content: "Share" },
            //     pane: {
            //         key: "tab4",
            //         content: <SharePane formula={initialValues} />,
            //     },
            // },
        ].filter((pane) => pane);
        
        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="formula-details-modal tab-modal"
                onClose={onClose}
            >
                <Modal.Header style ={{
                    position: "realtive",
                    padding: "15px",
                    margin :"20px"
                }}>Notes details
                <i
                        className="close icon"
                        style={{ margin: "0 0 0 82%", 
                        cursor: "pointer" ,
                        position: "absolute",
                        top: "30px",
                        right: "45px"
                    }}
                        onClick={onClose}
                    ></i>
                </Modal.Header>
                {/* <Modal.Header>
                    {initialValues.isInterval
                        ? "Interval details"
                        : "Task details"}
                    <i
                        className="close icon"
                        style={{ margin: "0 0 0 82%", cursor: "pointer" }}
                        onClick={onClose}
                    ></i>
                </Modal.Header> */}

                <Tab
                    panes={panes}
                    renderActiveOnly={false}
                    menu={{ secondary: true, pointing: true }}
                />

                <Modal.Actions className="modal-flex-actions">
                <ModusButton
                        className="button-flex-order1"
                        filled content="Save" type="submit" loading={submitting} />
                    
                </Modal.Actions>
            </Modal>
        );
    }
}
