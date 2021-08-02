/* eslint-disable max-len */
import React, { Component } from "react";
import { connect } from "react-redux";
import { push } from "react-router-redux";
import { reset } from "redux-form";
import moment from "moment";
import { success, error, info, warning } from "react-notification-system-redux";
import autobind from "autobind-decorator";
import cn from "classnames";

import { Icon, Rating } from "semantic-ui-react";

import { ALL_PROJECTS_ID } from "@constants/projectCostants";

import { hasChildDependencies, canDelete } from "@utils/task";
import { overdueRequest } from "@utils/constants";
// import { loadOwnedFormulas } from "@store/formula";
import {
    loadTasks,
    loadTasksForAllProjects,
    loadTasksInStatus,
    updateTasksForProject,
    addTask,
    editTask,
    deleteTask,
    checkOverdue,
    updateStatus,
    updateStatusList,
    updateProcessingUser,
    setProjectsToLoad,
    stopOutsource,
    completeConditionalTask,
    loadUserTasksInStatus,
    updateTaskTodos,
    loadVendorTasksInStatus
} from "@store/projectTasks";

import {} from "@store/pendingTask";

import { requestRedrawUIEditor } from "@store/layout";
import { loadProjects, loadMostRecentProject } from "@store/project";
import { loadSkills } from "@store/skill";
import { loadUsersForAllProjects, loadUsers } from "@store/users";
import {
    getFormulaTaskVendorNotification,
    getProjectTaskVendorNotification,
    projectTaskNotificationResponse,
    formulaTaskNotificationResponse,
    setPendingVendorNotification,
    loadVednorSnapshotDetail,
} from "@store/vendor";
import {
    setResources,
    updateProjectTaskResource,
    clearResources,
    getAllResources,
} from "@store/resource";
import { loadComments } from "@store/comment";
import {
    clearTaskNotes,
    loadProjectTask,
    loadProjectNotes,
    createProjectNote,
    publishProjectNote,
    removeProjectNote,
    publishProjectTask,
    setPendingTask,
    setProjectOutsourceAssigne,
    saveProjectOutsourceAssigne,
} from "@store/pendingTask";
import { getAllUnread, readComments } from "@store/notification";

import { selectProjectName, getProjectId } from "@selectors/project";
import { getNotificationId } from "@selectors/vendor";
import { getTransferRequestId } from "@selectors/credits";
import {
    selectOverdueTaskCount,
    selectTasksBySkillOrUserAndProject,
} from "@selectors/projectTasks";
import {
    selectWorkerSkills,
    selectReviewingSkills,
    selectUsersAssignedToSkill,
    selectUsersAssignedToReviewingSkill,
} from "@selectors/skill";
import ModusSocket from "@infrastructure/modusSocket";

import { loadCredits, buyCredits, acceptTransferRequest } from "@store/credits";

import {
    Prompt,
    WhenPermitted,
    TaskFormModal,
    ResourceModal,
    BraintreeModal,
    ProcessingUserModal,
    UserAcronym,
    ModusButton,
    TimeAgo,
    VendorPopupModal,
    TimeEstimate,
    RatingModal,
    ConditionalTaskModal,
    CommentsModal
} from "@components";

// Jai Changes/////////
import {
    addProjectTaskChecklist,
    addProjectTaskReviewCheckList,
} from "@store/projectTasks";
////////////////////////
const updateProperties = (target, source) => {
    Object.assign(target, ...source);
    for (const i in source) {
        if (source.hasOwnProperty(i)) {
            target[i] = source[i];
        }
    }
};
@connect(
    (state, props) => ({
        tasks: selectTasksBySkillOrUserAndProject(state, props),
        projects: state.project.projects,
        projectId: getProjectId(state, props),
        projectName: selectProjectName(state, props),
        overdueCount: selectOverdueTaskCount(state),
        workerSkills: selectWorkerSkills(state),
        reviewerSkills: selectReviewingSkills(state),
        resources: state.resource,
        credits: state.credits.credits,
        userPermissions: state.auth.permissions,
        pendingTask: state.pendingTask.pendingTask,
        vendorNotification: state.vendor.vendorNotification,
        vendorNotificationId: getNotificationId(state, props),
        transferRequestId: getTransferRequestId(state, props),
        selectedUserIdsForEditAssignedSkill: selectUsersAssignedToSkill(state),
        selectedUserIdsForEditReviewingSkill: selectUsersAssignedToReviewingSkill(
            state
        ),
        skills: state.skill.skills,
        users: state.users.users,
        outsources: state.pendingTask.outsources,
        todos: state.projectTasks.todos,
        taskStatus: state.pendingTask.pendingTask.status,
        reviewerTodos: state.projectTasks.reviewerTodos,
        workerTodos: state.projectTasks.workerTodos,
        taskNotes: state.pendingTask.taskNotes,
        roles: state.auth.user.roles,
        user:state.auth.user,
        userTasks: state.projectTasks.userTasks,
        usersList: state.projectTasks.projectTaskUsers,
        // ownedFormulas: state.formula.ownedFormulas    
    }),
    {
        reset,
        addTask,
        editTask,
        deleteTask,
        loadTasks,
        updateProcessingUser,
        stopOutsource,
        loadTasksForAllProjects,
        updateTasksForProject,
        checkOverdue,
        loadSkills,
        loadCredits,
        acceptTransferRequest,
        buyCredits,
        loadUsersForAllProjects,
        loadProjects,
        requestRedrawUIEditor,
        setResources,
        updateProjectTaskResource,
        success,
        error,
        info,
        warning,
        loadComments,
        loadMostRecentProject,
        clearResources,
        loadProjectTask,
        setPendingTask,
        updateStatus,
        updateStatusList,
        getAllResources,
        setProjectsToLoad,
        loadUsers,
        loadTasksInStatus,
        getFormulaTaskVendorNotification,
        getProjectTaskVendorNotification,
        projectTaskNotificationResponse,
        formulaTaskNotificationResponse,
        setPendingVendorNotification,
        setProjectOutsourceAssigne,
        saveProjectOutsourceAssigne,
        completeConditionalTask,
        // Jai Changes ////
        addProjectTaskChecklist,
        ///////////////////
        addProjectTaskReviewCheckList,
        push,
        publishProjectTask,
        loadProjectNotes,
        createProjectNote,
        publishProjectNote,
        removeProjectNote,
        clearTaskNotes,
        getAllUnread,
        readComments,
        loadUserTasksInStatus,
        updateTaskTodos,
        loadVendorTasksInStatus,
        loadVednorSnapshotDetail
        // loadOwnedFormulas
    }
)
export default class ProjectTaskLayout extends Component {
    constructor(props) {
        super(props);

        this.state = {
            taskModalOpen: false,
            vendorPopupOpen: false,
            braintreeModalOpen: false,
            commentModalOpen: false,
            pendingTask: {},
            resourceModal: false,
            outsourceTab: false,
            commentTab: false,
            currentCredits: {},
            isPublishClicked: false,
            showCompleteAndPublish: false,
            isEdited:true,
            editable:false,
            commentTaskId: null
        };
        this.openTaskModal = this.toggleModal.bind(this, true);
        this.closeTaskModal = this.toggleModal.bind(this, false);

        this.openBraintreeModal = this.toggleBraintreeModal.bind(this, true);
        this.closeBraintreeModal = this.toggleBraintreeModal.bind(this, false);

        this.openVendorPopup = this.toggleVendorPopup.bind(this, true);
        this.closeVendorPopup = this.toggleVendorPopup.bind(this, false);

        this.openCommentModal = this.toggleCommentModal.bind(this, true);
        this.closeCommentModal = this.toggleCommentModal.bind(this, false);

        this.handleSavePublish = this.handleSavePublish.bind(this);
    }

