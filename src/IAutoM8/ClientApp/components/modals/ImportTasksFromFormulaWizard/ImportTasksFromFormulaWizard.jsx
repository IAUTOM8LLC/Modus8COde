import React, { Component } from "react";
import { connect } from "react-redux";
import { error, success } from "react-notification-system-redux";
import moment from "moment";

import {
    SelectProjectStartDateModal,
    AssignSkillsModal,
    SkillFormModal as SkillFormModalWindow,
} from "@components";

import { addSkill } from "@store/skill";
import {
    loadOwnedFormulas,
    loadSkillsForFormula,
    nextPage,
} from "@store/formula";
import { importFromFormula } from "@store/project";
import { addFormulaTask } from "@store/projectTasks";
import { loadTasksWithNestedFormulas } from "@store/formulaTasks";
import { loadUsers } from "@store/users";

import { selectFormulasWithTasks } from "@selectors/formula";
import { selectFormulaProjectSkills } from "@selectors/formulaTasks";

import {
    getUsersForSkill,
    getInHouseUsersForSkill,
    getReviewersForSkill,
} from "@selectors/users";

import { onDeleteOutsorsers as genericOnDeleteOutsorsers } from "@utils/skillOutsourceMapping";
import { addOutsouceItem as genericAddOutsouceItem } from "@utils/skillOutsourceMapping";

import SelectFormulaModal from "./components/SelectFormulaModal";
import SelectTimeModal from "./components/SelectTimeModal";
import QuickStartProjectModal from "./components/QuickStartProjectModal";

const STAGES = {
    SELECT_FORMULA: 'SELECT_FORMULA',
    SELECT_TIME: 'SELECT_TIME',
    SELECT_TIME_COMPLEX: 'SELECT_TIME_COMPLEX',
    ASSIGN_SKILLS: 'ASSIGN_SKILLS',
    QUICK_START_FORMULA: 'QUICK_START_FORMULA',
};

const initialState = {
    formulaId: null,
    formulaName: '',
    dates: {},
    mappings: [],
    stage: STAGES.SELECT_FORMULA,
    submitting: false,
    userOptions: {},
    skillMaps: {},
    formulaTaskMaps: {},
    inHouseUserOptions: {},
    reviewingUserOptions: {},
    outsourceUserOptions: [],
};

@connect(
    (state, props) => ({
        loading: state.formulaTasks.loading
            || state.formula.loading
            || state.users.loading,
        // formulas: selectFormulasWithTasks(state, props),
        ownedFormulas: state.formula.ownedFormulas,
        skillMaps: { skillMaps: state.formula.formulaSkills },
        formulaTaskMaps: { formulaTaskMaps: state.formula.formulaTasks },
        userOptions: getUsersForSkill(state, props),
        inHouseUserOptions: getInHouseUsersForSkill(state, props),
        reviewingUserOptions: getReviewersForSkill(state, props),
        outsourceUsers: state.formula.outsourceUsers,
        skills: state.skill.skills,
        users: state.users.users,
    }),
    {
        loadOwnedFormulas,
        nextPage,
        addFormulaTask,
        importFromFormula,
        loadTasksWithNestedFormulas,
        loadSkillsForFormula,
        error, success,
        addSkill,
        loadUsers,
    }
)
export default class ImportTasksFromFormulaWizard extends Component {
    state = initialState;

    componentDidMount() {
        this.props.loadOwnedFormulas();
    }

    componentWillUnmount() {
        this.props.nextPage();
    }

    componentWillReceiveProps(nextProps) {
        this.setState({
            userOptions: nextProps.userOptions,
            inHouseUserOptions: nextProps.inHouseUserOptions,
            reviewingUserOptions: nextProps.reviewingUserOptions,
            outsourceUserOptions: nextProps.outsourceUsers,
            skillMaps: nextProps.skillMaps,
            formulaTaskMaps: nextProps.formulaTaskMaps,
        });
    }

