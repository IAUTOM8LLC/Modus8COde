import React, { Component } from 'react'
import { Segment, Divider } from 'semantic-ui-react'
import { connect } from 'react-redux'
import { success, error, info } from 'react-notification-system-redux'
import { push } from 'react-router-redux'
import sortBy from 'lodash/sortBy'
import moment from 'moment'
import autobind from 'autobind-decorator'

import InfiniteScroll from 'react-infinite-scroll-component';

import { ALL_PROJECTS_ID } from '@constants/projectCostants'

import { tasksExceptIntervals } from '@utils/task'
import { setResources, updateProjectTaskResource } from '@store/resource'
import {
    updateStatus, completeConditionalTask,
    loadTasksForAllProjects,
    doTask, reviewTask,doVendorTask,
} from '@store/projectTasks'

import { selectGrayedTasks } from '@selectors/projectTasks'
import { clearTasks } from '@store/formulaTasks'

import {
    KanbanBoard,
    ProjectTasksHeader,
    ImportTasksFromFormulaWizard,
    ConditionalTaskModal,
    RatingModal,
    WhenPermitted,
    ProjectHeader
} from '@components'

@connect(
    (state, props) => ({
        roles: state.auth.user.roles,
        tasks: sortBy(tasksExceptIntervals(props.tasks), t => moment(t.dueDate).valueOf()),
        hasMoreProjects: state.projectTasks.hasMoreProjects,
        resources: state.resource,
        projects: state.project.projects,
        grayedOutTasks: selectGrayedTasks(state, props)
    }),
    {
        completeConditionalTask,
        updateStatus,
        error, info,
        doTask,
        doVendorTask,
        reviewTask,
        setResources,
        loadTasksForAllProjects,
        updateProjectTaskResource,
        pushState: push,
        clearTasks
    }
)
export default class KanbanEditor extends Component {
    state = {
        importTasksFromFormulaWizardOpen: false
    }
    handleAddCard = (type) => {
        this.props.newTask({
            isConditional: type.includes('conditional'),
            isRecurrent: type.includes('recurrent'),
            recurrenceOptions: { recurrenceType: 0 },
        });
    }

    getProjectName = (projectId) => {
        const {
            projects
        } = this.props;
        const project = projects.find(t => t.id === projectId);
        return project ? project.name : '';
    };

    handleEditCard = (taskId) => {
        const {
            editProjectTask,
            projectTaskEditorModalViewNotEdit
        } = this.props.userPermissions;

        if (editProjectTask || projectTaskEditorModalViewNotEdit) {
            this.props.editTask(taskId);
        } else {
            this.props.info({ title: 'You do not have permission to edit task' });
        }
    }

    @autobind
    async handleCardStatusUpdate(blockId, status) {
        const {
            tasks,
            completeConditionalTask,
            canAssignVendor
        } = this.props;
        const task = tasks.find(t => t.id === blockId);

        if (task.isConditional) {
            if (task.needsReview) {
                let rating = null;
                if (status === 'Completed' && task.hasAcceptedVendor && canAssignVendor)
                    rating = await this.ratingTaskModal.show(task.condition);
                await completeConditionalTask(blockId, 0, status, rating);
                this.props.checkProjectIdAndLoadTasks(task.projectId);
            }
            else {
                const selectedOptionId = await this.conditionalTaskModal.show(task.condition);
                await completeConditionalTask(blockId, selectedOptionId, status);
                this.props.checkProjectIdAndLoadTasks(task.projectId);
            }
        } else {
            let rating = null;
            if (task.status === 'NeedsReview' && status === 'Completed' &&
                task.hasAcceptedVendor && canAssignVendor)
                rating = await this.ratingTaskModal.show(task.condition);
            await this.props.updateStatus(blockId, status, rating);
        }
    }

    handleUpdateStatusError = (error, taskId) => {
        if (typeof error === 'string') {
            this.props.error({ title: error });
        } else if (error.title) {
            this.props.error(error);
        } else if (error.status && error.status !== 500) {
            this.props.error({ title: error.data.message });
        } else {
            this.props.error({ title: 'Cannot update task status' });
        }

        const task = this.props.tasks.find(t => t.id === taskId);
        this.props.checkProjectIdAndLoadTasks(task.projectId);
    }

    toggleImportModal = (open) => {
        this.setState({
            importTasksFromFormulaWizardOpen: open
        });
    }