    componentWillMount() {
        window.scrollTo(0, 0);
    }

    closeTaskModalAndReloadCredits = () => {
        this.closeTaskModal();
        if (this.props.userPermissions.canAssignVendor) {
            this.props.loadCredits().then(() => {
                this.setState({
                    currentCredits: {
                        reservedCredits: this.props.credits.reservedCredits,
                        availableCredits: this.props.credits.availableCredits,
                    },
                });
            });
        }
        // this.props.loadVendorTasksInStatus(); 
        // this.props.loadUserTasksInStatus();
    };

    componentDidMount() {
        
        this.props.loadUserTasksInStatus()
        const {
            isRecentProjectView,
            userPermissions,
            projectId,
            projectTaskNotification,
            formulaTaskNotification,
            transferRequest,
            vendorNotificationId,
            transferRequestId,
            match,
        } = this.props;

        //this.props.loadOwnedFormulas();

        if (projectTaskNotification || formulaTaskNotification) {
            this.handleOpenVendorPopup(vendorNotificationId);
        }

        if (transferRequest) {
            this.handeTransferRequestAccept(transferRequestId);
        }

        if (isRecentProjectView) {
            this.props.loadMostRecentProject();
        } else {
            this.props.loadProjects().then(() => {
                if (this.props.projectId !== ALL_PROJECTS_ID) {
                    this.loadProjectTasks();
                } else if (projectId === ALL_PROJECTS_ID) {
                    if (userPermissions.filterTasksByUser)
                        this.props.loadUsers();
                    this.props.setProjectsToLoad(
                        this.props.projects.map((t) => t.id)
                    );
                    this.loadTasksForAllProjects();
                }
            });
        }
        if (userPermissions.readSkill) {
            this.props.loadSkills();
        }

        if (userPermissions.canAssignVendor) {
            this.props.loadCredits().then(() => {
                this.setState({
                    currentCredits: {
                        reservedCredits: this.props.credits.reservedCredits,
                        availableCredits: this.props.credits.availableCredits,
                    },
                });
            });
        }

        this.overdueIntervalId = setInterval(
            this.props.checkOverdue,
            1000 * 60
        );

        let socket;
        if (projectId === ALL_PROJECTS_ID) {
            socket = this.createSocketForAllProjects();
        } else {
            socket = this.createSocketForProject(projectId);
        }
        if (socket) {
            socket.start();
            this.socket = socket;
        }
        if (match.params.taskId !== undefined) {
            const tabs = {
                outsourceTab: true,
                commentTab: false,
            };
            if (match.params.tab === "info") {
                tabs.outsourceTab = false;
            } else if (match.params.tab === "comment") {
                tabs.outsourceTab = false;
                tabs.commentTab = true;
            }
            this.handleSpecificTabOpen(Number(match.params.taskId), tabs);
        }
    }