    submitImportFormula(formulaId, mappings, dates) {
        const { projectId, onImported } = this.props;

        if (this.state.submitting) {
            return;
        }

        this.setState({ submitting: true });
        return this.props
            .importFromFormula({
                projectId,
                formulaId,
                dates,
                mappings: mappings.map(t => {
                    let newMap = {};
                    newMap.skillId = t.skillId;
                    newMap.reviewingSkillId = t.reviewingSkillId;
                    newMap.reviewingUserIds = t.reviewingUserIds;
                    newMap.userIds = t.userIds.filter(x => x !== "outsource");
                    //newMap.isOutsorced = t.userIds.includes("outsource");
                    newMap.isOutsorced = t.canBeOutsourced;
                    newMap.formulaTaskId = t.formulaTaskId;
                    newMap.outsourceUserIds = t.outsourceUserIds;
                    newMap.isDisabled = t.isDisabled;

                    return newMap;
                })
            })
            .then(() => this.setState({ ...initialState }))
            .catch(() => this.props.error({ title: 'Cannot import tasks' }))
            .finally(() => onImported());
    }

    submitAddFormula = (formulaId, skillMappings, startDate) => {
        const { projectId, position, onImported } = this.props;

        if (this.state.submitting) {
            return;
        }

        this.setState({ submitting: true });
        return this.props
            .addFormulaTask({
                posX: Math.floor(position[0]),
                posY: Math.floor(position[1]),
                startDate,
                projectId,
                formulaId,
                skillMappings: skillMappings.map(t => {
                    let newMap = {};
                    newMap.skillId = t.skillId;
                    // newMap.userIds = t.userIds.filter(x => x !== "outsource");
                    // newMap.isOutsorced = t.userIds.includes("outsource");
                    newMap.skillId = t.skillId;
                    newMap.reviewingSkillId = t.reviewingSkillId;
                    newMap.reviewingUserIds = t.reviewingUserIds;
                    newMap.userIds = t.userIds.filter(x => x !== "outsource");
                    //newMap.isOutsorced = t.userIds.includes("outsource");
                    newMap.isOutsorced = t.canBeOutsourced;
                    newMap.formulaTaskId = t.formulaTaskId;
                    newMap.outsourceUserIds = t.outsourceUserIds;
                    newMap.isDisabled = t.isDisabled;

                    return newMap;
                })
            })
            .then(() => this.setState({ ...initialState }))
            .catch(() => this.props.error({ title: 'Cannot add formula' }))
            .finally(() => onImported());
    }

    /**
     * STAGES.SELECT_FORMULA
     */

    handleFormulaSelect = () => {
        const { formulaId } = this.state;
        if (!formulaId || formulaId < 1)
            return;

        return this.props.loadTasksWithNestedFormulas(formulaId)
            .then(this.props.loadSkillsForFormula(this.state.formulaId))
            .then(() => this.setState({ stage: STAGES.ASSIGN_SKILLS }));
    }

    handleFormulaChange = (e, data) => {
        const formulaId = data.value;
        this.setState({
            formulaId: data.value,
            formulaName: this.props.ownedFormulas.find(f => f.id === formulaId).name
        });
    }

    /**
     * STAGES.ASSIGN_SKILLS
     */

    handleSkillMappingsChange = (maps) => {
        // if (maps.skillMaps.some(t => t.userIds.length === 0)) {
        //     this.props.error({ title: 'Each skill should have selected users or be outsourced' });
        //     return;
        // }
        // this.setState({
        //     mappings: maps.skillMaps,
        //     stage: STAGES.QUICK_START_FORMULA
        // });

        if (maps.formulaTaskMaps
            .some(t => t.userIds.length === 0 && t.outsourceUserIds.length === 0 && !t.isDisabled)) {
            this.props.error({ title: 'Each skill should have selected users or be outsourced' });
            return;
        }

        // this.setState({
        //     mappings: maps.formulaTaskMaps,
        //     stage: STAGES.QUICK_START_FORMULA
        // });
        // following code added w.r.t Sprint-10A feedback
        const now = moment().format('MMM D, YYYY h:mm A');
        this.submitImportFormula(
            this.state.formulaId,
            maps.formulaTaskMaps,
            {
                projectStartDateTime: now,
                rootStartDateTime: []
            });
    }

    backToSelectFormula = () => { this.setState({ stage: STAGES.SELECT_FORMULA }); }

    /**
     * STAGES.SELECT_TIME_COMPLEX
     */

    handleComplexDateSelect = (dates) => {
        const { formulaId, mappings } = this.state;
        return this.submitImportFormula(formulaId, mappings, dates)
    }

