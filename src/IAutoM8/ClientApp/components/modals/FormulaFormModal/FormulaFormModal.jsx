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
    form: "formulaFormModal",
    validate,
    enableReinitialize: true,
})
export default class FormulaFormModal extends Component {
    static tabMenuOptions = {
        fluid: true,
        vertical: true,
        tabular: "left",
    };

    render() {
        const {
            initialValues,
            open,
            handleSubmit,
            submitting,
            onClose,
            userPermissions,
            categories,
            loggedInUserRole,
            showCopyFormula
        } = this.props;
        
        const panes = [
            userPermissions.editFormulaInfo && {
                menuItem: { key: "information", content: "Information" },
                pane: {
                    key: "tab1",
                    content: <InformationPane categories={categories} showCategory={showCopyFormula} />,
                },
            },
            userPermissions.shareFormula &&
            initialValues.id &&
            loggedInUserRole !== "Admin" && !showCopyFormula && {
                menuItem: { key: "share", content: "Share" },
                pane: {
                    key: "tab4",
                    content: <SharePane formula={initialValues} />,
                },
            },
        ].filter((pane) => pane);

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="formula-details-modal tab-modal"
                onClose={onClose}
                
            >
                <Modal.Header>Formula details</Modal.Header>

                <Tab
                    panes={panes}
                    renderActiveOnly={false}
                    menu={{ secondary: true, pointing: true }}
                />

{!this.props.disabled ?(<Modal.Actions className="modal-flex-actions">
                {showCopyFormula  && <ModusButton

                        filled
                        content= "Save"
                        type={
                            initialValues.isView || initialValues.isLocked
                                ? "button"
                                : "submit"
                        }                        
                        className="button-flex-order1"
                        loading={submitting}
                        popup={
                            initialValues.isView || initialValues.isLocked
                                ? "You cannot edit locked formula."
                                : ""
                        }
                        style={
                            initialValues.isView || initialValues.isLocked
                                ? { cursor: "no-drop" }
                                : {}
                        }
                    />}
                    {!showCopyFormula && <ModusButton
                        disabled={this.props.disabled}
                        filled
                        content= "Save"
                        type={
                            initialValues.isView || initialValues.isLocked
                                ? "button"
                                : "submit"
                        }
                        className="button-flex-order1"
                        loading={submitting}
                        popup={
                            initialValues.isView || initialValues.isLocked
                                ? "You cannot edit locked formula."
                                : ""
                        }
                        style={
                            initialValues.isView || initialValues.isLocked
                                ? { cursor: "no-drop"}
                                : {}
                        }
                    />}

                    <ModusButton
                        className="button-flex-order2"
                        grey
                        type="button"
                        content="Cancel"
                        onClick={onClose}
                    />
                </Modal.Actions>):(<Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        //style={styles}

                        filled
                        content= "Save" 
                        type={ "button"}                       
                        className="button-flex-order1"
                        loading={submitting}
                        popup={
                            initialValues.isView || initialValues.isLocked
                                ? "You cannot edit locked formula."
                                : ""
                        }
                        style={{background: "#988f9f" ,pointerEvents: "none" } }
                        
                    />

                    <ModusButton
                        className="button-flex-order2"
                        grey
                        type="button"
                        content="Cancel"
                        onClick={onClose}
                    />
                </Modal.Actions>)}
            </Modal>
        );
    }
}