    componentDidUpdate(prevProps) {
        if (this.props.projectId !== prevProps.projectId) {
            let socket;
            if (this.props.projectId === ALL_PROJECTS_ID) {
                this.props.setProjectsToLoad(
                    this.props.projects.map((t) => t.Id)
                );
                this.loadTasksForAllProjects();
                if (this.props.userPermissions.filterTasksByUser)
                    this.props.loadUsers();
                socket = this.createSocketForAllProjects(this.props.projects);
            } else {
                this.loadProjectTasks();
                socket = this.createSocketForProject(this.props.projectId);
            }

            if (socket) {
                socket.start();
                this.socket = socket;
            }
        }

        if (
            prevProps.credits &&
            prevProps.credits.availableCredits !==
                this.props.credits.availableCredits
        ) {
            const diff =
                this.props.credits.availableCredits -
                prevProps.credits.availableCredits;

            this.setState({
                currentCredits: {
                    reservedCredits: this.state.currentCredits.reservedCredits,
                    availableCredits:
                        this.state.currentCredits.availableCredits + diff,
                },
            });
        }

        if (
            this.props.projects.length !== prevProps.projects.length &&
            this.props.projectId === ALL_PROJECTS_ID
        ) {
            const socket = this.createSocketForAllProjects();

            if (socket) {
                socket.start();
                this.socket = socket;
            }
        }
    }

    componentWillUnmount() {
        clearInterval(this.overdueIntervalId);
        this.socket && this.socket.stop();
    }

    createSocketForProject = (projectId) => {
        return new ModusSocket({
            path: "task",
            enableLogging: __DEV__,
            events: [
                {
                    name: "taskStatusChanged",
                    action: (taskStatusList, projectId) => {
                        this.props.updateStatusList(taskStatusList);
                        this.loadProjectTasks();
                    },
                },
            ],
            startEvent: { name: "SubscribeToProject", args: [projectId] },
        });
    };

    createSocketForAllProjects = () => {
        if (this.props.projects.length > 0) {
            return new ModusSocket({
                path: "task",
                enableLogging: __DEV__,
                events: [
                    {
                        name: "taskStatusChanged",
                        action: (taskStatusList, projectId) => {
                            this.props.updateStatusList(taskStatusList);
                            this.checkProjectIdAndLoadTasks(projectId);
                        },
                    },
                ],
                startEvent: {
                    name: "SubscribeToProjects",
                    args: this.props.projects.map((t) => t.id),
                },
            });
        }
    };

    onChangeAssignedSkill = (event, data, task) => {
        if (this.props.pendingTask.assignedSkillId != data) {
            updateProperties(this.props.pendingTask, task);
            this.props.pendingTask.assignedSkillId = data;
        }
    };

    onChangeReviewingSkill = (event, data, task) => {
        if (this.props.pendingTask.reviewingSkillId != data) {
            updateProperties(this.props.pendingTask, task);
            this.props.pendingTask.reviewingSkillId = data;
        }
    };

    loadProjectTasks = () => {
        const { projectId, match, requestRedrawUIEditor } = this.props;
        if (projectId) {
            return this.props
                .loadTasks(projectId)
                .then(() => this.props.checkOverdue())
                .then(
                    () =>
                        match.path.includes("editor") && requestRedrawUIEditor()
                );
        }
    };

    loadTasksForAllProjects = () => {
        if (this.props.match.path.includes("listView")) {
            let statusesToLoad = "InProgress";

            if (this.props.userPermissions.canAssignVendor) {
                statusesToLoad += ",New,NeedsReview";
            }
            this.props.loadTasksInStatus(statusesToLoad);
        } else {
            this.props.loadTasksForAllProjects();
        }
    };

    toggleModal = (opened) => {
        if (opened) {
            this.setState({
                taskModalOpen: true,
                resourceModal: this.state.resourceModal,
            });
        } else {
            const {
                push,
                match: {
                    params: { taskId, projectId },
                },
            } = this.props;
            if (taskId !== undefined) {
                push(`/projects/${projectId}`);
            }
            this.props.reset("taskFormModal");
            //window.location.reload();
            this.setState({
                taskModalOpen: false,
                resourceModal: this.state.resourceModal,
                outsourceTab: false,
                commentTab: false,
                showCompleteAndPublish: false,
            });
            this.props.setPendingTask({});
            this.props.clearTaskNotes();
            this.setState({
                isEdited: true
            })
        }
    };

    toggleVendorPopup = (opened) => {
        if (opened) {
            this.setState({
                vendorPopupOpen: true,
            });
        } else {
            this.props.reset("vendorNotificationFormModal");
            this.setState({
                vendorPopupOpen: false,
            });
            this.props.setPendingVendorNotification({});
        }
    };

    toggleBraintreeModal = (opened) => {
        if (opened) {
            this.setState({
                braintreeModalOpen: true,
            });
        } else {
            this.props.reset("braintreeModal");
            this.setState({
                braintreeModalOpen: false,
            });
        }
    };

    onAcceptVendorNotification = () => {
        if (this.props.projectTaskNotification) {
            this.props.vendorNotification.answer = 5;
        } else {
            this.props.vendorNotification.answer = 3;
        }
    };

    onDeclineVendorNotification = () => {
        this.props.vendorNotification.answer = 0;
    };

    handleNewTask = (pendingTask = {}) => {
        this.props.setPendingTask({
            // provide nice defaults
            posX: 125,
            posY: 25,
            status: "New",
            projectId: this.props.projectId,
            isConditional: false,
            isRecurent: false,
            workerSkills: this.props.workerSkills,
            reviewerSkills: this.props.reviewerSkills,
            startDate: moment().add(5, "m").format("MMM D, YYYY h:mm A"),
            duration: 1,
            reviewingSkillId: null,
            isNewTask: true,

            // and override if necessary
            ...pendingTask,
        });
        this.props.clearResources();
        // Jai Changes ///
        this.props.addProjectTaskChecklist([]);
        this.props.addProjectTaskReviewCheckList([]);
        //////////////////
        this.props.loadProjectNotes(-999);
        this.openTaskModal();
    };

