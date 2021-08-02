import React, { Component } from "react";

import { success } from "react-notification-system-redux";

import { connect } from "react-redux";

import { push } from "react-router-redux";

import { getUsers } from "@selectors/users";

import {
    ProjectTasksHeader
} from "@components";

import {
    reviewTask,
    loadTasksInStatus,
    loadOwnerUserTasksInStatus,
} from "@store/projectTasks";

import Profiles from "../Profiles/Profiles";

import HomeDashboard from "./components/HomeDashboard/HomeDashboard";

import VendorDashboard from "./components/VendorDashboard/VendorDashboard";
import "./HomeList.less";


@connect(
    (state) => ({
        users: getUsers(state),
        hasMoreTasks: state.projectTasks.hasMoreTasks,
        projects: state.project.projects,
        roles: state.auth.user.roles,
        userdetail: state.auth.user,
        userTasks: state.projectTasks.userTasks,
    }),
    {
        loadTasksInStatus,
        loadOwnerUserTasksInStatus,
        pushState: push,
        reviewTask,
        success,
    }
)
export default class HomeList extends Component {

    goToBoard = () => {
        this.props.setProjectsToLoad(this.props.projects.map(t => t.id));
        this.props.loadTasksForAllProjects();
    }

    onGoToProject = (projectId) => {
        this.props.setProjectsToLoad(this.props.projects.map((t) => t.id));
        this.props.pushState(`/projects/${projectId}`);
    };

    render() {
        const {
            editTask,
            tasks,
            match,
            projectId,
            roles,
            onOpenComments,
            userTasks
        } = this.props;
        console.log("roles", roles);
        localStorage.setItem("UserInfo", JSON.stringify(this.props.userdetail));
        //const isVendor = roles.includes('Vendor');
        //const isVendor = roles.some(r => ['CompanyWorker', 'Vendor'].includes(r));
        const isCompany = roles.some(r => ['Company'].includes(r));
        const isVendor = roles.some(r => ['CompanyWorker', 'Vendor'].includes(r)); // added logic for new role CompanyWorker and Company WRT Sprint 10B
        const isManagerOrOwnerOrWorker = roles.some(r => ['Manager', 'Owner', 'Worker'].includes(r));
        const isWorker = roles.includes('Worker');

        const tasksCount = isManagerOrOwnerOrWorker ? userTasks.length : tasks.length;
        const overdueTasksCount = isManagerOrOwnerOrWorker
            ? userTasks.filter(t => t.status.toLowerCase() === "overdue").length
            : tasks.filter(t => t.status.toLowerCase() === "tasksoverdue").length;

        return (
            <div>
                {!isVendor && (
                    <ProjectTasksHeader
                        projectId={projectId}
                        path={match.path}
                        params={match.params}
                        name={"To Do's"}
                        taskCount={tasksCount}
                        overdueCount={overdueTasksCount}
                        onAddCard={this.handleAddCard}
                        onImportFormula={this.handleImportFromFormulaClick}
                        onShowResources={this.props.onShowResources}
                        onGoToBoard={this.goToBoard}
                    />
                )}

                {isManagerOrOwnerOrWorker && (
                    <HomeDashboard
                        {...this.props}
                        onOpenComments={onOpenComments}
                        onGoToProject={this.onGoToProject}
                        isWorker={isWorker}
                        onEditTask={editTask}
                    />
                )}
                {isVendor && !isCompany && (
                    <VendorDashboard
                        {...this.props}
                        onGoToProject={this.onGoToProject}
                    />
                )}
                {isCompany && (
                    <Profiles />
                )}
            </div>
        );
    }
}
