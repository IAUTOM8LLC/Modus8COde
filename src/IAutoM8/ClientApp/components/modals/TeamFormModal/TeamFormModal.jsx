import React, { Component } from "react";
import { reduxForm } from "redux-form";
import { Modal, Form } from "semantic-ui-react";

import { required, alphaNumeric, minUsersNumber } from "@utils/validators";

import editModalHoc from "../../common/editModalHoc";

import AddCustomSkill from "../TaskFormModal/components/MemberPanes/AddCustomSkill";

import {
    TextInput,
    DropdownInput,
    WhenPermitted,
    ModusButton,
} from "@components";

import { target } from "../../ChainingEditor/settings";

const validate = (values, props) => {
    const { teamName, teamSkills, id } = values;
    const { teams } = { ...props };

    const isNotDuplicated = () => {
        const errorMessage = `Team ${teamName} already exists`;

        const filteredTeams = teams.filter((t) => t.id !== id);
        const lowercase = teamName.toLowerCase().trim();
        const isTeamNameExists = filteredTeams.some(
            (t) => t.teamName.toLowerCase().trim() === lowercase
        );

        if (!values.hasOwnProperty("id") && isTeamNameExists) {
            return errorMessage;
        }

        if (isTeamNameExists) {
            return errorMessage;
        }

        return undefined;
    };

    return {
        teamName: required(teamName) || isNotDuplicated(),
        teamSkills: minUsersNumber(1)(teamSkills),
    };
};

@editModalHoc
@reduxForm({
    form: "teamFormModal",
    enableReinitialize: true,
    validate,
})
export default class TeamFormModal extends Component {
    render() {
        const {
            open,
            loading,
            onClose,
            skills,
            handleSubmit,
            isAdmin,
            teamIdValue,
            addOnlyTeam,
            // teamsDll,
        } = this.props;

        const options = skills.map((u) => ({
            key: u.id,
            value: u.id,
            text: u.name,
        }));

        // const teamData = teamsDll.map((u) => ({
        //     key: u.id,
        //     value: u.id,
        //     text: u.name,
        // }));

        return (
            <Modal
                as="form"
                open={open}
                className="task-details-modal frm-teams"
                size="small"
                onSubmit={handleSubmit}
                onClose={onClose}
            >
                <Modal.Header>
                    {isAdmin 
                        ? addOnlyTeam 
                            ? "Add Team"
                            : "Edit Team"
                        : "Add or Assign Skill"}
                </Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        {addOnlyTeam && (
                            <TextInput
                                fluid
                                name="teamName"
                                label="Team Name"
                                placeholder="Add team name"
                                validate={[required, alphaNumeric]}
                            />
                        )}

                        {!addOnlyTeam && (
                            <div>
                                {/* <DropdownInput
                                    search
                                    fluid                                    
                                    name="id"
                                    label="Assign Team"
                                    options={teamData}
                                    validate={[required, minUsersNumber(1)]}
                                /> */}

                                <TextInput
                                    fluid
                                    name="teamName"
                                    label="Team Name"
                                    validate={[required, alphaNumeric]}
                                />

                                <DropdownInput
                                    search
                                    fluid
                                    multiple
                                    name="teamSkills"
                                    label="Assign Skills"
                                    options={options}
                                    validate={[required, minUsersNumber(1)]}
                                />

                                <div
                                    style={{
                                        display: "flex",
                                        justifyContent: "flex-end",
                                    }}
                                >
                                    <AddCustomSkill
                                        isAdmin={isAdmin}
                                        assignedTeamId={"undefined"}
                                        onAddCustomSkill={(data) => {
                                            this.props.loadSkillsForNewTeam(0);
                                            if (teamIdValue > 0) {
                                                this.props.loadSkillsForNewTeam(
                                                    teamIdValue
                                                );
                                            }
                                        }}
                                    />
                                </div>
                            </div>
                        )}
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <WhenPermitted rule="editSkill">
                        <ModusButton
                            className="button-flex-order1"
                            filled
                            content="Save"
                            type="submit"
                            loading={loading}
                        />
                    </WhenPermitted>
                    <ModusButton
                        className="button-flex-order2"
                        grey
                        type="button"
                        content="Close"
                        onClick={onClose}
                    />
                </Modal.Actions>
            </Modal>
        );
    }
}