    handleEditTask = (id) => {
        const assignedIds = this.props.userTasks.filter(item => item.id === id)
        const assignedIdArray = assignedIds.map(item => item.proccessingUserId) 
        const assignedId = assignedIdArray.toString()

        
        this.openTaskModal();
        this.props
            .loadProjectTask(id)
            .then(({ value }) => {
                this.props.loadComments(id);
                this.props.clearResources();
                this.props.setResources(value.data.resources);                
                this.props.loadProjectNotes(id);
                if (
                    value.data.status === "InProgress" ||
                    value.data.status === "NeedsReview"
                ) {
                    this.setState({
                        showCompleteAndPublish: true,
                    });
                } 
                else {
                    this.setState({
                        showCompleteAndPublish: false,
                    });
                }


                if (this.props.projectId === ALL_PROJECTS_ID && 
                    //assignedId !== '' &&
                    assignedId !== this.props.user.id ) {
                    this.setState({
                        isEdited: false
                    })
                }
                
                // if(assignedId.toString() === this.props.user.id){
                //     console.log('this.props.projectId',this.props.projectId);
                //     console.log('ALL_PROJECTS_ID',ALL_PROJECTS_ID);
                //     this.setState({
                //         isEdited: true,
                //         //editable: true,

                //     })
                // }
                // else {
                //     this.setState({
                //         isEdited: false,
                //         //editable:false
                //     })
                // }
            })
            .catch(() => {
                this.closeTaskModal();
            })
    };

    handleOpenVendorPopup = (notificationId) => {
        this.openVendorPopup();
        if (this.props.projectTaskNotification) {
            this.props
                .getProjectTaskVendorNotification(notificationId)
                .then(({ value }) => {
                    this.props.clearResources();
                    this.props.setResources(value.data.resources);
                })
                .catch(() => {
                    this.closeVendorPopup();
                });
        } else {
            this.props
                .getFormulaTaskVendorNotification(notificationId)
                .then(({ value }) => {
                    this.props.clearResources();
                    this.props.setResources(value.data.resources);
                })
                .catch(() => {
                    this.closeVendorPopup();
                });
        }
    };

    handeTransferRequestAccept = (transferRequestId) => {
        this.props
            .acceptTransferRequest(transferRequestId)
            .then(() =>
                this.props.success({
                    title: "Transfer request was accepted successfully",
                })
            )
            .catch((error) => this.props.error({ title: error.data.message }));
    };

    checkProjectIdAndLoadTasks = (projectId) => {
        if (this.props.projectId === ALL_PROJECTS_ID) {
            if (projectId) {
                return this.props
                    .updateTasksForProject(projectId)
                    .then(() => this.props.checkOverdue());
            }
        } else {
            this.loadProjectTasks();
        }
    };

    // Handle Form Submit & Task Model Update + Complete Task
    handleSubmit = (task) => {
        let workerMilestones = [];
        let reviewerMilestones = [];

        if (this.props.userPermissions.editProjectTask) {
            const performAction = task.id
                ? this.props.editTask
                : this.props.addTask;

            const isChecked =
                this.props.taskStatus === "InProgress"
                    ? "todoIsChecked"
                    : "reviewerIsChecked";
            const checkList = this.props.todos.map((todo) => ({
                id: todo.id,
                [isChecked]: todo[isChecked],
            }));

            task.editCheckLists = checkList;

            reviewerMilestones = this.props.reviewerTodos.map((todo) => {
                return todo.name;
            });

            workerMilestones = this.props.workerTodos.map((todo) => {
                return todo.name;
            });

            if (!task.id) {
                task.addTodoCheckList = {
                    type: 1,
                    milestones: workerMilestones,
                };

                task.addReviewerCheckList = {
                    type: 2,
                    milestones: reviewerMilestones,
                };
            }

            return performAction(task)
                .then((updatedTask) => {
                    this.props.updateProjectTaskResource(
                        updatedTask.action.payload.data.id,
                        this.props.resources
                    );
                    this.props.saveProjectOutsourceAssigne(
                        updatedTask.action.payload.data.id
                    );
                })
                .then(() => {
                    this.closeTaskModal();
                    this.checkProjectIdAndLoadTasks(task.projectId);
                })
                .catch(() =>
                    this.props.error({
                        title: "Error updating the project task.",
                    })
                );
        } else {
            this.props
                .updateProjectTaskResource(task.id, this.props.resources)
                .then(() => {
                    this.closeTaskModal();
                });
        }
    };

