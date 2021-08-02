import React, { Component } from "react";
import {
    reduxForm,
    formValueSelector,
    getFormSyncErrors,
    getFormMeta,
    getFormValues,
} from "redux-form";
import { connect } from "react-redux";
import { Modal, Tab, Icon, Popup, Loader, Dimmer } from "semantic-ui-react";

import { ModusButton, OutsourcePane } from "@components";

import { required, dateGreaterThen, onlyFutureDate } from "@utils/validators";
import { canEdit as canEditTask, getTabsWithErrors } from "@utils/task";
import {
    filterFormulasByQuery
} from "@selectors/formula";
import {
    hideTrainingNotification,
} from "@store/pendingTask";
import { updateTaskTodos } from "@store/projectTasks";
import { selectTasksForOptions as selectProjectTasksForOptions } from "@selectors/projectTasks";
import { selectTasksForOptions as selectFormulaTasksForOptions } from "@selectors/formulaTasks";

import MembersPane from "./components/MemberPanes/MembersPane";
import FormulaMembersPane from "./components/MemberPanes/FormulaMembersPane";
import SchedulePane from "./components/SchedulePane";
import FormulaSchedulePane from "./components/FormulaSchedulePane";
import RecurrencePane from "./components/RecurrencePane";
import InformationPane from "./components/InformationPane";
import ConditionalPane from "./components/ConditionalPane";
import IntervalPane from "./components/IntervalPane";
import CommentPane from "./components/CommentPane";

import "./TaskFormModal.less";

const validate = (values, props) => {
    const {
        id,
        teamId,
        dueDate,
        startDate,
        title,
        isInterval,
        status,
        parentTasks,
        conditionalParentTasks,
        assignedUserIds,
        reviewingUserIds,
        outsources,
        assignedSkillId,
    } = values;


    const { isFormulaTask, allTasks, items } = props;

    const errors = {
        title: required(title),
        assignedUserIds: required(assignedUserIds),
    };

    let validateStartDates = false;
    if (!isFormulaTask && !isInterval && status === "New") {
        if (id) {
            const runningTaskIds = allTasks
                .filter((t) => t.isInProgress)
                .map((t) => t.id);
            const hasRunningParents = [
                ...parentTasks,
                ...conditionalParentTasks,
            ].some((parentId) => runningTaskIds.includes(parentId));

            validateStartDates = !hasRunningParents;
        } else {
            validateStartDates = true;
        }
    }

    if (isFormulaTask && !teamId) {
        errors.assignedTeamId = "Required";
    }

    if (isFormulaTask && !teamId && !assignedSkillId) {
        errors.assignedSkillId = "Required";
    }

    if (assignedUserIds && assignedUserIds.length === 0) {
        errors.assignedUserIds = "Required";
    }

    if (items && items.length > 0) {
        errors.assignedUserIds = null;
    }

    if (validateStartDates) {
        const { formMeta } = props;

        const canEdit =
            props.isFormulaTask ||
            (props.userPermissions.editProjectTask &&
                canEditTask(props.initialValues));

        const shouldValidateStartDate =
            canEdit &&
            formMeta &&
            formMeta.startDate &&
            formMeta.startDate.touched &&
            formMeta.startDate.visited;

        errors.startDate =
            required(startDate) ||
            (shouldValidateStartDate && onlyFutureDate(startDate));
        errors.dueDate =
            onlyFutureDate(dueDate) ||
            dateGreaterThen(dueDate, startDate, "a Start date");
    }
    if (
        outsources &&
        outsources.length > 0 &&
        (!reviewingUserIds || reviewingUserIds.length === 0) &&
        outsources.some(
            (s) => s.status !== null && s.status !== "DeclinedByOwner"
        )
    ) {
        errors.reviewingUserIds = "Required";
    }
    return errors;
};

const taskFormValueSelector = formValueSelector("taskFormModal");

