import axios from "axios";
import React, { Component } from "react";
import { connect } from "react-redux";
import { reset } from "redux-form";
import { success, error, info } from "react-notification-system-redux";
import sortBy from "lodash/sortBy";
import autobind from "autobind-decorator";
import { Tab } from 'semantic-ui-react';

import { getAuthHeaders } from "@infrastructure/auth";
import { loadSkills, addSkill, deleteSkill, editSkill } from "@store/skill";
import { loadUsers } from "@store/users";
import { filterSkillsByQuery } from "@selectors/skill";
import { toggleDirection, skillAccessor } from "@utils/sort";
import { setFilterColumn } from "@store/layout";
import { selectSearchColumn } from "@selectors/layout";

import {
    Prompt, SkillFormModal as SkillFormModalWindow, SkillFormulaModal,
    SkillUserModal
} from "@components";

import SkillTable from "./components/SkillTable";
import SkillHeader from "./components/SkillHeader";

import './Skills.less';

@connect(
    (state) => ({
        ...state.skill,
        skills: filterSkillsByQuery(state),
        users: state.users.users,
        filterBy: selectSearchColumn(state),
    }),
    {
        loadSkills,
        addSkill,
        deleteSkill,
        editSkill,
        reset,
        success,
        error,
        info,
        loadUsers,
        setFilterColumn,
    }
)
export default class Skills extends Component {
    state = {
        orderColumn: null,
        orderDirection: null,
        orderedSkills: [],
        showSkillFormulaModal: false,
        showSkillUserModal: false,
        selectedSkill: {},
        usersList: {},
    };

    componentWillReceiveProps(nextProps) {
        const { orderColumn, orderDirection } = this.state;
        if (orderColumn) {
            let skills = nextProps.skills
                ? sortBy(nextProps.skills, skillAccessor(orderColumn))
                : [];
            if (orderDirection === "descending") {
                skills = skills.reverse();
            }
            this.setState({
                orderedSkills: skills,
            });
        } else {
            this.setState({
                orderedSkills: [...nextProps.skills],
            });
        }
    }

    componentDidMount() {
        this.props.loadSkills();
    }

    @autobind
    async handleEditSkill(skillId) {
        const [skillResponse] = await Promise.all([
            axios.get(`api/skill/${skillId}`, getAuthHeaders()),
            this.props.loadUsers(),
        ]);

        const { orderedSkills } = {...this.state};

        this.skillFormModal
            .show({ initialValues: skillResponse.data })
            .then((skill) => {
                this.props
                    .editSkill(skill)
                    .then((response) => {
                        const updatedTask = response.value.data;
                        const oldSkillItem = orderedSkills.find(
                            (t) => t.id === updatedTask.id
                        );

                        orderedSkills[
                            orderedSkills.indexOf(oldSkillItem)
                        ] = updatedTask;
                        this.setState({
                            orderedSkills: orderedSkills,
                        });
                        this.props.success({
                            title: "Skill was updated successfully",
                        });
                    })
                    .catch(() =>
                        this.props.error({ title: "Cannot update skill" })
                    );

                this.props.reset("skillFormModal");
            })
            .catch(() => {
                this.props.reset("skillFormModal");
            });
    }

    handleAddSkill = () => {
        this.props.loadUsers();

        this.skillFormModal
            .show({})
            .then((skill) => {
                skill.type = 2; // All the new Skills that are added wii be of Type = 2, for Custom Skills
                this.props
                    .addSkill(skill)
                    .then(() =>
                        this.props.success({
                            title: "Skill was added successfully",
                        })
                    )
                    .catch(() =>
                        this.props.error({ title: "Cannot add new skill" })
                    );
                this.props.reset("skillFormModal");
            })
            .catch(() => {
                this.props.reset("skillFormModal");
            });
    };

    @autobind
    async handleDeleteSkill(skillId) {
        const confirmed = await Prompt.confirm(
            `Do you want to delete skill ${
            this.props.skills.find((x) => x.id === skillId).name
            }?`,
            "Confirm delete skill",
            "users"
        );

        if (confirmed) {
            this.props
                .deleteSkill(skillId)
                .then(() =>
                    this.props.success({
                        title: "Skill was deleted successfully",
                    })
                )
                .catch(() =>
                    this.props.error({ title: "Cannot delete skill" })
                );
        }
    }