    handleCompleteTask = async (task) => {
        const role = this.props.roles.toString()
        let workerMilestones = [];
        let reviewerMilestones = [];
        if (role === 'Vendor' || role === 'CompanyWorker' ) {
            this.props.updateTaskTodos(this.props.todos)
                                // this.setState({
                                //     isPublishedClicked: false,
                                // })
                                let nextStatus = "";

                                switch (task.status) {
                                    case "New":
                                        nextStatus = "InProgress";
                                        break;
                                    case "InProgress":
                                        if (task.reviewingUserIds.length > 0)
                                            nextStatus = "NeedsReview";
                                        else nextStatus = "Completed";
                                        break;
                                    case "NeedsReview":
                                        nextStatus = "Completed";
                                        break;
                                }
            
                                if (task.status === "NeedsReview") nextStatus = "Completed";
            
                                if (task.isConditional) {
                                    if (task.needsReview) {
                                        let rating = null;
                                        if (
                                            nextStatus === "Completed" &&
                                            task.hasAcceptedVendor &&
                                            this.props.userPermissions.canAssignVendor
                                        ) {
                                            rating = await this.ratingTaskModal.show(
                                                task.condition
                                            );
                                        }
            
                                        await completeConditionalTask(
                                            task.id,
                                            0,
                                            nextStatus,
                                            rating
                                        );
                                        this.props.checkProjectIdAndLoadTasks(
                                            task.projectId
                                        );
                                    } else {
                                        const selectedOptionId = await this.conditionalTaskModal.show(
                                            task.condition
                                        );
                                        await completeConditionalTask(
                                            task.id,
                                            selectedOptionId,
                                            status
                                        );
                                        this.props.checkProjectIdAndLoadTasks(
                                            task.projectId
                                        );
                                    }
                                } else {
                                    let rating = null;
                                    if (
                                        task.status === "NeedsReview" &&
                                        nextStatus === "Completed" &&
                                        task.hasAcceptedVendor &&
                                        this.props.userPermissions.canAssignVendor
                                    ) {
                                        rating = await this.ratingTaskModal.show(
                                            task.condition
                                        );
                                    }
                                    await this.props.updateStatus(
                                        task.id,
                                        nextStatus,
                                        rating
                                    );
                                    this.setState({
                                        isPublishClicked: false
                                    });
                                    this.closeTaskModal();
                                    //this.props.loadVendorTasksInStatus();
                                    this.checkProjectIdAndLoadTasks(task.projectId);
                                }
            this.closeTaskModal(); 
            this.props.loadVendorTasksInStatus();
            this.props.loadVednorSnapshotDetail();                                      
        }
        else if (this.props.userPermissions.editProjectTask) {
            const performAction = task.id
                ? this.props.editTask
                : this.props.addTask;

            const isChecked =
                this.props.taskStatus === "InProgress"
                    ? "todoIsChecked"
                    : "reviewerIsChecked";
            const checkList = this.props.todos.map((todo) => ({
                id: todo.id,
                [isChecked]: todo[isChecked],
            }));

            task.editCheckLists = checkList;

            reviewerMilestones = this.props.reviewerTodos.map((todo) => {
                return todo.name;
            });

            workerMilestones = this.props.workerTodos.map((todo) => {
                return todo.name;
            });

            if (!task.id) {
                task.addTodoCheckList = {
                    type: 1,
                    milestones: workerMilestones,
                };

                task.addReviewerCheckList = {
                    type: 2,
                    milestones: reviewerMilestones,
                };
            }

            return performAction(task)
                .then(async (updatedTask) => {
                    this.props.updateProjectTaskResource(
                        updatedTask.action.payload.data.id,
                        this.props.resources
                    );
                    this.props.saveProjectOutsourceAssigne(
                        updatedTask.action.payload.data.id
                    );

                    let nextStatus = "";

                    switch (task.status) {
                        case "New":
                            nextStatus = "InProgress";
                            break;
                        case "InProgress":
                            if (task.reviewingUserIds.length > 0)
                                nextStatus = "NeedsReview";
                            else nextStatus = "Completed";
                            break;
                        case "NeedsReview":
                            nextStatus = "Completed";
                            break;
                    }

                    if (task.status === "NeedsReview") nextStatus = "Completed";

                    if (task.isConditional) {
                        if (task.needsReview) {
                            let rating = null;
                            if (
                                nextStatus === "Completed" &&
                                task.hasAcceptedVendor &&
                                this.props.userPermissions.canAssignVendor
                            ) {
                                rating = await this.ratingTaskModal.show(
                                    task.condition
                                );
                            }

                            await completeConditionalTask(
                                task.id,
                                0,
                                nextStatus,
                                rating
                            );
                            this.props.checkProjectIdAndLoadTasks(
                                task.projectId
                            );
                        } else {
                            const selectedOptionId = await this.conditionalTaskModal.show(
                                task.condition
                            );
                            await completeConditionalTask(
                                task.id,
                                selectedOptionId,
                                status
                            );
                            this.props.checkProjectIdAndLoadTasks(
                                task.projectId
                            );
                        }
                    } else {
                        let rating = null;
                        if (
                            task.status === "NeedsReview" &&
                            nextStatus === "Completed" &&
                            task.hasAcceptedVendor &&
                            this.props.userPermissions.canAssignVendor
                        ) {
                            rating = await this.ratingTaskModal.show(
                                task.condition
                            );
                        }

                        await this.props.updateStatus(
                            task.id,
                            nextStatus,
                            rating
                        );

                        this.setState({
                            isPublishClicked: false
                        });
                        this.closeTaskModal();
                        this.checkProjectIdAndLoadTasks(task.projectId);
                    }
                })
                .then(() => {
                    this.closeTaskModal();
                    this.checkProjectIdAndLoadTasks(task.projectId);
                    this.props.loadUserTasksInStatus();
                })
                .catch((error) => {
                    if (typeof error === 'string') {
                        this.props.error({ title: error });
                    } else if (error.title) {
                        this.props.error(error);
                    } else if (error.status && error.status !== 500) {
                        this.props.error({ title: error.data.message });
                    } else {
                        this.props.error({ title: 'Cannot update task status' });
                    }

                    this.closeTaskModal();
                    this.checkProjectIdAndLoadTasks(task.projectId);
                    this.props.loadUserTasksInStatus();
                });
        } else {
            this.props
                .updateProjectTaskResource(task.id, this.props.resources)
                .then(() => {
                    this.closeTaskModal();
                });
        }

        this.handleSubmit(task).then(() => setTimeout(async () => {}, 2500));
    };

    handleSavePublish = () => {
        this.setState({
            isPublishClicked: true,
        });
    };

    // let workerMilestones = [];
    // let reviewerMilestones = [];

    // if (this.props.userPermissions.editProjectTask) {
    //     const performAction = task.id
    //         ? this.props.editTask
    //         : this.props.addTask;

