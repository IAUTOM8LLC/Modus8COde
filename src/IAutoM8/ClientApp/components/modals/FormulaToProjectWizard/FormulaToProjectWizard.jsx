import React, { Component } from "react";
import axios from "axios";
import { reset } from "redux-form";
import { connect } from "react-redux";
import { error, success } from "react-notification-system-redux";
import { push } from "react-router-redux";

import {
    FormulaOverviewModal,
    SetProjectNameAndHierarchiesModal,
    SelectProjectStartDateModal,
    AssignSkillsModal,
    SkillFormModal as SkillFormModalWindow,
} from "@components";

// import { selectFormulaProjectSkills } from '@selectors/formulaTasks'

import { addSkill } from "@store/skill";
import { loadSkillsForFormula } from "@store/formula";
import { importFromFormula } from "@store/project";
import { loadTasksWithNestedFormulas } from "@store/formulaTasks";
import { loadSkills } from "@store/skill";
import { loadUsers } from "@store/users";

import {
    getUsersForSkill,
    getInHouseUsersForSkill,
    getReviewersForSkill,
} from "@selectors/users";

import { onDeleteOutsorsers as genericOnDeleteOutsorsers } from "@utils/skillOutsourceMapping";

// import { selectFormulasWithTasks } from '@selectors/formula'
import { getAuthHeaders } from "@infrastructure/auth";

const initialState = {
    submitting: false,
    dates: {},
    projectDetails: { projectName: "", parentProjectId: null },
    //stage: 'setStartDates',
    //stage: 'setProjectNameAndHierarchies',
    stage: "viewFormulaOverview",
    userOptions: {},
    skillMaps: {},
    formulaTaskMaps: {},
    inHouseUserOptions: {},
    outsourceUserOptions: [],
};

@connect(
    (state, props) => ({
        loading:
            state.formulaTasks.loading ||
            state.formula.loading ||
            state.users.loading,
        skillMaps: { skillMaps: state.formula.formulaSkills },
        formulaTaskMaps: { formulaTaskMaps: state.formula.formulaTasks },
        userOptions: getUsersForSkill(state, props),
        inHouseUserOptions: getInHouseUsersForSkill(state, props),
        reviewingUserOptions: getReviewersForSkill(state, props),
        outsourceUsers: state.formula.outsourceUsers,
        formulaTasks: state.formulaTasks.tasks,
        skills: state.skill.skills,
        users: state.users.users,
    }),
    {
        addSkill,
        importFromFormula,
        loadTasksWithNestedFormulas,
        loadSkills,
        error,
        success,
        pushState: push,
        reset,
        loadUsers,
        loadSkillsForFormula,
    }
)
export default class FormulaToProjectWizard extends Component {
    state = initialState;

    componentWillReceiveProps(nextProps) {
        if (
            this.props.formulaId !== nextProps.formulaId &&
            nextProps.formulaId > 0
        ) {
            this.props
                .loadTasksWithNestedFormulas(nextProps.formulaId)
                .then(() =>
                    this.props.loadSkillsForFormula(this.props.formulaId)
                )
                .then(() => this.props.loadSkills())
                .then(() => this.props.loadUsers());
        }

        this.setState({
            userOptions: nextProps.userOptions,
            inHouseUserOptions: nextProps.inHouseUserOptions,
            reviewingUserOptions: nextProps.reviewingUserOptions,
            outsourceUserOptions: nextProps.outsourceUsers,
            skillMaps: nextProps.skillMaps,
            formulaTaskMaps: nextProps.formulaTaskMaps,
        });
    }

    handleViewFormulaOverview = () => {
        this.setState({
            stage: "setProjectNameAndHierarchies",
        });
    };

    handleSetProjectDetails = (projectDetails) => {
        this.setState({
            projectDetails: projectDetails,
            //stage: "setStartDates",
            /** As per Client request, dated: Oct 9 2020 */
            /** Bypassing the select dates window in Run Formula */
            stage: "assignSkills",
        });
    };

    handleSelectStartDates = (dates) => {
        this.setState({
            dates: dates,
            stage: "assignSkills",
        });
    };

