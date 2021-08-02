/* eslint-disable max-len */
import React from "react";
import { bindActionCreators } from 'redux';
import { connect } from "react-redux";
import { success, error, info } from 'react-notification-system-redux';
import { 
    formValueSelector,
    reduxForm,
    FieldArray,
    change as changeFieldValue,
} from "redux-form";
import {
    Form,
    Icon,
    List,
    Message,
    Modal,
    Segment,
    Header,
} from "semantic-ui-react";

import { 
    loadFormulaMeanTat
} from "@store/formula"; 
import { loadCredits } from "@store/credits";
import { 
    loadVendorsByOptionType,
} from '@store/vendor';

import { formatMoney } from "@utils/formatMoney";
import { getErrorFieldNames } from "@utils/assignSkills/getErrorFieldNames";

import { ModusButton } from "@components";

import AssignSkillUsers from "./components/AssignSkillUsers";

const validate = ({ formulaTaskMaps = [] }) => {
    const errors = { formulaTaskMaps: [] };
    formulaTaskMaps.forEach((x, index) => {
        if (!x.isDisabled) {
            if (x.userIds.length === 0 && x.outsourceUserIds.length === 0) {
                errors.formulaTaskMaps[index] = { 
                    userIds: "Atleast one In-House or Outsourced user is required",
                    outsourceUserIds: "Atleast one In-House or Outsourced user is required"
                };
            }
            if (x.outsourceUserIds.length !== 0 && x.reviewingUserIds.length === 0) {
                errors.formulaTaskMaps[index] = { 
                    reviewingUserIds: "Reviewer is mandatory to select for the outsourced task"
                };
            }
        }
    });
    
    return errors;
};

const scrollToFirstError = (errors) => {
    const errorFields = getErrorFieldNames(errors);

    // Using breakable for loop
    for (let i = 0; i < errorFields.length; i++) {
        const fieldName = `${errorFields[i]}`;
        // Checking if the marker exists in DOM
        if (document.querySelectorAll(`[name="${fieldName}"]`).length) {
            document
                .querySelector(`[name="${fieldName}"]`)
                .scrollIntoView({ behavior: "smooth" });

            break;
        }
    }
};

const assignSkillsValueSelector = formValueSelector("assignSkillsForm");

class AssignSkillsModal extends React.Component {
    state = {
        total: 0,
        priceRange: '',
        maxRangePrice:0,
    };

    componentDidMount() {
        this.props.loadCredits();
        this.props.loadFormulaMeanTat(this.props.formulaId);
    }
    
    componentDidUpdate(prevProps) {
        if (this.props.formulaTaskMaps !== prevProps.formulaTaskMaps) {
            let total = 0;
            let highestPrice = 0;
            let lowestPrice = 0;
            const { formulaTaskMaps } = this.props;

            formulaTaskMaps.forEach(formulaTask => {
                let currentTaskHighestPrice = 0;
                let currentTaskLowestPrice = Number.POSITIVE_INFINITY;

                formulaTask.outsourceUserIds.forEach(id => {
                    const outsourcer = formulaTask.certifiedVendors
                        .find(element => element.id === id);
                    const currentPrice = outsourcer ? outsourcer.price : 0;

                    currentTaskHighestPrice = Math.max(currentTaskHighestPrice, currentPrice);
                    currentTaskLowestPrice = Math.min(currentTaskLowestPrice, currentPrice);
                });

                highestPrice += currentTaskHighestPrice;
                //console.log('highestPrice',highestPrice);
                this.setState({ 
                    maxRangePrice :highestPrice 
                });
                //console.log('maxRangePrice',this.state.maxRangePrice);

                // lowestPrice += currentTaskLowestPrice;
                lowestPrice += (Number.isFinite(currentTaskLowestPrice) ? currentTaskLowestPrice : 0);
            });

            //const lowerPrice = (Number.isFinite(lowestPrice) ? lowestPrice : 0);
            total = highestPrice + lowestPrice;
            
            //console.log('total', total);
            const priceRange = `$${formatMoney(lowestPrice, 2, ".", " ")} - $${formatMoney(highestPrice, 2, ".", " ")}`;
            
            this.setState({ total, priceRange });
        }
    }