    handleSort = (clickedColumn) => () => {
        const { orderColumn, orderDirection, orderedSkills } = this.state;

        if (orderColumn !== clickedColumn) {
            this.setState({
                orderColumn: clickedColumn,
                orderedSkills: sortBy(
                    orderedSkills,
                    skillAccessor(clickedColumn)
                ),
                orderDirection: "ascending",
            });
            return;
        }

        this.setState({
            orderedSkills: orderedSkills.reverse(),
            orderDirection: toggleDirection(orderDirection),
        });
    };

    showFormulaModal = (skill) => {
        let combinedList = [];
        if (skill.devFormulas != undefined || skill.revFormulas != undefined) {
            combinedList = [...skill.devFormulas, ...skill.revFormulas];
        }

        const distinctFormulas = combinedList.filter(
            (thing, i, arr) => arr.findIndex((t) => t.id === thing.id) === i
        );

        this.setState({
            showSkillFormulaModal: true,
            selectedSkill: distinctFormulas,
        });
    };


    showUserModal = (users) => {
        if (users.length == 0) {
            return false;
        }
        this.setState({
            showSkillUserModal: true,
            usersList: users,
        });
    };


    hideUserModal = () => {
        this.setState({
            showSkillUserModal: false,
        });
    };

    hideFormulaModal = () => {
        this.setState({
            showSkillFormulaModal: false,
        });
    };

    renderTab = () => {
        const { orderedSkills } = this.state;

        let panes = [];

        const isAdmin = this.props.roles.includes("Admin");
        const globalOrderedSkills = orderedSkills.filter((s) => s.isGlobal);
        const customOrderedSkills = orderedSkills.filter((s) => !s.isGlobal);

        panes = [
            {
                menuItem: { key: "public", content: "Public" },
                pane: {
                    key: "public",
                    content: this.renderSkills(globalOrderedSkills),
                },
            },
            {
                menuItem: { key: "custom", content: "Custom" },
                pane: {
                    key: "custom",
                    content: this.renderSkills(customOrderedSkills),
                },
            },
        ].filter((pane) => {
            if (isAdmin) {
                if (pane.menuItem.key !== "custom") {
                    return pane;
                }
            } else {
                return pane;
            }
        });

        return (
            <Tab
                panes={panes}
                renderActiveOnly={false}
                menu={{ secondary: true, pointing: true }}
                onTabChange={this.handleFormulaSelector}
            />
        );
    };

    renderSkills = (skills) => {
        const { orderColumn, orderDirection } = this.state;

        return (
            <React.Fragment>
                <SkillTable
                    skills={skills}
                    onDelete={this.handleDeleteSkill}
                    onEdit={this.handleEditSkill}
                    orderColumn={orderColumn}
                    onSort={this.handleSort}
                    sortDirection={orderDirection}
                    loggedInUserRole={
                        this.props.roles.length > 0 ? this.props.roles[0] : ""
                    }
                    showFormulaModal={this.showFormulaModal}
                    showUserModal={this.showUserModal}
                />
            </React.Fragment>
        );
    };

    render() {
        const { setFilterColumn, filterBy } = this.props;
        const {
            showSkillFormulaModal,
            selectedSkill,
            showSkillUserModal, usersList
        } = this.state;
        
        return (
            <div className="iauto-projects iauto-skills">
                <SkillHeader
                    onAddSkillClick={this.handleAddSkill}
                    filterBy={filterBy}
                    setFilterColumn={setFilterColumn}
                />

                <div className="skills-pane tab-modal">
                    {this.renderTab()}
                </div>

                <SkillFormModalWindow
                    ref={(c) => {
                        this.skillFormModal = c;
                    }}
                    users={this.props.users}
                    loading={this.props.loading}
                    skills={this.props.skills}
                    isSkillContainer={true}
                />

                <SkillFormulaModal
                    open={showSkillFormulaModal}
                    selectedSkill={selectedSkill}
                    onClose={this.hideFormulaModal}
                />

                <SkillUserModal
                    open={showSkillUserModal}
                    usersList={usersList}
                    onClose={this.hideUserModal}
                />
            </div>
        );
    }
}
