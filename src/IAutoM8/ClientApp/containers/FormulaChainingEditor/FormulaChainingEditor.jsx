import React, { Component } from "react";
import { connect } from "react-redux";
import { reset } from "redux-form";
import axios from "axios";
import { success, error, info } from "react-notification-system-redux";
import autobind from "autobind-decorator";
import cn from "classnames";

import { Rating } from "semantic-ui-react";

import { getAuthHeaders } from "@infrastructure/auth";
import { getTaskConnections } from "@utils/task";
import {
    selectCurrentFormulaName,
    selectCurrentFormulaShared,
    filterFormulasByQuery,
    getState
} from "@selectors/formula";
import { requestRedrawUIEditor } from "@store/layout";
import { loadSkills } from "@store/skill";
import { clearFormulas, formulaSelector, loadFormulas } from "@store/formula";
import {
    setResources,
    updateFormulaTaskResource,
    clearResources,
} from "@store/resource";
import {
    loadTasks,
    addTask,
    editTask,
    enableTask,
    deleteTask,
    disableTask,
    addConnection,
    removeConnection,
    loadInternalTasks,
    clearTasks,
    addTodo,
    todoTypeTwo,
    addTodoTypeTwo,
    loadDisabledFormulaTasks,
} from "@store/formulaTasks";

import {
    loadFormulaTask,
    setPendingTask,
    loadFormulaOutsources,
    setFormulaOutsourceAssigne,
    clearFormulaOutsoureces,
    saveFormulaOutsource,
    loadFormulaNotes,
    createFormulaNote,
    removeFormulaNote,
    clearTaskNotes,
} from "@store/pendingTask";

import {
    ChainingEditor,
    TaskFormModal,
    Prompt,
    UserAcronym,
    ModusButton,
    TimeAgo,
    TimeEstimate,
} from "@components";

import TaskNode from "./components/TaskNode";
import FormulaTasksHeader from "./components/FormulaTasksHeader";
import SelectFormulaWizard from "./components/SelectFormulaWizard";
import GroupNode from "./components/GroupNode";

@connect(
    (state, props) => ({
        //...state.skill,
        tasks: state.formulaTasks.tasks,
        canEdit: state.formulaTasks.canEdit,
        loading: state.formulaTasks.loading,
        formulaId: props.match.params.formulaId,
        name: selectCurrentFormulaName(state, props),
        isShared: selectCurrentFormulaShared(state, props),
        formulas: filterFormulasByQuery(state),
        getState: getState(state),
        userPermissions: state.auth.permissions,
        resources: state.resource,
        pendingTask: state.pendingTask.pendingTask,
        hasMore: state.pendingTask.hasMore,
        outsources: state.pendingTask.outsources,
        todos: state.formulaTasks.todo,
        todoTypeTwo: state.formulaTasks.todoTypeTwo,
        taskNotes: state.pendingTask.taskNotes,
        grayedOutTasks: state.formulaTasks.disabledFormulaTasks,
        globalTasks: { ...state.skill },
        formulaItem: state.formula
    }),
    {
        loadSkills,
        formulaSelector,
        loadTasks,
        addTask,
        editTask,
        enableTask,
        deleteTask,
        disableTask,
        addConnection,
        removeConnection,
        reset,
        requestRedrawUIEditor,
        success,
        error,
        info,
        setResources,
        updateFormulaTaskResource,
        clearResources,
        loadFormulaTask,
        setPendingTask,
        loadInternalTasks,
        clearTasks,
        loadFormulaOutsources,
        setFormulaOutsourceAssigne,
        clearFormulaOutsoureces,
        saveFormulaOutsource,
        addTodo,
        todoTypeTwo,
        addTodoTypeTwo,
        loadFormulaNotes,
        createFormulaNote,
        removeFormulaNote,
        clearTaskNotes,
        clearFormulas,
        loadDisabledFormulaTasks,
        loadFormulas
    }
)
export default class FormulaChainingEditor extends Component {
    state = {
        taskModalOpen: false,
        submitAsEdit: false,
        isTodoChange: false,
        editDetails: [],
        isRun: true,

    };