    handleImportFromFormulaClick = () => {
        this.toggleImportModal(true);
    }

    handleImportCompleted = () => {
        this.toggleImportModal(false);
        this.props.loadProjectTasks();
    }

    validateStatusChange = (taskId, newStatus) => {
        return new Promise((resolve, reject) => {
            if (!this.props.userPermissions.changeProjectTaskStatus) {
                reject({
                    title: 'You do not have permission to change task status'
                });
            }

            const { tasks } = this.props;
            const task = tasks.find(t => t.id === taskId);

            if (newStatus === 'New')
                reject({ title: 'Cannot change to upcoming status manually' });

            if (!task)
                reject({ title: 'Cannot find task' });

            if (newStatus === task.status)
                reject(null);

            if (task.parentTasks.length > 0 || task.conditionalParentTasks.length > 0) {
                if (task.parentTasks.length > 0) {
                    let isDisabled = false;
                    task.parentTasks.map(t => {
                        const pt = tasks.find(p => p.id === t);
                        if (pt && pt.isDisabled) {
                            isDisabled = true;
                            return;
                        }
                        return;
                    })

                    if (isDisabled) {
                        resolve(true);
                    }
                } 

                const parents = tasks.filter(t =>
                    task.parentTasks.indexOf(t.id) >= 0);
                const condParents = tasks.filter(t => task.conditionalParentTasks.indexOf(t.id) >= 0);

                const dep = parents.filter(t => !t.isCompleted);
                if (dep.length > 0 || (condParents.length > 0 &&
                    !condParents.some(t => t.isCompleted &&
                        t.condition.options.some(o => o.assignedTaskId === taskId && o.isSelected)))) {
                    const names = dep.map(p => p.title).join(',\n');
                    reject({
                        title: "Can't change task status",
                        message: `This task depends on some uncompleted task(s): \n\n ${names}`
                    });
                }
            }

            if (task.isConditional) {

                if (newStatus === 'Completed' &&
                    task.reviewingUserIds.length > 0 && !task.needsReview) {
                    reject({
                        title: 'This task need to be reviewed'
                    });
                } else if (newStatus === 'NeedsReview' && task.reviewingUserIds.length === 0) {
                    reject({
                        title: 'This task cannot be reviewed'
                    });
                }
            }

            resolve(true);
        });
    }

    handleDoCard = (taskId) => {
        this.props.doTask(taskId);
    }

    handleDoVendorCard = (taskId) => {
        this.props.doVendorTask(taskId);
    }

    handleReviewCard = (taskId) => {
        //console.log('handleReviewCard',taskId);
        this.props.reviewTask(taskId);
    }

    componentWillUnmount() {
        this.props.clearTasks();
    }

    loadNextProjects = () => {
        if (this.props.loadTasksForAllProjects)
            this.props.loadTasksForAllProjects();
    }

    goToListView = () => {
        this.props.setProjectsToLoad(this.props.projects.map(t => t.id));
        let statusesToLoad = "InProgress";
        
        if (this.props.userPermissions.canAssignVendor) {
            statusesToLoad += ",New,NeedsReview"
        }
        this.props.loadTasksInStatus(statusesToLoad);
    }