    //     const isChecked = this.props.taskStatus ===
    // 'InProgress' ? 'todoIsChecked' : 'reviewerIsChecked';
    //     const checkList = this.props.todos.map(todo => ({
    //         id: todo.id,
    //         [isChecked]: todo[isChecked]
    //     }));

    //     task.editCheckLists = checkList;

    //     reviewerMilestones = this.props.reviewerTodos.map(todo => {
    //         return todo.name;
    //     });

    //     workerMilestones = this.props.workerTodos.map(todo => {
    //         return todo.name;
    //     });

    //     if (!task.id) {
    //         task.addTodoCheckList = {
    //             type: 1,
    //             milestones: workerMilestones
    //         };

    //         task.addReviewerCheckList = {
    //             type: 2,
    //             milestones: reviewerMilestones
    //         };
    //     }

    //     return performAction(task)
    //         .then((updatedTask) => {
    //             this.props
    //                 .updateProjectTaskResource
    // (updatedTask.action.payload.data.id, this.props.resources);
    //             this.props
    //                 .saveProjectOutsourceAssigne(updatedTask.action.payload.data.id)
    //         })
    //         .then(() => {
    //             this.closeTaskModal();
    //             this.checkProjectIdAndLoadTasks(task.projectId);
    //         })
    //         .catch((error) => this.props
    //             .error({ title: error != undefined ? error.data.message : "Internal error" }));
    // }
    // else {
    //     this.props.updateProjectTaskResource(task.id, this.props.resources)
    //         .then(() => {
    //             this.closeTaskModal();
    //         });
    // }
    // }

    handleVendorNotificationSubmit = (notification) => {
        if (this.props.projectTaskNotification) {
            return this.props
                .projectTaskNotificationResponse(this.props.vendorNotification)
                .then(() => {
                    // if (this.props.vendorNotification.answer === 5) {
                    //     this.props.loadProjects();
                    //     this.props.loadTasksForAllProjects();
                    // }
                    this.closeVendorPopup();
                    this.props.reset("vendorNotificationFormModal");
                    this.props.history.push('/projects/getTasksFromAllProjects/listView');
                })
                .catch((error) =>
                    this.props.error({ title: error.data.message })
                );
        } else {
            if (
                (notification.answer === 3 && !notification.price) ||
                notification.price < 0
            ) {
                this.props.error({
                    title: "Price is required field and can't be negative",
                });
                return;
            }
            this.props.vendorNotification.price = notification.price;
            this.props.vendorNotification.companyWorkerPrice = notification.companyWorkerPrice;
            return this.props
                .formulaTaskNotificationResponse(this.props.vendorNotification)
                .then(() => {
                    this.closeVendorPopup();
                    this.props.reset("vendorNotificationFormModal");
                    this.props.history.push('/projects/getTasksFromAllProjects/listView');
                })
                .catch((error) =>
                    this.props.error({ title: error.data.message })
                );
        }
    };

    @autobind
    async handleProcessingUser(taskId) {
        await Promise.all([this.props.loadUsers()]);
        this.props.loadUsers();
        const task = this.props.tasks.find((t) => t.id === taskId);
        let skill = this.props.skills.find((t) =>
            t.users.some((user) =>
                task.assignedUserIds.some((uId) => user.userId === uId)
            )
        );
        if (skill === undefined) {
            skill = this.props.skills.find((t) =>
                t.users.some((user) =>
                    task.reviewingUserIds.some((uId) => user.userId === uId)
                )
            );
        }

        this.processingUserModal
            .show({
                initialValues: {
                    taskId,
                    skillName: skill.name,
                    processingUserId:
                        task.status === "InProgress"
                            ? task.proccessingUserId
                            : task.reviewingUserId,
                    assignedUsers: skill.users,
                },
            })
            .then((processingUser) => {
                this.props
                    .updateProcessingUser(processingUser)
                    .then(() => {
                        this.props.success({
                            title: "Processing user was updated successfully",
                        });
                    })
                    .catch(() =>
                        this.props.error({
                            title: "Cannot update processing user",
                        })
                    );

                this.props.reset("skillFormModal");
            })
            .catch(() => {
                this.props.reset("skillFormModal");
            });
    }

    @autobind
    async handleStopOutsource(taskId) {
        this.props.stopOutsource(taskId).then(() => {
            this.props.success({ title: "Outsource is stopped successfully" });
        });
    }

    @autobind
    async handleDeleteTask(taskId, beforeDelete) {
        if (!this.props.userPermissions.deleteProjectTask) {
            this.props.info({
                title: "You do not have permission to delet task",
            });
            return;
        }

        const task = this.props.tasks.find((t) => t.id === Number(taskId));

        const status = canDelete(task, this.props.tasks);
        if (!status.canDelete) {
            this.props.warning({
                title: status.title,
                message: status.message,
            });
            return;
        }

        if (hasChildDependencies(task)) {
            this.props.warning({
                title: "Cannot delete parent task",
                message: `You cannot delete task until it has at least one child.
                        \n Please delete all children and then try again.`,
            });
            return;
        }

        const confirmed = await Prompt.confirm(
            `Do you want to delete task ${task.title}?`,
            "Confirm delete task",
            "tasks"
        );

        if (confirmed) {
            try {
                beforeDelete && beforeDelete();
                this.props
                    .deleteTask(task)
                    .then(() => this.checkProjectIdAndLoadTasks(task.projectId))
                    .then(() =>
                        this.props.success({
                            title: "Task was deleted successfully",
                        })
                    )
                    .catch(() =>
                        this.props.error({ title: "Cannot delete task" })
                    );
            } catch (_) {
                this.props.requestRedrawUIEditor();
            }
        }
    }