    componentWillMount() {
        window.scrollTo(0, 0);
    }
    componentWillUnmount() {
        this.props.clearTasks();
        this.props.clearFormulas();
    }

    componentDidMount() {
        this.loadFormulaTasks(true);
        if (!this.props.formulas.length)
            this.props.history.goBack()
        //this.props.loadSkills();
        //this.props.loadFormulas(true);
        //this.props.loadFormulas();
        //this.props.loadTasks(this.props.formulaId)

        //this.props.formulaSelector();

    }

    loadFormulaTasks = (refreshStore = false) => {
        // console.log('clickeddddd');
        // console.log('this.props.formulaId',this.props.formulaId);
        return this.props
            .loadTasks(this.props.formulaId, !refreshStore) // TODO: dont reload all
            .then(() => this.props.requestRedrawUIEditor());
    };

    isFromInternalFormulaTask(...ids) {
        const _ids = ids.map((id) => Number(id));
        return this.props.tasks.some(
            (task) => task.parentTaskId !== null && _ids.includes(task.id)
        );
    }

    isAllowedToEdit = () => {
        if (!this.props.canEdit) {
            this.props.info({
                title: "Formula is locked. You cannot edit.",
            });
        }
        return this.props.canEdit;
    };
    handleConnectionAdded = (c) => {
        if (!this.isAllowedToEdit()) {
            return;
        }
        if (this.isFromInternalFormulaTask(c.sourceId, c.targetId)) {
            this.props.info({
                title: "This task from other formula",
            });
            return;
        }

        if (this.props.userPermissions.formulaUiEditorAddTaskConnection) {
            this.props.addConnection(c.sourceId, c.targetId);
        } else {
            this.props.info({
                title: "You do not have permission to add connections",
            });
        }
    };

    handleConnectionRemoved = (c) => {
        if (!this.isAllowedToEdit()) {
            return;
        }
        if (this.isFromInternalFormulaTask(c.sourceId, c.targetId)) {
            this.props.info({
                title: "This task from other formula",
            });
            return;
        }

        if (this.props.userPermissions.formulaUiEditorRemoveTaskConnection) {
            this.props.removeConnection(c.sourceId, c.targetId);
        } else {
            this.props.info({
                title: "You do not have permission to remove connections",
            });
        }
    };

    handleNodeMoved = (nodes) => {
        if (!this.isAllowedToEdit()) {
            return;
        }
        if (!this.isFromInternalFormulaTask(nodes[0].id)) {
            axios.put(`/api/formulaTask/position`, nodes, getAuthHeaders());
        }
    };


    handleDisableTask = async (id) => {
        let parsedId = id;
        const task = this.props.tasks.find((t) => t.id === id);
        if (task.parentTaskId) {
            parsedId = Number(id.replace(task.parentTaskId, ""));
        }

        const model = {
            parentFormulaId: this.props.formulaId,
            childFormulaId: task.parentTaskId,
            internalChildFormulaId: task.formulaProjectId,
            internalChildFormulaTaskId: parsedId,
        };

        await this.props.disableTask(model, id.toString());
        this.props.requestRedrawUIEditor();
    }

    handleEnableTask = async (id) => {
        let parsedId = id;
        const task = this.props.tasks.find((t) => t.id === id);
        if (task.parentTaskId) {
            parsedId = Number(id.replace(task.parentTaskId, ""));
        }

        const model = {
            parentFormulaId: this.props.formulaId,
            childFormulaId: task.parentTaskId,
            internalChildFormulaId: task.formulaProjectId,
            internalChildFormulaTaskId: parsedId,
        };

        await this.props.enableTask(model, id);
        this.props.requestRedrawUIEditor();
    }