    renderAssignOptions = ({ fields, meta: { error } }) => {
        return (
            <div className="assign-skills">
                {error && (
                    <Message visible warning>
                        <Icon name="help" />
                        {error}
                    </Message>
                )}
                <List>
                    {fields.map((field, index, fields) => {
                        const taskMapItem = fields.get(index);

                        const styles = taskMapItem.isDisabled ? { display: "none" } : {};
                        return (
                            <List.Item key={index} style={styles}>
                                <Segment style={{ paddingBottom: "2em" }}>
                                    <Header
                                        as="h4"
                                        style={{
                                            color: "#c3922e",
                                            paddingBottom: "5px",
                                        }}
                                    >
                                        <i
                                            aria-hidden="true"
                                            style={{
                                                background:
                                                    "url(/images/icons-sprite.svg) no-repeat",
                                                backgroundSize: "112px 494px",
                                                backgroundPosition:
                                                    "-87px -251px",
                                                height: "18px",
                                                width: "18px",
                                                float: "left",
                                            }}
                                        />
                                        <div>{taskMapItem.title}</div>
                                    </Header>
                                    <div
                                        style={{
                                            paddingLeft: "15px",
                                            fontSize: "0.92857143em",
                                            fontWeight: "bold",
                                            color: "#78909c",
                                            textTransform: "none",
                                        }}
                                    >
                                        <div style={{ paddingBottom: "5px" }}>
                                            <label>{taskMapItem.team}</label>
                                        </div>
                                        <div>
                                            <label>{taskMapItem.skill}</label>
                                        </div>
                                    </div>
                                    <AssignSkillUsers
                                        field={field}
                                        taskMapItem={taskMapItem}
                                        inHouseOptions={
                                            this.props.inHouseUserOptions[
                                                taskMapItem.skillId
                                            ].users
                                        }
                                        reviewingUsers={
                                            this.props.reviewingUserOptions[
                                                taskMapItem.reviewingSkillId
                                            ].reviewers
                                        }
                                        outsourcedUsers={
                                            taskMapItem.certifiedVendors
                                        }
                                        index={index}
                                        addSelectedVendors={this.addSelectedVendors}
                                    />
                                </Segment>
                            </List.Item>
                        );
                    })}
                </List>
            </div>
        );
    };

    renderTotalAmount = () => {
        return (
            <div
                style={{
                    width: "100%",
                    textAlign: "right",
                    paddingRight: "15px",
                }}
            >
                <label>
                    {/* Total: $ {formatMoney(this.state.total, 2, ".", " ")} */}
                    Total: {this.state.priceRange}
                </label>
            </div>
        );
    };

    addSelectedVendors = (index, optionType) => {
        const {
            form: formName,
            formulaId,
            formulaTaskMaps,
            changeFieldValue, 
            loadVendorsByOptionType 
        } = this.props;

        const fieldName = `formulaTaskMaps[${index}].outsourceUserIds`;
        const formulaTaskId = formulaTaskMaps[index].formulaTaskId;

        loadVendorsByOptionType(formulaId, formulaTaskId, optionType)
            .then(() => {
                changeFieldValue(
                    formName,
                    fieldName, 
                    this.props.selectedVendors
                );
            });
    };

