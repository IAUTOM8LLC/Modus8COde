import { createSelector } from 'reselect'

import { ALL_PROJECTS_ID } from '@constants/projectCostants'

import { getFilterSkillId } from '@selectors/skill'
import { getProjectId } from '@selectors/project'
import { getFilterUserId, getUsers } from '@selectors/users'
import { getConditionalUnselectedTasks } from '@utils/task'

const getProjectTaskId = (state) => Number(state.pendingTask.pendingTask.id) || 0;

const getUserProjectTasks = (state) => state.projectTasks.userTasks;

const getProjectTaskUsers = (state) => state.projectTasks.projectTaskUsers;

const getProjectVendorUsers = (state) => state.projectTasks.projectVendorUsers;

const getSelectedTaskUsers = (state) => state.projectTasks.selectedUsers;

const getSelectedVendorTaskUsers = (state) => state.projectTasks;

const getSelectedStatusFilter = (state) => state.projectTasks.filterByStatusModel.selectedFilter;

const getUserTypeFilter = (state) => state.projectTasks.outsourcerSelected;

export const getProjectTasks = (state) => state.projectTasks.tasks;

export const getUsersForChecklist = createSelector(
    [getProjectTaskUsers], 
    (projectTaskUsers = []) => projectTaskUsers.filter(t => t.roles.toString() !== 'Vendor') //console.log('projectTaskUsers',projectTaskUsers)
        .map((u) => ({
            value: u.userId,
            text: u.fullName,
        }))
)

export const getVendorUsersForChecklist = createSelector(   
    [getProjectVendorUsers], 
    (projectVendorUsers = []) => projectVendorUsers.filter(t => t.roles.toString() === 'Vendor')
        .map((u) => ({
            value: u.userId,
            text: u.fullName,
        }))
)

export const selectTasksForOptions = createSelector(
    [getProjectTaskId, getProjectTasks],
    (id, projectTasks) => projectTasks.filter(t => t.id !== id) || []
);

export const selectOverdueTaskCount = createSelector(
    [getProjectTasks],
    (projectTasks = []) => projectTasks.filter(t => t.isOverdue).length
);

export const selectTasksBySkillOrUserAndProject = createSelector(
    [getProjectTasks, getProjectId, getFilterUserId],
    (projectTasks = [], projectId, filterUserId) => {
        if (projectId === ALL_PROJECTS_ID) {
            return projectTasks.filter(t =>
                !filterUserId ||
                (t.assignedUserIds.includes(filterUserId) || t.reviewingUserIds.includes(filterUserId)))
        }
        return projectTasks.filter(t =>
            t.projectId === projectId && !filterUserId ||
            ((t.assignedUserIds && t.assignedUserIds.includes(filterUserId))
                || (t.reviewingUserIds && t.reviewingUserIds.includes(filterUserId))));
    }
);

export const selectGrayedTasks = createSelector(
    [getProjectTasks],
    tasks => getConditionalUnselectedTasks(tasks)
);

export const selectTaskByStatus = createSelector(
    [
        getSelectedStatusFilter,
        getSelectedTaskUsers,
        getUserProjectTasks,
        getUserTypeFilter,
        getSelectedVendorTaskUsers,
    ],

    (
        selectedFilter = "ALL",
        selectedUsers = [],
        //selectedVendorUsers =[],
        userTasks,
        outsourcerSelected,
        test

    ) => {
        // console.log('getSelectedVendorTaskUsers',test.selectedVendorUsers);
        let filteredTasks = userTasks;
        let filteredTaskByUserType = userTasks;
        
        if (outsourcerSelected) {
            filteredTaskByUserType = userTasks.filter(
                (t) => t.processingUserRole === "Vendor"
            );
        } else {
            filteredTaskByUserType = userTasks.filter(
                (t) => t.processingUserRole !== "Vendor"
            );
        }

        if (selectedFilter === "ALL") {
            filteredTasks = filteredTaskByUserType.filter(
                (t) => t.status !== ("COMPLETED" || "UPCOMING") 
            );
        } else if (selectedFilter === "UPCOMING") {
            return filteredTaskByUserType.filter(
                (t) => t.status === "UPCOMING"
            );
        } else if (selectedFilter === "TODO") {
            return filteredTaskByUserType.filter((t) => t.status === "TODO" ||
             t.status === "TODO-NOTSTARTED").reverse();

        } else if (selectedFilter === "REVIEW") {
            return filteredTaskByUserType.filter((t) => t.status === "REVIEW" ||
             t.status === "REVIEW-NOTSTARTED").reverse();
        }
        else {
            filteredTasks = filteredTaskByUserType.filter(
                (t) => t.status === selectedFilter
            ).reverse();
            //console.log('filteredTaskscheck',filteredTasks);
        }
        //Get tasks filtered for users and vendors included as well.
        // if (outsourcerSelected && selectedFilter !== "ALL") {
        //     
        // }
        // if (outsourcerSelected && selectedFilter === "ALL" ) {
        //     filteredTasks = filteredTaskByUserType.filter(
        //         (t) => t.processingUserRole === "Vendor"
        //     );
        // }
        //else{
            // console.log('selctuser',selectedUsers)
            //console.log('selectedVendorUsersinselecters', selectedVendorUsers);
            
        if (outsourcerSelected) {
            filteredTasks = filteredTasks.filter(
                (t) =>
                test.selectedVendorUsers.includes(t.proccessingUserId) || (t.status.includes("TODO")
                ||t.status.includes("REVIEW") )
            );
        }
        else{
            filteredTasks = filteredTasks.filter(
                        (t) => //!t.proccessingUserId
                            selectedUsers.includes(t.proccessingUserId) ||
                             (!t.proccessingUserId && t.status !== ("UPCOMING") )
                            // (t.status.includes("TODO")
                            //  ||(t.status.includes("REVIEW")))
                    );}
                    //console.log('filteredTasks',filteredTasks);
        //}
        

        return filteredTasks;
    }
);