    @autobind
    async handleDelete(id, beforeDelete) {
        if (!this.isAllowedToEdit()) {
            return;
        }
        if (this.isFromInternalFormulaTask(id)) {
            this.props.info({
                title: "This task from other formula",
            });
            return;
        }
        const confirmed = await Prompt.confirm(
            `Do you want to delete task ${this.props.tasks.find((x) => x.id === id).title
            }?`,
            "Confirm delete task",
            "tasks"
        );

        if (confirmed) {
            beforeDelete && beforeDelete();
            this.props
                .deleteTask(id)
                .then(() =>
                    this.props.success({
                        title: "Task was deleted successfully",
                    })
                )
                .catch(() => this.props.error({ title: "Cannot delete task" }));
        }
    }

    @autobind
    async handleDeleteGroup(id) {
        if (!this.isAllowedToEdit()) {
            return;
        }
        if (this.isFromInternalFormulaTask(id)) {
            this.props.info({
                title: "This task from other formula",
            });
            return;
        }
        const confirmed = await Prompt.confirm(
            `Do you want to delete task ${this.props.tasks.find((x) => x.id === id).title
            }?`,
            "Confirm delete task",
            "tasks"
        );

        if (confirmed) {
            this.props
                .deleteTask(id)
                .then(() =>
                    this.props.success({
                        title: "Task was deleted successfully",
                    })
                )
                .catch(() => this.props.error({ title: "Cannot delete task" }));
        }
    }

    handleEdit = (id) => {
        const {
            loadFormulaTask,
            loadFormulaNotes,
            formulaId,
            clearResources,
            setResources,
            tasks,
        } = this.props;
        this.toggleModal(true);
        let parsedId = id;
        const task = tasks.find((t) => t.id === id);
        if (task.parentTaskId) {
            parsedId = Number(id.replace(task.parentTaskId, ""));
        }
        loadFormulaTask(parsedId, formulaId)
            .then(({ value }) => {
                this.setState({
                    submitAsEdit: true,
                });
                value.data.condition = task.condition;
                this.props.setPendingTask(value.data);
                clearResources();
                setResources(value.data.resources);
                loadFormulaNotes(formulaId);
            })
            .catch(() => {
                this.toggleModal(false);
            });
    };

    handleAddNewNode = (x, y, type) => {
        if (!this.isAllowedToEdit()) {
            return;
        }
        if (type === "formula") {
            this.toggleFormulaWizardModal(true, x, y);
            return;
        }
        this.props.clearResources();
        this.props.loadFormulaNotes(this.props.formulaId);

        // Jai Changes//
        this.props.addTodo([]);
        this.props.addTodoTypeTwo([]);
        ////////////////////////////
        this.toggleModal(true);
        this.props.setPendingTask({
            posX: x,
            posY: y,
            formulaProjectId: this.props.formulaId,
            isConditional: type.includes("conditional"),
            isRecurrent: type.includes("recurrent"),
            isInterval: type.includes("interval"),
            duration: 1,
            reviewingSkillId: null,
            recurrenceOptions: {
                recurrenceType: 0,
            },
            isNewTask: true,
        });
    };

    handleOptionAttached = (data) => {
        if (!this.isAllowedToEdit()) {
            return;
        }
        if (this.isFromInternalFormulaTask(data.targetId, data.optionId)) {
            this.props.info({
                title: "This task from other formula",
            });
            return;
        }
        if (!this.props.userPermissions.formulaUiEditorAddTaskConnection) {
            this.props.info({
                title: "You do not have permission to add connections",
            });
            return;
        }
        axios
            .put(
                `/api/formulaTask/${data.targetId}/attach-option`,
                JSON.stringify(data.optionId),
                getAuthHeaders()
            )
            .then(() => this.loadFormulaTasks());
    };