    /**
     * STAGES.SELECT_TIME
     */

    handleDateSelect = (values) => {
        const { formulaId, mappings } = this.state;
        return this.submitAddFormula(
            formulaId,
            mappings,
            values.projectStartDateTime);
    }

    backToQuickStartModal = () => { this.setState({ stage: STAGES.QUICK_START_FORMULA }); }

    /**
     * STAGES.QUICK_START_FORMULA
     */

    handleQuickStartProject = () => {
        const { formulaId, mappings } = this.state;
        const now = moment().format('MMM D, YYYY h:mm A');

        return this.props.simple
            ? this.submitAddFormula(
                formulaId,
                mappings,
                now)
            : this.submitImportFormula(
                formulaId,
                mappings,
                {
                    projectStartDateTime: now,
                    rootStartDateTime: []
                });
    }

    handleSkipQuickStartProject = () => {
        this.setState({ stage: this.props.simple ? STAGES.SELECT_TIME : STAGES.SELECT_TIME_COMPLEX })
    }

    backToAssignSkills = () => { this.setState({ stage: STAGES.ASSIGN_SKILLS }); }

    handleClose = () => {
        this.setState({ ...initialState });
        this.props.onClose();
    }

    addOutsouceItem = (skillId) => {
        const newMapps = genericAddOutsouceItem(this.state.userOptions,
            this.state.skillMaps.skillMaps, skillId);
        this.setState({ ...newMapps });
    }

    onDeleteOutsorsers = (skillId, userIds) => {
        const newMapps = genericOnDeleteOutsorsers(this.state.userOptions,
            this.state.skillMaps.skillMaps, skillId, userIds);

        this.setState({ ...newMapps});
    }

    render() {
        const {
            open,
            ownedFormulas,
            loading,
            users
        } = this.props;

        const { submitting, formulaId } = this.state;

        if (!open)
            return null;

        switch (this.state.stage) {
            case STAGES.SELECT_FORMULA:
                return <SelectFormulaModal
                    open={open}
                    formulas={ownedFormulas}
                    formulaId={formulaId}
                    onClose={this.handleClose}
                    onSelect={this.handleFormulaSelect}
                    onFormulaChange={this.handleFormulaChange}
                />

            case STAGES.ASSIGN_SKILLS:
                return <div>
                    <AssignSkillsModal
                        initialValues={this.state.formulaTaskMaps}
                        //usersOptions={this.state.userOptions}
                        inHouseUserOptions={this.state.inHouseUserOptions}
                        reviewingUserOptions={this.state.reviewingUserOptions}
                        outsourceUserOptions={this.state.outsourceUserOptions}
                        loading={loading || submitting}
                        addOutsouceItem={this.addOutsouceItem}
                        onDeleteOutsorsers={this.onDeleteOutsorsers}
                        submitLabel="Start"
                        modalHeader="Select start dates for tasks"
                        open={open}
                        formulaId={formulaId}
                        onBack={this.backToSelectFormula}
                        onClose={this.handleClose}
                        onSubmit={this.handleSkillMappingsChange}
                    />
                    <SkillFormModalWindow
                        ref={(c) => { this.skillFormModal = c; }}
                        users={users}
                        loading={loading}
                    />
                </div>

            case STAGES.SELECT_TIME:
                return <SelectTimeModal
                    header="Select formula start time"
                    open={open}
                    loading={loading || submitting}
                    onClose={this.handleClose}
                    onBack={this.backToQuickStartModal}
                    onSubmit={this.handleDateSelect}
                />

            case STAGES.SELECT_TIME_COMPLEX:
                return <SelectProjectStartDateModal
                    modalHeader="Select start dates for tasks"
                    open={open}
                    loading={loading || submitting}
                    onClose={this.handleClose}
                    onBack={this.backToQuickStartModal}
                    onSubmit={this.handleComplexDateSelect}
                />

            case STAGES.QUICK_START_FORMULA:
                return <QuickStartProjectModal
                    open={open}
                    loading={loading || submitting}
                    onClose={this.handleClose}
                    onBack={this.backToAssignSkills}
                    onSkip={this.handleSkipQuickStartProject}
                    onSubmit={this.handleQuickStartProject}
                />

            default:
                return null;
        }
    }
}