    render() {
        const {
            open,
            onClose,
            onBack,
            handleSubmit,
            //submitting,
            credits,
            //formulaMeanTat,
            formulaMeanTatObj,
            loading,
            formulaTaskMaps
        } = this.props;

        let totalOutsourcer = 0
        if (formulaTaskMaps !== undefined) {
            //console.log('props', formulaTaskMaps);

        const outsourcerArray = formulaTaskMaps.map (t => t.outsourceUserIds)
        //console.log('outsourcerArray',outsourcerArray);
        const outsourcerCount = outsourcerArray.map ((t) => {
            //console.log('t',t);
            return t.filter(item => item !== '00000000-0000-0000-0000-000000000000' ).length
        }    
        )
        //console.log('outsourcer',outsourcerCount);
        totalOutsourcer  = outsourcerCount.reduce((total, value) => {
            return total + value;
        },0)
        }
                //console.log('totalOutsourcer',totalOutsourcer);

        // console.log("formulaMeanTatObj", formulaMeanTatObj);
        const tasksCount = this.props.initialValues.formulaTaskMaps.length;
        const availableCredits = credits ? credits.availableCredits : 0;

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="select-project-start-date-modal"
                size="small"
                onClose={() => {
                    this.props.reset("assignSkillsForm");
                    onClose();
                }}
            >
                <Modal.Header>
                    <div style={{ display: "flex", justifyContent: "space-between" }}>
                        <div>Assign skills</div>
                        <div>
                            <i
                                className="close icon"
                                style={{ cursor: "pointer" }}
                                onClick={() => {
                                    this.props.reset("assignSkillsForm");
                                    onClose();
                                }}
                            ></i>
                        </div>
                    </div>
                </Modal.Header>

                <Modal.Content scrolling>
                    {tasksCount === 0 && (
                        <Message visible>
                            <Icon name="info" />
                            There are no assigned skills to tasks
                        </Message>
                    )}
                    {tasksCount > 0 && (
                        <Form as="section">
                            <FieldArray
                                name="formulaTaskMaps"
                                component={this.renderAssignOptions}
                            />
                        </Form>
                    )}
                </Modal.Content>

                <Modal.Actions>
                    {/* <div 
                        style={{
                            display: "flex",
                            fontSize: "0.92857143em",
                            fontWeight: "bold",
                            color: "#637b94",
                            textTransform: "none",
                            marginBottom: "5px",
                        }}
                    >
                        <div
                            style={{
                                width: "100%",
                                textAlign: "right",
                                paddingRight: "15px",
                            }}
                        >
                            <label>
                                Range: {this.state.priceRange}
                            </label>
                        </div>
                    </div> */}
                    <div
                        style={{
                            display: "flex",
                            fontSize: "0.92857143em",
                            fontWeight: "bold",
                            color: "#637b94",
                            textTransform: "none",
                            marginBottom: "20px",
                        }}
                    >
                        <div
                            style={{
                                width: "100%",
                                textAlign: "left",
                                paddingLeft: "5px",
                            }}
                        >
                            <label>
                                Available Credits: ${" "}
                                {formatMoney(availableCredits, 2, ".", " ")}
                            </label>
                        </div>
                        <div style={{ width: "100%", textAlign: "center", fontSize: "1em" }}>
                            <label style={{ fontSize: "1.12em" }}>Est. Turn Around = {formulaMeanTatObj.outsourceR_TAT} days </label>
                            <div style={{ width: "100%", textAlign: "center",display:"none" }}>
                                <label>Approx.{" "}  Total TAT = {formulaMeanTatObj.totaL_TAT} days </label>
                        </div>
                        </div>
                        
                        {this.renderTotalAmount()}
                    </div> 
                    {totalOutsourcer > 0 && 
                    (availableCredits <= 0 || availableCredits < this.state.maxRangePrice )  ? 
                    (<ModusButton
                        className="button-flex-order1"
                        filled
                        type="button"
                        style={{ background: "#988f9f"}} 
                        loading={loading}
                        content={this.props.submitLabel}
                        onClick = {() => this.props.info({ title: "You don't have enough credits" })}
                        
                    />)
                    : (<ModusButton
                        className="button-flex-order1"
                        filled
                        type="submit"
                        style={{ background:"#5d2684"}} 
                        loading={loading}
                        content={this.props.submitLabel}
                    />)}
                    {/* <ModusButton
                        className="button-flex-order2"
                        grey
                        type="button"
                        content="Cancel"
                        onClick={() => {
                            this.props.reset("assignSkillsForm");
                            onClose();
                        }}
                    /> */}
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
        loading: state.skill.loading,
        credits: state.credits.credits,
        //formulaMeanTat: state.formula.formulaMeanTat,
        formulaMeanTatObj: state.formula.formulaMeanTatObj,
        selectedVendors: state.vendor.selectedVendors,

        // redux form values
        formulaTaskMaps: assignSkillsValueSelector(state, 'formulaTaskMaps'),
    }),
    (dispatch) => {
        return bindActionCreators({ 
            changeFieldValue, 
            loadCredits, 
            loadFormulaMeanTat,
            loadVendorsByOptionType,
            success, error, info,
        }, dispatch);
    }
)(
    reduxForm({
        form: "assignSkillsForm",
        enableReinitialize: true,
        validate,
        onSubmitFail: errors => scrollToFirstError(errors),
    })(AssignSkillsModal)
);
// export default AssignSkillsModal