    @autobind
    async handleAssignProcessingUser(vendorId, isAssign) {
        const outsources = this.props.pendingTask.outsources;
        let diffBeetwPrevMaxPrice = 0;
        const prices = outsources
            .filter((t) => t.status === "Send")
            .map((t) => t.price)
            .sort((a, b) => b - a);

        if (isAssign) {
            const confirmed = await Prompt.confirm(
                `Do you want to assign vendor for this task?`,
                "If there is some processing user he will be replaced by vendor which accept this request",
                "tasks"
            );

            if (confirmed) {
                const outsourceToAdd = outsources.find(
                    (t) => t.id === vendorId
                );
                const maxPrice = prices.length === 0 ? 0 : prices[0];
                if (outsourceToAdd.price > maxPrice) {
                    diffBeetwPrevMaxPrice = outsourceToAdd.price - maxPrice;
                }

                this.setState({
                    currentCredits: {
                        reservedCredits:
                            this.state.currentCredits.reservedCredits +
                            diffBeetwPrevMaxPrice,
                        availableCredits:
                            this.state.currentCredits.availableCredits -
                            diffBeetwPrevMaxPrice,
                    },
                });

                await this.props.setProjectOutsourceAssigne(vendorId, isAssign);
            }
        } else {
            const outsourceToUnassign = outsources.find(
                (t) => t.id === vendorId
            );

            if (prices.length === 1) {
                diffBeetwPrevMaxPrice = prices[0];
            } else if (
                outsourceToUnassign.price === prices[0] &&
                prices[0] > prices[1]
            ) {
                diffBeetwPrevMaxPrice = prices[0] - prices[1];
            }

            this.setState({
                currentCredits: {
                    reservedCredits:
                        this.state.currentCredits.reservedCredits -
                        diffBeetwPrevMaxPrice,
                    availableCredits:
                        this.state.currentCredits.availableCredits +
                        diffBeetwPrevMaxPrice,
                },
            });

            await this.props.setProjectOutsourceAssigne(vendorId, isAssign);
        }
    }

    handleShowResources = () => {
        this.setState({
            ...this.state,
            resourceModal: true,
        });
        this.props.getAllResources(this.props.projectId);
    };

    handleCloseResources = () => {
        this.setState({
            ...this.state,
            resourceModal: false,
        });
    };

    handleSpecificTabOpen = (id, tabInfo = { outsourceTab: true }) => {
        this.setState({
            ...this.state,
            ...tabInfo,
        });
        this.handleEditTask(id);
    };

    handleOpenBraintreeModal = () => {
        this.openBraintreeModal();
    };

    handleAddNewNotes = (taskNote, sharedTaskIds) => {
        const {
            pendingTask: { id },
            projectId,
            createProjectNote,
        } = this.props;
        //taskNote.projectId = projectId;
        taskNote.projectTaskId = id ? id : -999;

        createProjectNote(taskNote, sharedTaskIds);
    };

    handleDeleteNotes = (noteId) => {
        this.props.removeProjectNote(noteId);
    };

    handlePublishNotes = (noteId, isPublished) => {
        const { success, publishProjectNote } = this.props;

        publishProjectNote(noteId, isPublished).then(() => {
            success({ title: "Task note is published successfully." });
        });
    };

    renderVendor = (vendor, index) => {
        const { outsources } = this.props.pendingTask;
        const {
            currentCredits: { availableCredits },
        } = this.state;
        const sortedPrices = this.props.pendingTask.outsources
            .filter((t) => t.status === "Send")
            .map((t) => t.price)
            .sort((a, b) => b - a);

        const maxPrice = sortedPrices.length > 0 ? sortedPrices[0] : 0;
        const text =
            vendor.status === overdueRequest.send
                ? "Sent on "
                : vendor.status === overdueRequest.accepted
                ? "Confirmed on "
                : vendor.status === overdueRequest.declined
                ? "Declined on "
                : vendor.status === overdueRequest.cancelAfterNudge
                ? "Declined after nudge on "
                : "";

        return (
            <div className="vendor-detail" key={index}>
                <div className="vendor-detail__acronym">
                    <UserAcronym fullname={vendor.fullName} />
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

                    <div className="vendor-detail__price">${vendor.price}</div>
                    {(vendor.status === overdueRequest.none ||
                        vendor.status === overdueRequest.declinedByOwner) &&
                        !outsources.some(
                            (t) => t.status === overdueRequest.accepted
                        ) && (
                            <ModusButton
                                as="a"
                                className="vendor-detail__btn"
                                content="Assign"
                                onClick={() =>
                                    this.handleAssignProcessingUser(
                                        vendor.id,
                                        true
                                    )
                                }
                            />
                        )}
                    {vendor.status === overdueRequest.send && (
                        <ModusButton
                            as="div"
                            className="vendor-detail__btn decline"
                            content={<i />}
                            onClick={() =>
                                this.handleAssignProcessingUser(
                                    vendor.id,
                                    false
                                )
                            }
                            popup="Waiting for confirmation. Do you want to cancel?"
                        />
                    )}
                    {vendor.status === overdueRequest.accepted && (
                        <ModusButton
                            as="div"
                            className="vendor-detail__btn accept"
                            content={<i />}
                        />
                    )}
                    {vendor.status === overdueRequest.declined && (
                        <ModusButton
                            as="div"
                            disabled
                            className="vendor-detail__btn"
                            content="denied"
                        />
                    )}
                    {vendor.status === "Lost" && (
                        <ModusButton
                            as="div"
                            disabled
                            className="vendor-detail__btn"
                            content="Lost"
                        />
                    )}
                </div>

                {vendor.price > maxPrice &&
                    vendor.price - maxPrice > availableCredits &&
                    (vendor.status === overdueRequest.none ||
                        vendor.status === overdueRequest.declinedByOwner) && (
                        <div className="credits-warning-overlap">
                            <div className="credits-info-container">
                                <Icon className="credits-info" />
                                <div className="credits-warning">
                                    Not enough credits
                                </div>
                                <ModusButton
                                    as="div"
                                    filled
                                    className="add-credits-btn"
                                    content="Add credits now"
                                    onClick={this.handleOpenBraintreeModal}
                                />
                            </div>
                        </div>
                    )}
            </div>
        );
    };

