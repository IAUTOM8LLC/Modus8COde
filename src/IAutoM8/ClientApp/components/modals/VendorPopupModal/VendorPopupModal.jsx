/* eslint-disable max-len */
import React, { Component } from "react";

import {
    reduxForm,
    formValueSelector
} from "redux-form";

import { connect } from "react-redux";

import {
    Modal,
    Tab,
    Icon,
    Divider,
    Loader,
    Dimmer,
    Form,
    Input,
    Label,
} from "semantic-ui-react";

import { loadVendorTax } from "@store/credits";

import { FilesPane, LinksPane, ModusButton } from "@components";

import RecurrencePane from "./components/RecurrencePane";

import editModalHoc from "../../common/editModalHoc";

import InformationPane from "./components/InformationPane";

import { TextInput } from "@components";

import "./VendorPopupModal.less";

const vendorPopupModalSelector = formValueSelector(
    "vendorNotificationFormModal"
);

@editModalHoc
@reduxForm({
    form: "vendorNotificationFormModal",
    enableReinitialize: true,
})
@connect(
    (state) => ({
        vendorTax: state.credits.vendorTax,
        amount: Number(vendorPopupModalSelector(state, "price")),
        roles: state.auth.user.roles,
    }),
    {
        loadVendorTax,
    }
)
export default class VendorPopupModal extends Component {
    componentDidMount() {
        this.props.loadVendorTax();
    }

    renderTab() {
        const {
            initialValues: {
                isConditional,
                isAutomated,
                recurrenceType,
                recurrenceOptions,
                isRecurrent,
            },
        } = this.props;

        let panes = [];

        panes = [
            {
                menuItem: { key: "information", content: "Information" },
                pane: { key: "information", content: <InformationPane /> },
            },
            {
                menuItem: { key: "files", content: "Files" },
                pane: { key: "files", content: <FilesPane isVendor /> },
            },
            {
                menuItem: { key: "links", content: "Links" },
                pane: { key: "links", content: <LinksPane isVendor /> },
            },
            isRecurrent && {
                menuItem: { key: "recurrence", content: "Recurrence" },
                pane: {
                    key: "recurrence",
                    content: (
                        <RecurrencePane
                            isAutomated={isAutomated}
                            isConditional={isConditional}
                            recurrenceType={recurrenceType}
                            isFormulaTask={false}
                            cronTab={recurrenceOptions.cronTab}
                            canEdit={false}
                        />
                    ),
                },
            },
        ].filter((pane) => pane);

        return (
            <Tab
                panes={panes}
                renderActiveOnly={false}
                menu={{ secondary: true, pointing: true }}
            />
        );
    }

    renderField = ({ input, type, name, value, label }) => {
        const elementProps = {
            type,
            name,
            value,
            label,
            ...input,
        };
        return <Input {...elementProps} />;
    };

    calculateAmountWithTax = () => {
        const { amount, vendorTax } = this.props;
        if (amount && vendorTax) {
            return (
                amount -
                vendorTax.fee -
                (vendorTax.percentage / 100) * amount
            ).toFixed(2);
        }
        return 0;
    };

