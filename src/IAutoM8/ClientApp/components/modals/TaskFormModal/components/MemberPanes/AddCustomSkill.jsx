import React, { Component, Fragment } from "react";
import { connect } from "react-redux";
import { reset } from "redux-form";
import { success, error } from "react-notification-system-redux";

import { loadSkills } from "@store/skill";

import { filterSkillsByQuery } from "@selectors/skill";



import { addCustomSkill } from "@store/skill";
import { loadUsers } from "@store/users";

import { ModusButton, SkillFormModal } from "@components";

@connect(
    (state) => ({
        users: state.users.users,
        skills: filterSkillsByQuery(state),
    }),
    {
        loadUsers,
        addCustomSkill,
        error,
        reset,
        success,
        loadSkills
    }
)
export default class AddCustomSkill extends Component {

    constructor(props) {
        super(props);

        this.state = {
            loading: false,
            skills: []
        }
    }

    componentDidMount() {
        this.props.loadSkills();
    }
    

    handleAddCustomSkill = (event) => {
        event.preventDefault();
        // Fix: Added to prevent opening the pop-up in pressing the enter button
        if (event.detail !== 0) {
            this.props.loadUsers();

            this.skillFormModal
                .show({})
                .then((skill) => {
                    if (this.props.assignedTeamId !== "undefined") {
                        skill.teamId = this.props.assignedTeamId;
                    }
                    skill.type = 2; // Type = 2 for Custom Skills
                    this.props
                        .addCustomSkill(skill)
                        .then((res) => {
                            this.props.success({
                                title: "Skill was added successfully",
                            });
                            this.props.onAddCustomSkill(res.value.data.id);
                        })
                        .catch(() =>
                            this.props.error({ title: "Cannot add new skill" })
                        );
                    this.props.reset("skillFormModal");
                })
                .catch(() => {
                    this.props.reset("skillFormModal");
                });
        }
    };

    render() {
        const { loading, skills } = this.state;
        return (
            <Fragment>
                <div className="add-custom-skill-btn">
                    <ModusButton
                        disabled={this.props.isDisabled}
                        content={this.props.isAdmin ? "Add Skill": "Add Custom Skill"}
                        onClick={(event) => this.handleAddCustomSkill(event)}
                    />
                    <SkillFormModal
                        ref={(c) => {
                            this.skillFormModal = c;
                        }}
                        users={this.props.users}
                        loading={loading}
                        skills={this.props.skills}
                        isSkillContainer={false}
                        isAdmin={this.props.isAdmin}
                    />
                </div>
            </Fragment>
        );
    }
}