@connect(
    (state, props) => ({
        userPermissions: state.auth.permissions,
        formulas: filterFormulasByQuery(state),
        tasks: props.isFormulaTask
            ? selectFormulaTasksForOptions(state, props)
            : selectProjectTasksForOptions(state, props),
        allTasks: state.projectTasks.tasks,
        loading: state.pendingTask.loading,
        showTrainingNotification: state.pendingTask.showTrainingNotification,
        todos: state.projectTasks.todos,

        // redux form props
        formErrors: getFormSyncErrors("taskFormModal")(state),
        recurrenceType: taskFormValueSelector(
            state,
            "recurrenceOptions.recurrenceType"
        ),
        teamId: taskFormValueSelector(state, "teamId"),
        isAutomated: taskFormValueSelector(state, "isAutomated"),
        startDate: taskFormValueSelector(state, "startDate"),
        //assignedTeamId: taskFormValueSelector(state, "assignedTeamId"),
        assignedSkillId: taskFormValueSelector(state, "assignedSkillId"),
        reviewingSkillId: taskFormValueSelector(state, "reviewingSkillId"),
        dueDate: taskFormValueSelector(state, "dueDate"),
        duration: taskFormValueSelector(state, "duration"),
        assignedUserIds: taskFormValueSelector(state, "assignedUserIds"),
        reviewingUserIds: taskFormValueSelector(state, "reviewingUserIds"),
        isTrainingLocked: taskFormValueSelector(state, "isTrainingLocked"),
        formMeta: getFormMeta("taskFormModal")(state),
        formValues: getFormValues("taskFormModal")(state),
    }),
    {
        hideTrainingNotification,
        updateTaskTodos,
    }
)
@reduxForm({
    form: "taskFormModal",
    validate,
    enableReinitialize: true,
})
export default class TaskFormModal extends Component {
    constructor(props) {
        super(props);
        this.state = {
            isPublishedClicked: false,
        };
    }

    editPermissions() {
        const { isFormulaTask, userPermissions } = this.props;

        const hasPermissionToEdit =
            (isFormulaTask && userPermissions.formulaUiEditorEditTask) ||
            userPermissions.editProjectTask;

        if (hasPermissionToEdit) return {};

        return {
            popup: "You have permissions to check/un-check the"
                + " checklist items, upload files and to add links",
        };
    }

    canEdit() {
        return (
            this.props.isFormulaTask ||
            (this.props.userPermissions.editProjectTask &&
                canEditTask(this.props.initialValues))
        );
    }

    renderSchedulePane(canEdit) {
        const {
            change,
            duration,
            dueDate,
            startDate,
            isAutomated,
            initialValues: { isConditional },
            formMeta,
        } = this.props;

        const commonProps = {
            isConditional,
            isAutomated,
        };

        const menuItem = {
            key: "schedule",
            content: "Schedule",
        };

        const pane = {
            key: "schedule",
            content: this.props.isFormulaTask ? (
                <FormulaSchedulePane {...commonProps} />
            ) : (
                    <SchedulePane
                        {...commonProps}
                        changeField={change}
                        duration={duration}
                        dueDate={dueDate}
                        startDate={startDate}
                        canEdit={canEdit}
                        formMeta={formMeta}
                    />
                ),
        };

        return { menuItem, pane };
    }