    handleCreateProject = (maps) => {
        const {
            // dates,
            projectDetails,
            projectDetails: { radioGroup },
        } = this.state;
        const { formulaId, formulaTasks } = this.props;

        if (this.state.submitting) {
            return;
        }

        this.setState({ submitting: true });

        // if (maps.skillMaps.some(t => t.userIds.length === 0)) {
        //     this.props.error({ title: 'Each skill should have selected users or be outsourced' });
        //     return;
        // }

        if (
            maps.formulaTaskMaps.some(
                (t) => t.userIds.length === 0 && t.outsourceUserIds.length === 0 && !t.isDisabled
            )
        ) {
            this.props.error({
                title: "Each skill should have selected users or be outsourced",
            });
            return;
        }

        /** Commented the code, as per the client request dated: Oct 09, 2020 */
        /** As now the selection option is not needed till further update */
        // const rootDates = dates.rootStartDateTime.reduce((map, obj) => {
        //     const task = formulaTasks.find((t) => t.id === obj.key);
        //     let parsedId = task.id;
        //     if (task.parentTaskId) {
        //         parsedId = Number(task.id.replace(task.parentTaskId, ""));
        //     }
        //     map[parsedId] = obj.value;
        //     return map;
        // }, {});

        const projectName = projectDetails.projectName;
        const parentProjectId =
            projectDetails.childProjectId || projectDetails.parentProjectId;

        const mappings = maps.formulaTaskMaps.map((t) => {
            let newMap = {};
            newMap.skillId = t.skillId;
            newMap.reviewingSkillId = t.reviewingSkillId;
            newMap.reviewingUserIds = t.reviewingUserIds;
            newMap.userIds = t.userIds.filter((x) => x !== "outsource");
            //newMap.isOutsorced = t.userIds.includes("outsource");
            newMap.isOutsorced = t.canBeOutsourced;
            newMap.formulaTaskId = t.formulaTaskId;
            //newMap.outsourceUserIds = t.outsourceUserIds;
            newMap.outsourceUserIds = t.outsourceUserIds
            .filter(item => item !== '00000000-0000-0000-0000-000000000000');//item === '00000000-0000-0000-0000-000000000000'
            newMap.isDisabled = t.isDisabled;

            return newMap;
        });

        return radioGroup && radioGroup === "existingProjectRadio"
            ? axios
                  .put(
                      `/api/projects/${parentProjectId}/import-tasks`,
                      {
                          formulaId: formulaId,
                          checkedTasks: null,
                          projectStartDates: {
                              //projectStartDateTime: dates.projectStartDateTime,
                              projectStartDateTime: null,
                              //rootStartDateTime: rootDates,
                              rootStartDateTime: null,
                          },
                          skillMappings: mappings,
                      },
                      getAuthHeaders()
                  )
                  .then(() => {
                      this.props.pushState(`/projects/${parentProjectId}`);
                  })
                  .catch(() => {
                      this.props.error({ title: "Cannot import tasks" });
                      this.setState({ ...initialState });
                      this.handleClose();
                  })
            : axios
                  .post(
                      `api/formula/${formulaId}/project`,
                      {
                          projectName: projectName,
                          parentProjectId: parentProjectId,
                          projectStartDates: {
                              //projectStartDateTime: dates.projectStartDateTime,
                              projectStartDateTime: null,
                              //rootStartDateTime: rootDates,
                              rootStartDateTime: null,
                          },
                          skillMappings: mappings,
                      },
                      getAuthHeaders()
                  )
                  .then((response) => {
                      this.props.pushState(`/projects/${response.data}`);
                  })
                  .catch(() => {
                      this.props.error({ title: "Cannot create project" });
                      this.setState({ ...initialState });
                      this.handleClose();
                  });
    };

    handleBackToFormulaOverview = () => {
        this.setState({ stage: "viewFormulaOverview" });
    };
    handleBackToProjectDetails = () => {
        this.setState({ stage: "setProjectNameAndHierarchies" });
    };
    handleBackToSetDates = () => {
        this.setState({ stage: "setStartDates" });
    };

    handleClose = () => {
        this.props.reset("viewFormulaOverviewForm");
        this.props.reset("setProjectNameAndHierarchiesForm");
        this.props.reset("selectProjectStartDateForm");
        this.props.reset("assignSkillsForm");
        this.setState({ ...initialState });
        this.props.onClose();
    };

    onDeleteOutsorsers = (skillId, userIds) => {
        const result = false;

        const newMapps = genericOnDeleteOutsorsers(
            this.state.userOptions,
            this.state.skillMaps.skillMaps,
            skillId,
            userIds
        );

        this.setState({ ...newMapps });
        return result;
    };

    render() {
        const { open, loading, formulaId, formulaName } = this.props;

        if (!open) return null;

        if (this.state.stage === "viewFormulaOverview")
            return (
                <FormulaOverviewModal
                    open={open}
                    onClose={this.handleClose}
                    onNext={this.handleViewFormulaOverview}
                    formulaId={formulaId}
                    formulaName={formulaName}
                    modalHeader="Formula Overview"
                    isCreateProject={true}
                />
            );

        if (this.state.stage === "setProjectNameAndHierarchies")
            return (
                <SetProjectNameAndHierarchiesModal
                    open={open}
                    onClose={this.handleClose}
                    onBack={this.handleBackToFormulaOverview}
                    onSubmit={this.handleSetProjectDetails}
                    loading={loading}
                    formulaName={formulaName}
                    modalHeader="Add Formula to Your Project"
                />
            );

        if (this.state.stage === "setStartDates")
            return (
                <SelectProjectStartDateModal
                    open={open}
                    onClose={this.handleClose}
                    onBack={this.handleBackToProjectDetails}
                    onSubmit={this.handleSelectStartDates}
                    loading={loading}
                    formulaId={formulaId}
                    modalHeader="Create project from formula"
                />
            );

        if (this.state.stage === "assignSkills") {
            return (
                <div>
                    <AssignSkillsModal
                        // initialValues={ this.state.skillMaps }
                        initialValues={this.state.formulaTaskMaps}
                        // usersOptions={this.state.userOptions}
                        inHouseUserOptions={this.state.inHouseUserOptions}
                        reviewingUserOptions={this.state.reviewingUserOptions}
                        outsourceUserOptions={this.state.outsourceUserOptions}
                        onDeleteOutsorsers={this.onDeleteOutsorsers}
                        open={open}
                        onClose={this.handleClose}
                        onBack={this.handleBackToProjectDetails}
                        onSubmit={this.handleCreateProject}
                        formulaId={formulaId}
                        submitLabel="Start"
                    />
                    <SkillFormModalWindow
                        ref={(c) => {
                            this.skillFormModal = c;
                        }}
                        users={this.props.users}
                        loading={this.props.loading}
                    />
                </div>
            );
        }
    }
}