    handleBraintreeSubmit = (payment) => {
        this.props
            .buyCredits(payment)
            .then(() => this.closeBraintreeModal())
            .catch((error) => {
                this.props.error({ title: error });
            });
    };

    handleAddComments = (taskId) => {
        this.openCommentModal();
        this.props.readComments(taskId)
            .then(() => {
                this.props.getAllUnread();
                this.props.loadUserTasksInStatus();
                this.props.loadComments(taskId)
                    .then(() => {
                        this.setState({ commentTaskId: taskId });
                    });
            });
    };

    toggleCommentModal = (opened) => {
        if (opened) {
            this.setState({ commentModalOpen: true });
        } else {
            this.setState({ commentModalOpen: false });
        }
    };

    render() {
        const {
            children,
            pendingTask,
            vendorNotification,
            outsources,
            taskNotes,
            userTasks,
            usersList,
            user,
            ...passThroughProps
        } = this.props;  
        const role = this.props.roles.toString()
        const extraProps = {
            ...passThroughProps,

            newTask: this.handleNewTask,
            editTask: this.handleEditTask,
            deleteTask: this.handleDeleteTask,
            openTaskModal: this.openTaskModal,
            closeTaskModal: this.closeTaskModal,
            loadProjectTasks: this.loadProjectTasks,
            checkProjectIdAndLoadTasks: this.checkProjectIdAndLoadTasks,
            onShowResources: this.handleShowResources,
            onChangeProcessingUser: this.handleProcessingUser,
            onStopOutsource: this.handleStopOutsource,
            onOutsourceTabOpen: this.handleSpecificTabOpen,
            canAssignVendor: this.props.userPermissions.canAssignVendor,
            onOpenComments: this.handleAddComments,
        };

        const enchancedChildren = React.cloneElement(children, extraProps);
        return (
            <div>
                {enchancedChildren}

                <WhenPermitted rule="projectTaskEditorModalViewNotEdit">
                    {(permitted) => (
                        <TaskFormModal
                            inOnlyViewMode={permitted}
                            open={this.state.taskModalOpen}
                            initialValues={{
                                ...pendingTask,
                                assignedUserIds: pendingTask.assignedUserIds,
                                reviewingUserIds: pendingTask.reviewingUserIds,
                                outsources,
                            }}
                            onChangeAssignedSkill={this.onChangeAssignedSkill}
                            onChangeReviewingSkill={this.onChangeReviewingSkill}
                            workerSkills={this.props.workerSkills}
                            reviewerSkills={this.props.reviewerSkills}
                            onSubmit={this.handleSubmit}
                            onCompleteTask={this.handleCompleteTask}
                            onClose={this.closeTaskModalAndReloadCredits}
                            onAddNewNotes={this.handleAddNewNotes}
                            onDeleteNotes={this.handleDeleteNotes}
                            onPublishNotes={this.handlePublishNotes}
                            hasMore={false}
                            credits={this.state.currentCredits}
                            items={pendingTask.outsources}
                            node={this.renderVendor}
                            isOutsourceOpenByDefault={this.state.outsourceTab}
                            isCommentOpenByDefault={this.state.commentTab}
                            isFromProject={true}
                            handleSavePublish={this.handleSavePublish}
                            showCompleteAndPublish={
                                this.state.showCompleteAndPublish
                            }
                            isEdited={
                                this.state.isEdited
                            }
                            editable={
                                this.state.editable
                            }
                            role ={role}
                            taskNotes={taskNotes}                            
                            showTrainingTab= {pendingTask.showTrainingTab}
                        />
                    )}
                </WhenPermitted>
                <ResourceModal
                    projectId={this.props.projectId}
                    open={this.state.resourceModal}
                    onClose={this.handleCloseResources}
                />

                <ProcessingUserModal
                    ref={(c) => {
                        this.processingUserModal = c;
                    }}
                    users={this.props.users}
                    loading={this.props.loading}
                />

                <VendorPopupModal
                    onAccept={this.onAcceptVendorNotification}
                    onDecline={this.onDeclineVendorNotification}
                    open={this.state.vendorPopupOpen}
                    onClose={this.closeVendorPopup}
                    onSubmit={this.handleVendorNotificationSubmit}
                    loading={this.props.loading}
                    initialValues={{
                        isProjectTaskNotification: this.props
                            .projectTaskNotification,
                        ...vendorNotification,
                    }}
                />

                <BraintreeModal
                    open={this.state.braintreeModalOpen}
                    onSubmit={this.handleBraintreeSubmit}
                    onClose={this.closeBraintreeModal}
                />

                <ConditionalTaskModal
                    ref={(c) => {
                        this.conditionalTaskModal = c;
                    }}
                />

                <RatingModal
                    ref={(c) => {
                        this.ratingTaskModal = c;
                    }}
                />

                <CommentsModal
                    open={this.state.commentModalOpen}
                    onClose={this.closeCommentModal}
                    taskId={this.state.commentTaskId}
                />
            </div>
        );
    }
}