    render() {
        const {
            tasks,
            match,
            projectName,
            projectId,
            projects,
            overdueCount,
            grayedOutTasks,
            roles,
            onChangeProcessingUser,
            loadTasksForAllProjects,
            onStopOutsource,
            onOutsourceTabOpen,
            hasMoreProjects,
            canAssignVendor
        } = this.props;
        const isVendor = roles.includes('Vendor');

        const filteredTasks = tasks.filter(t => t.formulaId === null);
        const projectIds = projects.map(t => t.id);

        if (projectId !== ALL_PROJECTS_ID) {
            return (
                <div>
                    <ProjectTasksHeader
                        projectId={projectId}
                        path={match.path}
                        params={match.params}
                        name={projectName}
                        taskCount={tasks.length}
                        overdueCount={overdueCount}
                        onAddCard={this.handleAddCard}
                        onImportFormula={this.handleImportFromFormulaClick}
                        onShowResources={this.props.onShowResources}
                    />

                    <WhenPermitted rule="projectTaskEditorModalViewNotEdit">
                        {(permitted) =>
                            <KanbanBoard
                                blocks={filteredTasks}
                                onDoCard={this.handleDoCard}
                                onDoVendorCard={this.handleDoVendorCard}
                                onReviewCard={this.handleReviewCard}
                                onEditCard={this.handleEditCard}
                                onChangeProcessingUser={onChangeProcessingUser}
                                onStopOutsource={onStopOutsource}
                                onDeleteCard={this.props.deleteTask}
                                onCardStatusUpdate={this.handleCardStatusUpdate}
                                validateStatusChange={this.validateStatusChange}
                                onUpdateStatusError={this.handleUpdateStatusError}
                                inOnlyViewMode={permitted}
                                grayedOutNodes={grayedOutTasks}
                                onOutsourceTabOpen={onOutsourceTabOpen}
                                canAssignVendor={canAssignVendor}
                                isVendor={isVendor}
                            />
                        }
                    </WhenPermitted>

                    <WhenPermitted rule="importTasksFromFormula">
                        <ImportTasksFromFormulaWizard
                            open={this.state.importTasksFromFormulaWizardOpen}
                            projectId={projectId}
                            onClose={() => this.toggleImportModal(false)}
                            onImported={this.handleImportCompleted}
                        />
                    </WhenPermitted>

                    <ConditionalTaskModal
                        ref={(c) => { this.conditionalTaskModal = c; }}
                    />
                    <RatingModal
                        ref={(c) => { this.ratingTaskModal = c; }}
                    />
                </div>
            );
        } else {
            return (
                <div>
                    <ProjectTasksHeader
                        projectId={projectId}
                        path={match.path}
                        params={match.params}
                        name={projectName}
                        taskCount={filteredTasks.length}
                        overdueCount={overdueCount}
                        onAddCard={this.handleAddCard}
                        onImportFormula={this.handleImportFromFormulaClick}
                        onShowResources={this.props.onShowResources}
                        onGoToListView={this.goToListView}
                    />

                    <InfiniteScroll
                        dataLength={tasks.length}
                        next={this.loadNextProjects}
                        hasMore={hasMoreProjects}
                        height={600}
                        loader={<h4>Loading...</h4>}
                        endMessage={
                            <p className="end-of-scroll">
                                <b>No more projects</b>
                            </p>
                        }
                    >
                        {projectIds.map((projectId, index) => {
                            if (filteredTasks.filter(t => t.projectId === projectId).length !== 0) {
                                return <div key={index}>
                                    <Segment clearing basic>
                                        <div className="iauto-task-header">
                                            <ProjectHeader
                                                name={this.getProjectName(projectId)}
                                                tasks={filteredTasks
                                                    .filter(t => t.projectId === projectId).length}
                                                overdue={filteredTasks
                                                    .filter(t => t.isOverdue
                                                        && t.projectId === projectId).length}
                                                className="iauto-task-header__left"
                                                floated="left"
                                            />
                                            {index !== 0 && <Divider />}
                                        </div>
                                    </Segment>

                                    <WhenPermitted rule="projectTaskEditorModalViewNotEdit">
                                        {(permitted) =>
                                            <KanbanBoard
                                                blocks={filteredTasks.filter(t => t.projectId === projectId)}
                                                onDoCard={this.handleDoCard}
                                                onDoVendorCard={this.handleDoVendorCard}
                                                onReviewCard={this.handleReviewCard}
                                                onEditCard={this.handleEditCard}
                                                onDeleteCard={this.props.deleteTask}
                                                onCardStatusUpdate={this.handleCardStatusUpdate}
                                                onChangeProcessingUser={onChangeProcessingUser}
                                                onStopOutsource={onStopOutsource}
                                                validateStatusChange={this.validateStatusChange}
                                                onUpdateStatusError={this.handleUpdateStatusError}
                                                inOnlyViewMode={permitted}
                                                grayedOutNodes={grayedOutTasks}
                                                onOutsourceTabOpen={onOutsourceTabOpen}
                                                canAssignVendor={canAssignVendor}
                                            />
                                        }
                                    </WhenPermitted>
                                </div>;
                            }
                        })
                        }
                    </InfiniteScroll>

                    <ConditionalTaskModal
                        ref={(c) => { this.conditionalTaskModal = c; }}
                    />
                    <RatingModal
                        ref={(c) => { this.ratingTaskModal = c; }}
                    />
                </div>
            );
        }
    }
}