    renderTab() {
        const {
            loading,
            tasks,
            isFormulaTask,
            showTrainingNotification,
            hideTrainingNotification,
            // redux-form props
            change,
            initialValues: {
                isConditional,
                isRecurrent,
                isInterval,
                recurrenceOptions,
                id,
                duration,
                formulaProjectId,
                teamId,
            },
            // redux form fields
            isAutomated,
            recurrenceType,
            submitFailed,
            anyTouched,
            reviewingSkillId,
            userPermissions,
            fetchData,
            hasMore,
            items,
            endMessage,
            node,
            credits,
            isOutsourceOpenByDefault,
            isCommentOpenByDefault,
            taskNotes,
            onAddNewNotes,
            onDeleteNotes,
            onPublishNotes,
            isTrainingLocked,
        } = this.props;
        const canEdit = this.canEdit();

        let panes = [];
        if (isInterval) {
            panes = [
                {
                    menuItem: {
                        key: "intervalSettings",
                        icon: "resize horizontal",
                        content: "Interval",
                    },
                    pane: {
                        key: "intervalSettings",
                        content: <IntervalPane canEdit={canEdit} />,
                    },
                },
            ];
        } else {
            panes = [
                {
                    menuItem: { key: "information", content: "Information" },
                    pane: {
                        key: "information",
                        content: (
                            <InformationPane
                                isFormulaTask={isFormulaTask}
                                showTrainingNotification={
                                    showTrainingNotification
                                }
                                hideTrainingNotification={
                                    hideTrainingNotification
                                }
                                onAddNewNotes={onAddNewNotes}
                                onDeleteNotes={onDeleteNotes}
                                onPublishNotes={onPublishNotes}
                                taskNotes={taskNotes}
                                formulaId={formulaProjectId}
                                taskId={id}
                                showTrainingTab={this.props.showTrainingTab}
                                isTrainingLocked={isTrainingLocked}
                                isTrainingLockIconVisible={
                                    userPermissions.formulaTaskLockTraining
                                }
                                isEdited={this.props.isEdited}
                                projectId={this.props.formValues.projectId}
                            />
                        ),
                    },
                },
                isConditional && {
                    menuItem: { key: "conditional", content: "Conditions" },
                    pane: {
                        key: "conditional",
                        content: (
                            <ConditionalPane tasks={tasks} canEdit={canEdit} />
                        ),
                    },
                },
                !isRecurrent && this.renderSchedulePane(canEdit),
                isRecurrent && {
                    menuItem: { key: "recurrence", content: "Recurrence" },
                    pane: {
                        key: "recurrence",
                        content: (
                            <RecurrencePane
                                isAutomated={isAutomated}
                                isConditional={isConditional}
                                recurrenceType={recurrenceType}
                                isFormulaTask={isFormulaTask}
                                cronTab={recurrenceOptions.cronTab}
                                changeField={change}
                                canEdit={canEdit}
                                isAsap={recurrenceOptions.isAsap}
                            />
                        ),
                    },
                },
                userPermissions.editProjectTask && {
                    menuItem: { key: "members", content: "ASSIGN" },
                    pane: {
                        key: "members",
                        content: isFormulaTask ? (
                            <FormulaMembersPane
                                reviewingSkillId={reviewingSkillId}
                                changeField={change}
                                //reviewerTraining ={this.props.initialValues.pendingTask}
                                teamId={teamId}
                            />
                        ) : (
                                <MembersPane
                                    canEdit={canEdit}
                                    changeField={change}
                                />
                            ),
                    },
                },
                !isFormulaTask &&
                id && {
                    menuItem: { key: "comments", content: "Comments" },
                    pane: {
                        key: "comments",
                        content: <CommentPane taskId={id} />,
                    },
                },
                ((isFormulaTask && duration !== undefined) ||
                    (!isFormulaTask &&
                        id &&
                        userPermissions.canAssignVendor)) && {
                    menuItem: { key: "outsource", content: "Outsource" },
                    pane: {
                        key: "outsource",
                        content: (
                            <OutsourcePane
                                fetchData={() => {
                                    if (fetchData) fetchData(id ? id : 0);
                                }}
                                hasMore={hasMore}
                                items={items}
                                endMessage={endMessage}
                                node={node}
                                credits={credits}
                            />
                        ),
                        className: "vendors",
                    },
                },
            ].filter((pane) => pane);

            const invalidTabs = getTabsWithErrors(this.props.formErrors);


            if (invalidTabs.length) {
                panes.map((p) => {
                    if (submitFailed || anyTouched) {
                        const pane = invalidTabs.find(
                            (f) => f && f.pane && f.pane === p.menuItem.key
                        );

                        if (pane) {
                            p.menuItem.icon = (
                                <Popup
                                    trigger={
                                        <Icon name="warning sign" color="red" />
                                    }
                                    content={pane.message}
                                />
                            );
                        }
                    }
                });
            }
        }
        let index = 0;
        if (isOutsourceOpenByDefault) {
            const outsourcePaneIndex = panes.findIndex(
                (pane) => pane.menuItem.key === "outsource"
            );
            if (outsourcePaneIndex !== -1) index = outsourcePaneIndex;
        }
        if (isCommentOpenByDefault) {
            const commentPaneIndex = panes.findIndex(
                (pane) => pane.menuItem.key === "comments"
            );
            if (commentPaneIndex !== -1) index = commentPaneIndex;
        }

        return loading && !isFormulaTask ? null : (
            <Tab
                panes={panes}
                defaultActiveIndex={index}
                renderActiveOnly={false}
                menu={{ secondary: true, pointing: true }}
            />
        );
    }

