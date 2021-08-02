import React, { Component } from "react";
import { connect } from "react-redux";
import { reset } from "redux-form";
import { success, error, info } from "react-notification-system-redux";
import autobind from "autobind-decorator";
import axios from "axios";

import { loadDDLTeams } from "@store/team";

import { loadUsers } from "@store/users";

import "./teams.less";

import { Prompt } from '@components'

import { getAuthHeaders } from "@infrastructure/auth";

import {
    addTeam,
    editTeam,
    deleteTeam,
    loadTeams,
    loadSkillsForNewTeam,
    loadOutsourcers,
    setFormulaOutsourceAssigne,
    saveFormulaOutsource,
    publishFormula,
    publishTeam,
    publishSkill
} from "@store/team";

import { TeamFormModal as TeamFormModalWindow, VendorInvitesModal } from "@components";

import TeamHeader from "./components/TeamHeader";

import TeamAccordion from "./components/TeamAccordion";

import "./teams.less";

@connect(
    (state) => ({
        teams: state.team.teams,
        teamSkills: state.team.teamSkills,
        loading: state.team.loading,
        error: state.team.error,
        outsourcers: state.team.outsourcers,
        teamIdValue: 0,
        teamData: state.team.ddlTeams,
        users: state.users.users,
    }),
    {
        loadTeams,
        loadOutsourcers,
        loadSkillsForNewTeam,
        addTeam,
        editTeam,
        deleteTeam,
        setFormulaOutsourceAssigne,
        saveFormulaOutsource,
        reset,
        success,
        error,
        info,
        publishFormula,
        publishTeam,
        publishSkill,
        loadDDLTeams,
        loadUsers
    }
)
export default class Teams extends Component {

    constructor(props) {
        super(props);

        this.state = {
            vendorInvitePopupOpen: false,
            selectedFormulaId: 0,
            teamName: '',
            isGlobalTeam: false,
            skillName: '',
            isGlobalSkill: false,
            formulaName: '',
            addOnlyTeam: false,
        }

        this.closeVendorInvitePopup = this.toggleVendorInvitePopup.bind(this, false);
    }

    handleFormulaPublishClick = (formulaId, canPublish) => {
    //     if (canPublish === false) {
    //         this.props.error({
    // title: "You can't Publish this Formula as there is no Vendor available for all tasks of this Formula.",
    //         })
    //         return false;
    //     }

        this.props.publishFormula(formulaId)
            .then((response) => {
                axios
                    .post('/api/formulaTask/add-neo-tasks', 
                        JSON.stringify(formulaId), 
                        getAuthHeaders())
                    .then(() => {
                        this.props.loadTeams();

                        if (response.value.data.result === 1) {
                            this.props.success({
                                title: "Formula Published successfully",
                            })
                        }
                        else {
                            this.props.error({
                                title: "There is some issue in sync.",
                            })
                        }
                    });
            })
            .catch(() => {
                this.props.error({
                    title: "Error in publishing the formula",
                });
            });
    }

    handleTeamPublishClick = (teamId) => {
        this.props.publishTeam(teamId).then((response) => {
            this.props.loadTeams();
            if (response.value.data[0].result == 1) {
                this.props.success({
                    title: "Team Published successfully",
                })
            }
            else {
                this.props.error({
                    title: "There is some issue in sync.",
                })
            }
        });
    }

    handleSkillPublishClick = (skillId) => {
        this.props.publishSkill(skillId).then((response) => {
            this.props.loadTeams();
            if (response.value.data[0].result == 1) {
                this.props.success({
                    title: "Skill Published successfully",
                })
            }
            else {
                this.props.error({
                    title: "There is some issue in sync.",
                })
            }
        });
    }

    componentDidMount() {
        this.props.loadTeams();        
    }

    toggleVendorInvitePopup = (opened) => {
        if (opened) {
            this.setState({
                vendorInvitePopupOpen: true
            });
        } else {
            this.setState({
                vendorInvitePopupOpen: false
            });
        }
    };

    handleAddTeam = () => {
        this.setState({ addOnlyTeam: false });
        this.props.loadSkillsForNewTeam(0);
        this.props.loadDDLTeams();
        this.props.loadUsers();
        this.teamFormModal
            .show({})
            .then((team) => {
                this.props
                    .addTeam(team)
                    .then(() => {
                        this.props.loadTeams();
                        this.props.success({
                            title: "Team was added successfully",
                        })
                    }
                    )
                    .catch(() =>
                        this.props.error({ title: "Cannot add new team" })
                    );
                this.props.reset("teamFormModal");
            })
            .catch((ex) => {
                this.props.reset("teamFormModal");
            });
    };