    render() {
        const {
            open,
            loading,
            onClose,
            handleSubmit,
            initialValues,
            onAccept,
            onDecline,
            vendorTax,
            submitting,
            roles
        } = this.props;

        const role =  roles.toString()
        const isProjectTaskNotification =
            initialValues !== undefined
                ? initialValues.isProjectTaskNotification
                : false;

        const isRecurrent =
            initialValues !== undefined && initialValues.isRecurrent;

        return (
            <React.Fragment>
                {initialValues && (
                    <Modal
                        as="form"
                        open={open}
                        onSubmit={handleSubmit}
                        className="task-details-modal tab-modal"
                        onClose={onClose}
                    >
                        <div className="grid-container">
                            <div className="task-information">
                                <Dimmer inverted active={loading} size="large">
                                    <Loader>Loading</Loader>
                                </Dimmer>

                                <Modal.Header className="vendor-popup-header">
                                    Task details
                                </Modal.Header>

                                {this.renderTab()}

                                {isRecurrent && (
                                    <div className="recurrent-warning">
                                        <Icon className="recurrent-info-icon" />
                                        <div className="credits-warning">
                                            Warnings
                                        </div>
                                        <Divider />
                                        <Label
                                            style={{ marginTop: "5px" }}
                                            content="This task is recurring task"
                                        />
                                        <br />
                                        <Label
                                            style={{ marginTop: "5px" }}
                                            content="For more information go to recurrence tab"
                                        />
                                        <br />
                                        <Label
                                            style={{
                                                marginTop: "5px",
                                                marginBottom: "10px",
                                            }}
                                            content="Please place your bid for EACH recurrence"
                                        />
                                    </div>
                                )}
                            </div>
                            <div className="notification-actions">
                                <Modal.Header className="vendor-popup-header">
                                    You are the one!
                                </Modal.Header>

                                <Form as="section">
                                    <TextInput
                                        fieldClassName="input-vendor-duration"
                                        readOnly
                                        name="duration"
                                        label="Duration"
                                        placeholder="HH:MM"
                                    />
                                    {role !== 'Company'&&(<TextInput
                                        fluid
                                        fieldClassName="input-vendor-price"
                                        readOnly={isProjectTaskNotification}
                                        name="price"
                                        label="Price"
                                        placeholder="$"
                                    />)}
                                    {role === 'Company'&& (
                                        <div>
                                    <TextInput
                                        fluid
                                        fieldClassName="input-vendor-price"
                                        readOnly={isProjectTaskNotification}
                                        name="price"
                                        label="Company Price"
                                        placeholder="$"
                                    /> 
                                    <div style = {{marginLeft: "31px",
                                            marginRight: "37px"}}>
                                    <TextInput
                                        
                                        fieldClassName="input-CompanyWorker-price"
                                        readOnly={isProjectTaskNotification}
                                        name="companyWorkerPrice"
                                        label="CompanyWorker Price"
                                        placeholder="$"  
                                    /></div>  
                                    </div>
                                    )}
                                    <Label
                                        className="label-tax-price"
                                        content={`Tax is ${
                                            !vendorTax ? 0 : vendorTax.fee
                                        }$
+ ${!vendorTax ? 0 : vendorTax.percentage}%.
You will get ${this.calculateAmountWithTax()}$`}
                                    />
                                </Form>

                                <Modal.Actions className="buttons-group">
                                {this.props.amount === null || this.props.amount === 0 && (<ModusButton
                                        className="button-flex-order1 button-submit-vendor"
                                        filled
                                        style={{ cursor: "not-allowed", background: 
                                            this.props.amount === null || this.props.amount === 0 ? "#988f9f" : "#5d2684" }}
                                        content={
                                            isProjectTaskNotification
                                                ? "Accept"
                                                : "Let's do it!"
                                                //vendorTax.fee === 0 ? <div style = {{background:"#988f9f"}}>"Let's do it!"</div>
                                                //:<div style = {{background:"#5d2684"}}>"Let's do it!"</div>
                                        }
                                        type="button"
                                        order="1"
                                        disabled={submitting}
                                        //onClick={this.props.amount === null  || this.props.amount === 0 ? console.log('gsgsggsgsg'): onAccept}
                                    />)}
                                    {this.props.amount === !null || this.props.amount > 0 && (<ModusButton
                                        className="button-flex-order1 button-submit-vendor"
                                        filled
                                        content={
                                            isProjectTaskNotification
                                                ? "Accept"
                                                : "Let's do it!"
                                        }
                                        type="submit"
                                        order="1"
                                        disabled={submitting}
                                        onClick={onAccept}
                                    />)}
                                    <ModusButton
                                        className="button-flex-order2 button-submit-vendor decline-button-color"
                                        filled
                                        content={
                                            isProjectTaskNotification
                                                ? "Decline"
                                                : "Decline task"
                                        }
                                        type="submit"
                                        order="2"
                                        disabled={submitting}
                                        onClick={onDecline}
                                    />
                                    <ModusButton
                                        className="button-flex-order3 skip-button-color"
                                        content="Skip"
                                        order="3"
                                        onClick={onClose}
                                    />
                                </Modal.Actions>
                            </div>
                        </div>
                    </Modal>
                )}
            </React.Fragment>
        );
    }
}