    handleOptionDetached = (data) => {
        if (!this.isAllowedToEdit()) {
            return;
        }
        if (this.isFromInternalFormulaTask(data.targetId, data.optionId)) {
            this.props.info({
                title: "This task from other formula",
            });
            return;
        }
        if (!this.props.userPermissions.formulaUiEditorRemoveTaskConnection) {
            this.props.info({
                title: "You do not have permission to remove connections",
            });
            return;
        }
        axios
            .put(
                `/api/formulaTask/${data.targetId}/detach-option`,
                JSON.stringify(data.optionId),
                getAuthHeaders()
            )
            .then(() => this.loadFormulaTasks());
    };

    handleSubmit = (task) => {
        console.log('tasksubmit', task);
        const milestones = [];
        const milestones2 = [];
        const addTodoCheckList = {};
        const addReviewerCheckList = {};

        if (this.props.todos) {
            this.props.todos.forEach((item) => milestones.push(item.name));
            (addTodoCheckList["type"] = 1),
                (addTodoCheckList["milestones"] = milestones);
        }

        if (this.props.todoTypeTwo) {
            this.props.todoTypeTwo.forEach((item) =>
                milestones2.push(item.name)
            );
            (addReviewerCheckList["type"] = 2),
                (addReviewerCheckList["milestones"] = milestones2);
        }

        if (milestones2.length > 0) {
            task["addReviewerCheckList"] = {
                type: 2,
                milestones: milestones2,
            };
        }

        if (milestones.length > 0) {
            task["addTodoCheckList"] = {
                type: 1,
                milestones: milestones,
            };
        }

        if (!this.isAllowedToEdit()) {
            return;
        }
        if (
            task.outsources.length > 0 &&
            task.reviewingSkillId === null &&
            task.outsources.some(
                (s) => s.status !== null && s.status !== "DeclinedByOwner"
            )
        ) {
            return;
        }
        if (this.isFromInternalFormulaTask(task.id)) {
            this.props.info({
                title: "This task from other formula",
            });
            return;
        }
        // task['formulaTaskChecklists'] = this.props.pendingTask.formulaTaskChecklists
        const performAction = task.id
            ? this.props.editTask
            : this.props.addTask;

        return performAction(task)
            .then((updatedTask) => {
                this.props.updateFormulaTaskResource(
                    updatedTask.action.payload.data.id,
                    this.props.resources
                );
                this.props.saveFormulaOutsource(
                    updatedTask.action.payload.data.id
                );
            })
            .then(() => {
                this.toggleModal(false);
                this.loadFormulaTasks();
            });
    };

    toggleModal = (opened) => {
        if (!opened) {
            this.props.reset("taskFormModal");
            this.props.setPendingTask({});
            this.props.clearFormulaOutsoureces();
            this.props.clearTaskNotes();
        }

        this.setState({
            taskModalOpen: opened,
        });
    };

    handleEditTaskModalClose = () => {
        this.toggleModal(false);
    };

    /**
     * SelectFormulaWizard
     */

    toggleFormulaWizardModal(open, x = 0, y = 0) {
        this.setState({
            selectFormulaWizardOpen: open,
            formulaTaskPos: [x, y],
        });
    }

    handleAddFormulaCompleted = () => {
        this.toggleFormulaWizardModal(false);
        this.loadFormulaTasks();
    };

    @autobind
    onTodoChange(params) {
        this.setState({
            isTodoChange: true,
        });
    }

    loadInternalFormulaTasks = (formulaId, taskId) => {
        const parentFormulaId = Number(this.props.formulaId);
        return this.props.loadInternalTasks(formulaId, parentFormulaId, taskId).then(() => {
            this.props.requestRedrawUIEditor();
            return Promise.resolve();
        });
    };

    handleAddNewNotes = (taskNote) => {
        taskNote.formulaId = this.props.formulaId;

        this.props.createFormulaNote(taskNote);
    };

    handleDeleteNotes = (noteId) => {
        this.props.removeFormulaNote(noteId);
    };