    handleAddOnlyTeam = () => {
        //  this.props.loadSkillsForNewTeam(0);
        this.setState({ addOnlyTeam: true });

        this.teamFormModal
            .show({})
            .then((team) => {
                this.props
                    .addTeam(team)
                    .then(() => {
                        this.props.loadTeams();
                        this.props.success({
                            title: "Team was added successfully",
                        })
                    }
                    )
                    .catch(() =>
                        this.props.error({ title: "Cannot add new team" })
                    );
                this.props.reset("teamFormModal");
            })
            .catch((ex) => {
                this.props.reset("teamFormModal");
            });
    };

    @autobind
    async handleEditSkill(e, teamId) {
        // add this to avoid the propogation of click to the accordion
        e.stopPropagation();

        this.setState({ addOnlyTeam: false });
        this.props.loadDDLTeams();
        const [teamResponse] = await Promise.all([
            axios.get(`api/teams/${teamId}`, getAuthHeaders()),
            this.props.loadSkillsForNewTeam(teamId),
        ]);

        this.setState({ teamIdValue: teamId });

        this.teamFormModal
            .show({ initialValues: teamResponse.data })
            .then((team) => {
                this.props
                    .editTeam(team)
                    .then(() => {
                        this.props.loadTeams();
                        this.props.success({
                            title: "Team was updated successfully",
                        });
                    })
                    .catch(() =>
                        this.props.error({ title: "Cannot update the Team" })
                    );

                this.props.reset("teamFormModal");
            })
            .catch(() => {
                this.props.reset("teamFormModal");
            });
    }

    @autobind
    async handleDeleteTeam(teamId) {
        const confirmed = await Prompt.confirm(
            `Do you want to delete team ${this.props.teams.find(x => x.id === teamId).teamName}?`,
            'Confirm delete team',
            'users'
        )

        if (confirmed) {
            this.props.deleteTeam(teamId)
                .then(() => this.props.success({ title: 'Team was deleted successfully' }))
                .catch(() => this.props.error({ title: 'Cannot delete team' }));
        }
    }

    handleOpenInvites = (...args) => {
        const [
            formulaTaskId,
            teamName,
            isGlobalTeam,
            skillName,
            isGlobalSkill,
            formulaName
        ] = args;

        this.props.loadOutsourcers(formulaTaskId)
            .then(() => {
                this.setState({
                    formulaTaskId: formulaTaskId,
                    teamName: teamName,
                    isGlobalTeam: isGlobalTeam,
                    skillName: skillName,
                    isGlobalSkill: isGlobalSkill,
                    formulaName: formulaName
                });
                this.toggleVendorInvitePopup(true);
            });
    };

    handleFormulaOutsourceAssigne = (vendorId, isSelected) => {
        this.props.setFormulaOutsourceAssigne(vendorId, isSelected);
    };

    handleSendInvites = () => {
        this.props.saveFormulaOutsource(this.state.formulaTaskId);
        this.toggleVendorInvitePopup(false);
    };

    render() {
        const { teams, teamSkills, loading } = this.props;
        let isAdmin = false;

        if (teams && teams.length > 0 && teams[0].isAdmin) {
            isAdmin = true;
        }
        const {
            teamName,
            isGlobalTeam,
            skillName,
            isGlobalSkill,
            formulaName,
            vendorInvitePopupOpen,
            publishFormula
        } = this.state;
        return (
            <div className="iauto-projects">

                <TeamHeader isAdmin={isAdmin} onAddSkillClick={this.handleAddTeam}
                    AddOnlyTeam={this.handleAddOnlyTeam} />

                <TeamAccordion teams={teams}
                    onEdit={this.handleEditSkill}
                    onDelete={this.handleDeleteTeam}
                    onOpenOutsourceInvites={this.handleOpenInvites}
                    publishFormula={this.handleFormulaPublishClick}
                    publishTeam={this.handleTeamPublishClick}
                    publishSkill={this.handleSkillPublishClick}
                    isAdmin={isAdmin}
                    loading={loading}
                />

                <TeamFormModalWindow
                    ref={(c) => {
                        this.teamFormModal = c;
                    }}
                    skills={teamSkills}
                    teams={teams}
                    loading={loading}
                    loadSkillsForNewTeam={this.props.loadSkillsForNewTeam}
                    isAdmin={isAdmin}
                    teamIdValue={this.state.teamIdValue}
                    addOnlyTeam={this.state.addOnlyTeam}
                    teamsDll={this.props.teamData}
                    users={this.props.users}
                />

                <VendorInvitesModal
                    loading={loading}
                    open={vendorInvitePopupOpen}
                    onClose={this.closeVendorInvitePopup}
                    outsourcers={this.props.outsourcers}
                    onSendInvites={this.handleSendInvites}
                    onSetFormulaOutsourceAssigne={this.handleFormulaOutsourceAssigne}
                    teamName={teamName}
                    isGlobalTeam={isGlobalTeam}
                    skillName={skillName}
                    isGlobalSkill={isGlobalSkill}
                    formulaName={formulaName}
                />
            </div>
        );
    }
}