    render() {
        const {
            open,
            onClose,
            // redux-form props
            submitting,
            initialValues,
            handleSubmit,
            formulas,
            loading,
            isEdited,
            editable,
            isAdmin,
            role,
            formValues
        } = this.props;
        
        
        const styles = isEdited || isEdited === undefined  
        ? {}: { pointerEvents: "none", opacity: "0.9" };
        const globals = formulas.map(item => item)

        let globalChecks = "";

        for (let i = 0; i < globals.length; i++) {
            globalChecks = globals[i]
        }
        
        if (!open) return null;

        return (
            role === 'Vendor' || role === 'CompanyWorker' || isAdmin ? (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="task-details-modal tab-modal"
                onClose={onClose}
                //style={styles}
            >
                <Dimmer inverted active={loading} size="large">
                    <Loader>Loading</Loader>
                </Dimmer>
                <Modal.Header>
                    {initialValues.isInterval
                        ? "Interval details"
                        : "Task details"}
                    <i
                        className="close icon"
                        style={{ margin: "0 0 0 82%", cursor: "pointer" }}
                        onClick={onClose}
                    ></i>
                </Modal.Header>

                {this.renderTab()}

                <Modal.Actions className="modal-flex-actions">


                    {this.props.showCompleteAndPublish && (
                        <ModusButton
                            className="button-flex-order1"
                            {...this.editPermissions()}
                            filled={
                                this.props.showTrainingNotification
                                    ? false
                                    : this.props.todos.every(function (
                                        element
                                    ) {
                                        if (element.todoIsChecked !== null) {
                                            if (
                                                element.todoIsChecked === true
                                            )
                                                return true;
                                            else return false;
                                        }
                                        if (
                                            element.reviewerIsChecked !== null
                                        ) {
                                            if (
                                                element.reviewerIsChecked ===
                                                true
                                            )
                                                return true;
                                            else return false;
                                        }
                                    })
                            }
                            disabled={
                                this.props.showTrainingNotification
                                    ? true
                                    : this.props.todos.some(function (
                                        element
                                    ) {
                                        if (element.todoIsChecked !== null) {
                                            if (
                                                element.todoIsChecked ===
                                                false
                                            )
                                                return true;
                                            else return false;
                                        }

                                        if (
                                            element.reviewerIsChecked !== null
                                        ) {
                                            if (
                                                element.reviewerIsChecked ===
                                                false
                                            )
                                                return true;
                                            else return false;
                                        }
                                    })
                            }
                            content="Complete Task"
                            type="button"
                            order="2"
                            onClick={() => {
                                this.props.onCompleteTask(this.props.formValues);
                                this.props.handleSavePublish(this.props.todos);
                                this.setState({
                                    isPublishedClicked: true,
                                });
                            }}
                            loading={
                                this.state.isPublishedClicked
                                    ? submitting
                                    : null
                            }
                            //style={{ background: isEdited ?  "#5d2684" : "#988f9f"}} //3-3-2021 by AT
                        />
                    )}



                    {isAdmin != undefined && isAdmin && 
                     this.props.isFormulaTask && (//isAdmin != undefined && isAdmin &&
                        <ModusButton
                            className="button-flex-order2"
                            {...this.editPermissions()}
                            filled
                            content={`Save & Exit`}
                            type="submit"
                            order="1"
                            //style={{ background: isEdited ? "#988f9f" : "#5d2684" }}
                            onClick={() => {
                                this.props.isFormulaTask
                                    ? ""
                                    : this.props.updateTaskTodos(this.props.todos)
                                this.setState({
                                    isPublishedClicked: false,
                                });
                            }}
                            loading={
                                this.state.isPublishedClicked ? null : submitting
                            }
                        />)}
                        {!this.props.isFormulaTask && (
                        <ModusButton
                            className="button-flex-order2"
                            {...this.editPermissions()}
                            filled
                            content={`Save & Exit`}
                            type="submit"
                            order="1"
                            //style={{ background: isEdited ? "#5d2684":"#988f9f" }}  //3-3-2021 by AT
                            onClick={() => {
                                this.props.isFormulaTask
                                    ? ""
                                    : this.props.updateTaskTodos(this.props.todos)
                                this.setState({
                                    isPublishedClicked: false,
                                });
                            }}
                            loading={
                                this.state.isPublishedClicked ? null : submitting
                            }
                        />)}

                    {/* ) */}
                    {/* } */}

                    {this.props.isFormulaTask && isAdmin != undefined && !isAdmin 
                    && globalChecks.type != undefined
                        && globalChecks.type === "public" && ( 
                        <ModusButton
                            {...this.editPermissions()}
                            className="button-flex-order2"
                            filled
                            // style={{
                            //     cursor: "not-allowed", opacity: "0.5",
                            //     background: isEdited ? "#988f9f" : "#5d2684"
                            // }}
                            content={`Save & Exit`}
                            type="submit"
                            order="1"
                            onClick={() => {
                            }}

                        />

                        )}

                    {this.props.isFormulaTask  && isAdmin != undefined && !isAdmin 
                    && globalChecks.type != undefined
                        && globalChecks.type === "custom" &&  (
                        <ModusButton
                            className="button-flex-order2"
                            {...this.editPermissions()}
                            filled
                            content={`Save & Exit`}
                            type="submit"
                            order="1"
                            //style={{ background: isEdited ? "#988f9f" : "#5d2684" }}
                            onClick={() => {
                                this.props.isFormulaTask
                                    ? ""
                                    : this.props.updateTaskTodos(this.props.todos)
                                this.setState({
                                    isPublishedClicked: false,
                                });
                            }}
                            loading={
                                this.state.isPublishedClicked ? null : submitting
                            }
                        />)}
                </Modal.Actions>
            </Modal>
             ) :
             (
                <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="task-details-modal tab-modal"
                onClose={onClose}
                style={styles}
            >
                <Dimmer inverted active={loading} size="large">
                    <Loader>Loading</Loader>
                </Dimmer>
                <Modal.Header>
                    {initialValues.isInterval
                        ? "Interval details"
                        : "Task details"}
                    <i
                        className="close icon"
                        style={{ margin: "0 0 0 82%", cursor: "pointer" }}
                        onClick={onClose}
                    ></i>
                </Modal.Header>

                {this.renderTab()}

                <Modal.Actions className="modal-flex-actions">


                    {this.props.showCompleteAndPublish && (
                        <ModusButton
                            className="button-flex-order1"
                            {...this.editPermissions()}
                            filled={
                                this.props.showTrainingNotification
                                    ? false
                                    : this.props.todos.every(function (
                                        element
                                    ) {
                                        if (element.todoIsChecked !== null) {
                                            if (
                                                element.todoIsChecked === true
                                            )
                                                return true;
                                            else return false;
                                        }
                                        if (
                                            element.reviewerIsChecked !== null
                                        ) {
                                            if (
                                                element.reviewerIsChecked ===
                                                true
                                            )
                                                return true;
                                            else return false;
                                        }
                                    })
                            }
                            disabled={
                                this.props.showTrainingNotification
                                    ? true
                                    : this.props.todos.some(function (
                                        element
                                    ) {
                                        if (element.todoIsChecked !== null) {
                                            if (
                                                element.todoIsChecked ===
                                                false
                                            )
                                                return true;
                                            else return false;
                                        }

                                        if (
                                            element.reviewerIsChecked !== null
                                        ) {
                                            if (
                                                element.reviewerIsChecked ===
                                                false
                                            )
                                                return true;
                                            else return false;
                                        }
                                    })
                            }
                            content="Complete Task"
                            type="button"
                            order="2"
                            onClick={() => {
                                this.props.onCompleteTask(this.props.formValues);
                                this.props.handleSavePublish(this.props.todos);
                                this.setState({
                                    isPublishedClicked: true,
                                });
                            }}
                            loading={
                                this.state.isPublishedClicked
                                    ? submitting
                                    : null
                            }
                            style={{ background: isEdited ?  "#5d2684" : "#988f9f"}} //3-3-2021 by AT
                        />
                    )}



                    {isAdmin != undefined && isAdmin && 
                     this.props.isFormulaTask && (//isAdmin != undefined && isAdmin &&
                        <ModusButton
                            className="button-flex-order2"
                            {...this.editPermissions()}
                            filled
                            content={`Save & Exit`}
                            type="submit"
                            order="1"
                            style={{ background: isEdited ? "#988f9f" : "#5d2684" }}
                            onClick={() => {
                                this.props.isFormulaTask
                                    ? ""
                                    : this.props.updateTaskTodos(this.props.todos)
                                this.setState({
                                    isPublishedClicked: false,
                                });
                            }}
                            loading={
                                this.state.isPublishedClicked ? null : submitting
                            }
                        />)}
                        {!this.props.isFormulaTask && (
                        <ModusButton
                            className="button-flex-order2"
                            {...this.editPermissions()}
                            filled
                            content={`Save & Exit`}
                            type="submit"
                            order="1"
                            style={{ background: isEdited ? "#5d2684":"#988f9f" }}  //3-3-2021 by AT
                            onClick={() => {
                                this.props.isFormulaTask
                                    ? ""
                                    : this.props.updateTaskTodos(this.props.todos)
                                this.setState({
                                    isPublishedClicked: false,
                                });
                            }}
                            loading={
                                this.state.isPublishedClicked ? null : submitting
                            }
                        />)}

                    {/* ) */}
                    {/* } */}

                    {this.props.isFormulaTask && isAdmin != undefined && !isAdmin 
                    && globalChecks.type != undefined
                        && globalChecks.type === "public" && ( 
                        <ModusButton
                            {...this.editPermissions()}
                            className="button-flex-order2"
                            filled
                            style={{
                                cursor: "not-allowed", opacity: "0.5",
                                background: isEdited ? "#988f9f" : "#5d2684"
                            }}
                            content={`Save & Exit`}
                            type="submit"
                            order="1"
                            onClick={() => {
                            }}

                        />

                        )}

                    {this.props.isFormulaTask  && isAdmin != undefined && !isAdmin 
                    && globalChecks.type != undefined
                        && globalChecks.type === "custom" &&  (
                        <ModusButton
                            className="button-flex-order2"
                            {...this.editPermissions()}
                            filled
                            content={`Save & Exit`}
                            type="submit"
                            order="1"
                            style={{ background: isEdited ? "#988f9f" : "#5d2684" }}
                            onClick={() => {
                                this.props.isFormulaTask
                                    ? ""
                                    : this.props.updateTaskTodos(this.props.todos)
                                this.setState({
                                    isPublishedClicked: false,
                                });
                            }}
                            loading={
                                this.state.isPublishedClicked ? null : submitting
                            }
                        />)}
                </Modal.Actions>
            </Modal>
             )
            
            
        );
    }
}
