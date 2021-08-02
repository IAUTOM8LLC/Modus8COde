import React from "react";
import { connect } from "react-redux";
import { Form } from "semantic-ui-react";

import { addSkill, loadSkills, loadSkillsByTeam, loadReviewerSkills } from "@store/skill";
import { loadDDLTeams } from "@store/team";

import { DropdownInput } from "@components";

import ReviewingSkill from "./ReviewingSkill";
import AddCustomSkill from "./AddCustomSkill";

@connect(
    (state) => ({
        teams: state.team.ddlTeams,
        skills: state.skill.skills,
        revskills: state.skill.revskills,
        loading: state.skill.loading,
        todosExist: checkTodosExist(state),
    }),
    { addSkill, loadSkills, loadDDLTeams, loadSkillsByTeam, loadReviewerSkills }
)
export default class FormulaMembersPane extends React.Component {
    constructor(props) {
        super(props);
        this.handleAddAssignedSkill = this.handleAddItem.bind(
            this,
            "assignedSkillId"
        );
        this.handleAddReviewSkill = this.handleAddItem.bind(
            this,
            "reviewingSkillId"
        );

        this.state = {
            isCustomSkillButtonDisabled: true,
            teamIdValue: 0,
        };
    }

    componentDidMount() {
        this.setState({ teamIdValue: this.props.teamId });
        //this.props.loadSkills();
        this.props.loadDDLTeams();
        this.props.loadReviewerSkills();
        
    }

    componentDidUpdate(prevProps) {
        const { teamId } = this.props;        
        if (teamId && (teamId !== prevProps.teamId)) {
            this.props.loadSkillsByTeam(teamId);
            this.setState({ teamIdValue: teamId });
            this.setState({ isCustomSkillButtonDisabled: false });
        }
    }

    handleAddItem(field, e, { value }) {
        this.props
            .addSkill({ name: value })
            .then(({ value }) => this.props.changeField(field, value.data.id));
    }

    handleAddCustomSkill = (id) => {
        this.props.changeField("assignedSkillId", id);
    };

    handleTeamChange = (teamId) => {
        
        this.setState({ teamIdValue: teamId });
        this.props.loadSkillsByTeam(teamId);
        this.setState({ isCustomSkillButtonDisabled: false });
    }

    render() {

        const options = this.props.skills.map((v) => ({
            key: v.id,
            value: v.id,
            text: v.name,
        }));

        const teams = this.props.teams.map((t) => ({
            key: `teams_${t.id}`,
            value: t.id,
            text: t.name,
        }));

        const reviewingOptions = this.props.revskills
            .filter((t) => !t.isWorkerSkill)
            .map((v) => ({
                key: v.id,
                value: v.id,
                text: v.name,
            }));

        const reviewingProps = {
            allowAdditions: true,
            search: true,
            options: reviewingOptions,
            onAddItem: this.handleAddReviewSkill,
        };

        return (
            <div className="task-details-modal__pane">
                <Form as="section">
                    <DropdownInput
                        search
                        label="Assign Team"
                        name="teamId"
                        options={teams}
                        onChange={(event, teamId) => this.handleTeamChange(teamId)}
                    />

                    <DropdownInput
                        allowAdditions
                        search
                        //label="Select required skill for this task"
                        label="Assign Skill"
                        name="assignedSkillId"
                        options={options}
                        onAddItem={this.handleAddAssignedSkill}
                    />

                    <AddCustomSkill
                        isDisabled={this.state.isCustomSkillButtonDisabled}
                        assignedTeamId={this.state.teamIdValue}
                        onAddCustomSkill={this.handleAddCustomSkill}
                    />

                    <ReviewingSkill
                        canEdit
                        reviewingSkillId={this.props.reviewingSkillId}
                        dropdownProps={reviewingProps}
                        changeField={this.props.changeField}
                        todosExist={this.props.todosExist}
                    />
                </Form>
            </div>
        );
    }
}

const checkTodosExist = (state) => {
    const {
        pendingTask: { pendingTask },
        formulaTasks,
    } = state;

    return formulaTasks.todoTypeTwo && formulaTasks.todoTypeTwo.length
        ? true
        : pendingTask.formulaTaskChecklists &&
            pendingTask.formulaTaskChecklists.filter((todo) => todo.type === 2)
                .length
            ? true
            : false;
};