    renderNode = (node, beforeDelete, taskCount) => {

        const { grayedOutTasks, formulas } = this.props;


        const global = formulas.map(item => item)

        let globalCheck = "";

        for (let i = 0; i < global.length; i++) {
            globalCheck = global[i]
        }

        const isEditable = grayedOutTasks.some(id => id === node.id);

        const isAdmin = this.props.roles.includes('Admin');
        // console.log('isAdmin',isAdmin);

        return (
            <TaskNode
                id={node.id}
                node={node}
                isEditable={isEditable}
                //formulaCheck = {revGlobalCheck}
                publicCheck={globalCheck}
                isAdmin={isAdmin}
                canDisable={
                    isAdmin && taskCount && taskCount > 1 && node.parentTaskId
                }
                onEdit={this.handleEdit}
                onDelete={(id) => this.handleDelete(id, beforeDelete)}
                onDisable={(id) => this.handleDisableTask(id)}
                onEnable={(id) => this.handleEnableTask(id)}
                isFormulaTask={true}
            />
        );
    };

    renderGroup = (
        group,
        membersCount,
        toggle,
        collapsed,
        jsPlumbBeforeDelete
    ) => {
        return (
            <GroupNode
                id={group.id}
                node={group}
                membersCount={membersCount}
                collapsed={collapsed}
                toggle={toggle}
                onDelete={(id) =>
                    this.handleDeleteGroup(id, jsPlumbBeforeDelete)
                }
            />
        );
    };
    renderVendor = (vendor, index) => {
        const text =
            vendor.status === "None"
                ? "Sent on "
                : vendor.status === "Accepted"
                    ? "Confirmed on "
                    : vendor.status === "Declined"
                        ? "Declined on "
                        : "";
        return (
            <div className="vendor-detail" key={index}>
                <div className="vendor-detail__acronym">
                    {/* Check added in case the vendor.fullName is null */}
                    <UserAcronym fullname={vendor.fullName || ""} />
                </div>
                <div className="vendor-detail__detail">
                    <div
                        className={cn([
                            "vendor-detail__name-project",
                            vendor.avgRating === null &&
                            vendor.avgResponding === null &&
                            "vendor-detail__name-project__empty",
                            vendor.avgRating === null &&
                            vendor.avgResponding !== null &&
                            "vendor-detail__name-project__partial",
                        ])}
                    >
                        {vendor.fullName}
                    </div>
                    {vendor.avgRating !== null && (
                        <Rating
                            disabled
                            maxRating={5}
                            rating={vendor.avgRating}
                            className="vendor-detail__rating"
                        />
                    )}
                    <div className="vendor-detail__avg-time">
                        {vendor.avgResponding !== null &&
                            "Avg. Time To Accept Your Invite "}
                        {vendor.avgResponding !== null && (
                            <TimeEstimate minutes={vendor.avgResponding} />
                        )}
                    </div>
                    <div className="vendor-detail__avg-time">
                        {vendor.avgWorking !== null &&
                            "Avg. Time To Complete Your Task "}
                        {vendor.avgWorking !== null && (
                            <TimeEstimate minutes={vendor.avgWorking} />
                        )}
                    </div>
                    <div className="vendor-detail__avg-time">
                        {vendor.avgMessaging !== null &&
                            "Avg. Time To Respond Your Message "}
                        {vendor.avgMessaging !== null && (
                            <TimeEstimate minutes={vendor.avgMessaging} />
                        )}
                    </div>
                </div>
                <div className="vendor-detail__request-info">
                    {text !== "" && vendor.date && (
                        <div className="vendor-detail__time">
                            {text}
                            <TimeAgo date={vendor.date} />
                        </div>
                    )}

                    {vendor.price !== null && vendor.price !== 0.0 && (
                        <div className="vendor-detail__price">
                            ${vendor.price}
                        </div>
                    )}
                    {(vendor.status === null ||
                        vendor.status === "DeclinedByOwner") && (
                            <ModusButton
                                as="a"
                                className="vendor-detail__btn"
                                content="assign"
                                onClick={() =>
                                    this.props.setFormulaOutsourceAssigne(
                                        vendor.id,
                                        true,
                                        vendor.role,
                                        vendor.ownerId,
                                        
                                    )
                                }
                            />
                        )}
                    {(vendor.status === "None" || vendor.status === "WaitingForCompanyApproval" ) && (
                        <ModusButton
                            as="div"
                            className="vendor-detail__btn decline"
                            content={<i />}
                            onClick={() =>
                                this.props.setFormulaOutsourceAssigne(
                                    vendor.id,
                                    false
                                )
                            }
                            popup="Waiting for confirmation. Do you want to cancel?"
                        />
                    )}
                    {vendor.status === "Accepted" && (
                        <ModusButton
                            as="div"
                            className="vendor-detail__btn accept"
                            content={<i />}
                        />
                    )}
                    {vendor.status === "Declined" && (
                        <ModusButton
                            as="div"
                            disabled
                            className="vendor-detail__btn"
                            content="denied"
                        />
                    )}
                </div>
            </div>
        );
    };
    render() {
        const {
            name,
            isShared,
            formulaId,
            tasks,
            loading,
            pendingTask,
            loadFormulaOutsources,
            hasMore,
            outsources,
            taskNotes,
            grayedOutTasks,
            globalTasks,
            formulaItem,
            formulas,
            getState
        } = this.props;
        // console.log('getState', getState);
        //console.log('props' , this.props);
        const globals = formulas.map(item => item)

        let globalChecks = "";

        for (let i = 0; i < globals.length; i++) {
            globalChecks = globals[i]
        }



        const isAdmin = this.props.roles.includes('Admin');

        return (
            <div className="iauto-chaining-editor">
                <FormulaTasksHeader
                    name={name}
                    isShared={isShared}
                    formulaId={formulaId}
                    taskCount={tasks.length}
                />

                <ChainingEditor
                    loading={loading}
                    publicCheck={globalChecks}
                    isAdmin={isAdmin}
                    nodes={tasks}
                    grayedOutNodes={grayedOutTasks}
                    connections={getTaskConnections(tasks)}
                    renderNode={this.renderNode}
                    renderGroup={this.renderGroup}
                    onAddNewNode={this.handleAddNewNode}
                    onNodeMoved={this.handleNodeMoved}
                    onOptionDetached={this.handleOptionDetached}
                    onOptionAttached={this.handleOptionAttached}
                    onConnectionAdded={this.handleConnectionAdded}
                    onConnectionRemoved={this.handleConnectionRemoved}
                    onLoadInternalFormulaTasks={this.loadInternalFormulaTasks}
                />
                <TaskFormModal
                    globalChecks={globalChecks}
                    isAdmin={isAdmin}
                    open={this.state.taskModalOpen}
                    initialValues={{ ...pendingTask, outsources }}
                    onSubmit={this.handleSubmit}
                    onClose={this.handleEditTaskModalClose}
                    onAddNewNotes={this.handleAddNewNotes}
                    onDeleteNotes={this.handleDeleteNotes}
                    isFormulaTask={true}
                    fetchData={loadFormulaOutsources}
                    hasMore={hasMore}
                    items={outsources}
                    endMessage="No more vendors"
                    node={this.renderVendor}
                    isTodoChange={this.onTodoChange}
                    editDetails={this.state.editDetails}
                    taskNotes={taskNotes}
                    showTrainingTab={pendingTask.showTrainingTab}
                />

                <SelectFormulaWizard
                    id={formulaId}
                    position={this.state.formulaTaskPos}
                    open={this.state.selectFormulaWizardOpen}
                    onClose={() => this.toggleFormulaWizardModal(false)}
                    onComplete={this.handleAddFormulaCompleted}
                />
            </div>
        );
    }
}
