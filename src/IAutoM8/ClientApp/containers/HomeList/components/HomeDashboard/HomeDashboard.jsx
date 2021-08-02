import React from 'react';
import { connect } from 'react-redux';
import { v4 as uuidv4 } from 'uuid';
import autobind from 'autobind-decorator';
import axios from 'axios';
import { Radio } from 'semantic-ui-react';

import { getAuthHeaders } from '@infrastructure/auth';

import {
    projectTaskFilter,
    loadUserTasksInStatus,
    updateNewProcessingUser,
    selectProjectTaskUser,
    selectProjectTaskVendorUser,
    selectAllProjectTaskUsers,
    unSelectAllProjectTaskUsers,
    filterTasksByUserType,
    doTask
} from '@store/projectTasks';
import { loadProfileById } from '@store/profile';
import { loadUsers } from "@store/users";

import {
    userTaskSort,
    selectTaskByStatus,
    getUsersForChecklist,
    getVendorUsersForChecklist
} from '@selectors/projectTasks';

import {
    UserProfileModal,
    CheckList,
    ProcessingUserModal
} from '@components';

import Stage from '../Stage';
//import TaskItem from '../TaskItem';

import StatusFilterButtonGroup from './StatusFilterButtonGroup';

class HomeDashboard extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            userProfile: {},
            filterUserId: null,
            filterOptions: null,
            selectedUserIds: [],
            userProfileModalOpen: false,
            disabled : false
        };
    }

    componentDidMount() {
        this.props.loadUserTasksInStatus();

    }

    getUserChecklistOptions() {
        const options = this.props.projectTaskUsers.map((t) => ({
            value: t.id,
            text: t.fullName,
        }));

        return [...options];
    }

    getUserIdValues() {
        const values = this.props.projectTaskUsers.map((u) => u.id);

        return [...values];
    }

    handleCheckboxChange = (userId) => {
        // console.log("userId", userId);
        this.props.selectProjectTaskUser(userId);
    };

    handleVendorCheckboxChange = (userId) => {
        // console.log('handleVendorCheckboxChange', userId);
        this.props.selectProjectTaskVendorUser(userId);
    };

    handleCheckAll = () => {
        this.props.selectAllProjectTaskUsers();
    };

    handleUncheckAll = () => {
        this.props.unSelectAllProjectTaskUsers();
    };

    closeUserInfoModal = () => {
        this.setState({
            userProfileModalOpen: false,
        });
    };

    handleOpenUserProfile = (userId) => {
        this.props.loadProfileById(userId).then(() => {
            this.setState({
                userProfile: this.props.profile.userProfile,
                userProfileModalOpen: true,
            });
        });
    };

    @autobind
    async handleChangeProccessingUser(taskId) {
        await Promise.all([this.props.loadUsers()]);
        this.props.loadUsers();

        const response = await axios.get(
            `/api/tasks/getTaskInStatusById/${taskId}`,
            getAuthHeaders()
        );
        const task = await response.data;

        //const task = this.props.getTaskInStatusById(taskId);
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
                    .updateNewProcessingUser(processingUser)
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

                this.props.reset("processingUserModal");
            })
            .catch(() => {
                this.props.reset("processingUserModal");
            });
    }

    handleChangeFilter = (clickedButton) => {
        this.props.projectTaskFilter(clickedButton);
    };

    handleUsersTypeToggle = () => {
        this.props.filterTasksByUserType(this.props.outsourcerSelected)
        this.props.selectAllProjectTaskUsers();
        this.handleVendorCheckboxChange()
    };
    handleDoCard = (taskId)=>{
        if (this.state.disabled) {
            return;
        }
         this.props.doTask(taskId)
        .then(()=>{
            this.setState({disabled: true});
            this.props.loadUserTasksInStatus();
            this.setState({disabled: false});
        })
    }
    handleReviewCard = (taskId) => {
        //console.log('handleReviewCard',taskId);
        //this.props.reviewTask(taskId);
        if (this.state.disabled) {
            return;
        }
         this.props.reviewTask(taskId)
        .then(()=>{
            this.setState({disabled: true});
            this.props.loadUserTasksInStatus();
            this.setState({disabled: false});
        })
    }

    render() {
        const {
            users,
            loading,
            projects,
            userTasks,
            userOptions,
            vendorUserOptions,
            selectedUsers,
            selectedVendorUsers,
            filterByStatusModel,
            onGoToProject,
            onEditTask,
            onOpenComments,
            outsourcerSelected
        } = this.props;

        //console.log('userOptions', userOptions);
        // console.log("vendorUserOptions", vendorUserOptions);
        // console.log('selectedUsers', selectedUsers);


        //if (this.props.selectedVendorUsers != undefined) {
        //    for (const [index, value] of this.props.selectedVendorUsers.entries()) {

        //        if (!this.props.selectedUsers.includes(value)) {
        //            this.props.selectedUsers.push(value);
        //        }
        //    }
        //    console.log("selectedUsers222", this.props.selectedUsers);
        //    //   selectedUsers.push(options);
        //}



        
        // const userFilterLabel = "USER:";


        return (
            <React.Fragment>
                {this.props.userOptions && (
                    <div className="modus-items-filter">
                        {/* {userFilterLabel && (
                            <label>
                                <strong>{userFilterLabel}</strong>
                            </label>
                        )} */}
                        <div>
                            <span
                                style={{
                                    paddingRight: "25px",
                                    fontWeight: 800,
                                    fontSize: "16px",
                                }}
                            >
                                In-House
                            </span>
                            <Radio
                                toggle
                                checked={outsourcerSelected}
                                onClick={this.handleUsersTypeToggle}
                            />
                            <span
                                style={{
                                    paddingLeft: "25px",
                                    fontWeight: 800,
                                    fontSize: "16px",
                                }}
                            >
                                Outsource
                            </span>
                        </div>
                        {!outsourcerSelected && (<CheckList
                            header="All"
                            options={userOptions}
                            values={selectedUsers}
                            onChange={this.handleCheckboxChange}
                            onCheckAll={this.handleCheckAll}
                            onUncheckAll={this.handleUncheckAll}
                        />)}

                        {outsourcerSelected && (<CheckList
                            header="All"
                            options={vendorUserOptions}
                            values={selectedVendorUsers}
                            onChange={this.handleVendorCheckboxChange}
                            onCheckAll={this.handleCheckAll}
                            onUncheckAll={this.handleUncheckAll}
                        />)}
                        {/* <Dropdown
                            multiple
                            selection
                            options={userOptions}
                            value={selectedUsers}
                        /> */}
                    </div>
                )}

                <div className="container">
                    <StatusFilterButtonGroup
                        style={{ marginTop: "10px" }}
                        filterButtonModel={filterByStatusModel}
                        onChangeFilter={this.handleChangeFilter}
                    />

                    <div className="taskTodoList">
                        <Stage
                            tasks={userTasks}
                            projects={projects}
                            onEditTask={onEditTask}
                            onOpenComments={onOpenComments}
                            //handleDoCard={this.props.doTask} //original
                            handleDoCard ={this.handleDoCard} //newly added
                            onReviewCard={this.handleReviewCard}
                            onGoToProject={onGoToProject}
                            onOpenUserProfile={this.handleOpenUserProfile}
                            onChangeProccessingUser={
                                this.handleChangeProccessingUser
                            }
                            isVendor={false}
                            loading={loading}
                        />
                    </div>

                    <UserProfileModal
                        open={this.state.userProfileModalOpen}
                        userProfile={this.state.userProfile}
                        onClose={this.closeUserInfoModal}
                    />

                    <ProcessingUserModal
                        ref={(c) => {
                            this.processingUserModal = c;
                        }}
                        users={users}
                        loading={loading}
                    />
                </div>
            </React.Fragment>
        );
    }
}

export default connect(
    (state) => ({
        ...state.profile,
        loggedInUser: state.auth.user,
        loading: state.projectTasks.loading,
        userTasks: selectTaskByStatus(state),
        userOptions: getUsersForChecklist(state),
        vendorUserOptions: getVendorUsersForChecklist(state),
        //selectedUsers: selectProjectTaskUsers(state),
        selectedUsers: state.projectTasks.selectedUsers,
        selectedVendorUsers: state.projectTasks.selectedVendorUsers,
        filterByStatusModel: state.projectTasks.filterByStatusModel,
        outsourcerSelected: state.projectTasks.outsourcerSelected,
    }),
    {
        loadUsers,
        loadProfileById,
        loadUserTasksInStatus,
        projectTaskFilter,
        selectProjectTaskUser,
        selectAllProjectTaskUsers,
        unSelectAllProjectTaskUsers,
        updateNewProcessingUser,
        filterTasksByUserType,
        userTaskSort,
        doTask,
        selectProjectTaskVendorUser
    }
)(HomeDashboard);