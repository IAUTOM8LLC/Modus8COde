import React, { Component } from 'react'
import { connect } from 'react-redux'
import axios from 'axios'
import { info, success, error } from 'react-notification-system-redux'
import moment from 'moment'

import { getAuthHeaders } from '@infrastructure/auth'
import { requestRedrawUIEditor } from '@store/layout'
import { updateStatus, addConnection, removeConnection, updatePositions } from '@store/projectTasks'
import { selectWorkerSkills } from '@selectors/skill'
import { getTaskConnections } from '@utils/task'
import { validateStartDates } from '@utils/task'
import { selectGrayedTasks } from '@selectors/projectTasks'
import { clearTasks } from '@store/formulaTasks'

import {
    ChainingEditor,
    ProjectTasksHeader,
    ImportTasksFromFormulaWizard,
    Prompt
} from '@components'

import TaskNode from './components/TaskNode'
import GroupNode from './components/GroupNode'

@connect(
    (state, props) => ({
        loading: state.projectTasks.loading,
        workerSkills: selectWorkerSkills(state),
        grayedOutTasks: selectGrayedTasks(state, props)
    }),
    {
        updateStatus,
        updatePositions,
        addConnection, removeConnection,
        info, success, error,
        requestRedrawUIEditor,
        clearTasks
    }
)
export default class TaskChainingEditor extends Component {
    state = {
        importTasksFromFormulaWizardOpen: false,
        simpleImportFormulaModal: false,
        formulaTaskPos: [0, 0]
    }

    componentDidMount() {
        this.props.requestRedrawUIEditor();
    }

    /**
     * ImportTasksFromFormulaWizard
     */

    toggleImportModal(open, simple = false, x = 0, y = 0) {
        this.setState({
            importTasksFromFormulaWizardOpen: open,
            formulaTaskPos: [x, y],
            simpleImportFormulaModal: simple
        });
    }

    handleImportFromFormulaClick = () => {
        this.toggleImportModal(true);
    }

    handleImportCompleted = () => {
        this.toggleImportModal(false);
        this.props.loadProjectTasks()
            .then(() => this.props.requestRedrawUIEditor());
    }

    /**
     * ChainingEditor
     */

    handleConnectionAdded = (c, revertConnection) => {
        if (this.props.userPermissions.projectUiEditorAddTaskConnection) {
            this.props.addConnection(c.sourceId, c.targetId)
                .catch((e) => {
                    this.props.error({
                        title: e.data.message
                    });
                    revertConnection();
                });
        } else {
            this.props.info({
                title: 'You do not have permission to add connections'
            });
        }
    }

    handleConnectionRemoved = (c) => {
        if (this.props.userPermissions.projectUiEditorRemoveTaskConnection) {
            this.props.removeConnection(c.sourceId, c.targetId)
                .then(() => {
                    this.props.success({ title: 'Connection was successfully deleted' })
                });
        } else {
            this.props.info({ title: 'You do not have permission to remove connections' });
        }
    }

    handleAddNewNode = (x, y, type) => {
        if (type === "formula") {
            this.toggleImportModal(true, true, x, y);
            return;
        }

        this.props.newTask({
            posX: x,
            posY: y,
            isConditional: type.includes('conditional'),
            isRecurrent: type.includes('recurrent'),
            isInterval: type.includes('interval'),
            recurrenceOptions: { recurrenceType: 0 },
            startDate: moment()
                .add(5, 'm')
                .format('MMM D, YYYY h:mm A')
        });
    }

    handleOptionAttached = (data) => {
        if (!this.props.userPermissions.projectUiEditorAddTaskConnection) {
            this.props.info({
                title: 'You do not have permission to add connections'
            });
            return;
        }

        axios.put(`/api/tasks/${data.targetId}/attach-option`,
            JSON.stringify(data.optionId), getAuthHeaders()
        ).then(() => this.props.loadProjectTasks());
    }

    handleOptionDetached = (data) => {
        if (!this.props.userPermissions.projectUiEditorRemoveTaskConnection) {
            this.props.info({
                title: 'You do not have permission to remove connections'
            });
            return;
        }
        axios.put(`/api/tasks/${data.targetId}/detach-option`,
            JSON.stringify(data.optionId), getAuthHeaders()
        ).then(() => this.props.loadProjectTasks());
    }

    validateAddNewConnection = (sourceId, targetId) => {
        if (validateStartDates(this.props.tasks, sourceId, targetId).length > 0) {
            return Prompt.confirm(
                'Confirm add connection that will update start dates?',
                `After adding this connection system will update all invalid childrens
                start dates to match the rule: "Child task must start after its parent".
                Are you sure?`,
                'compress'
            );
        }

        return Promise.resolve(true);
    }

    renderNode = (node, beforeDelete) => {
        return (
            <TaskNode
                id={node.id}
                node={node}
                skills={this.props.workerSkills}
                onEdit={this.props.editTask}
                onDelete={(id) => this.props.deleteTask(id, beforeDelete)}
                onChangeProcessingUser={this.props.onChangeProcessingUser}
                onOutsourceTabOpen={this.props.onOutsourceTabOpen}
                onStopOutsource={this.props.onStopOutsource}
                hasAcceptedVendor={this.props.hasAcceptedVendor}
            />
        )
    }

    renderGroup = (group, membersCount, toggle, collapsed) => {
        return (
            <GroupNode
                id={group.id}
                node={group}
                membersCount={membersCount}
                collapsed={collapsed}
                toggle={toggle}
                onDelete={(id) => this.props.deleteTask(id)}
            />
        )
    }

    componentWillUnmount() {
        this.props.clearTasks();
    }

    render() {
        const {
            projectName,
            projectId,
            match,
            tasks,
            loading,
            overdueCount,
            grayedOutTasks,
            onShowResources,
            onOutsourceTabOpen
        } = this.props;

        return (
            <div className="iauto-chaining-editor">
                <ProjectTasksHeader
                    projectId={projectId}
                    name={projectName}
                    path={match.path}
                    taskCount={tasks.length}
                    overdueCount={overdueCount}
                    onImportFormula={this.handleImportFromFormulaClick}
                    onShowResources={onShowResources}
                />

                <ChainingEditor
                    isForProjects
                    loading={loading}
                    nodes={tasks}
                    grayedOutNodes={grayedOutTasks}
                    connections={getTaskConnections(tasks, grayedOutTasks)}
                    renderNode={this.renderNode}
                    renderGroup={this.renderGroup}
                    onAddNewNode={this.handleAddNewNode}
                    onNodeMoved={this.props.updatePositions}
                    onOptionDetached={this.handleOptionDetached}
                    onOptionAttached={this.handleOptionAttached}
                    onConnectionAdded={this.handleConnectionAdded}
                    onConnectionRemoved={this.handleConnectionRemoved}
                    validateAddNewConnection={this.validateAddNewConnection}
                />

                <ImportTasksFromFormulaWizard
                    projectId={projectId}
                    open={this.state.importTasksFromFormulaWizardOpen}
                    simple={this.state.simpleImportFormulaModal}
                    position={this.state.formulaTaskPos}
                    onClose={() => this.toggleImportModal(false)}
                    onImported={this.handleImportCompleted}
                />
            </div>
        );
    }
}
