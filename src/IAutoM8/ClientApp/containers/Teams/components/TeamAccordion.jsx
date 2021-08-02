/* eslint-disable max-len */
import _ from "lodash";
import React, { useMemo } from "react";
import { Accordion, Icon } from "semantic-ui-react";
import { v4 as uuidv4 } from "uuid";

import TeamTable from "./TeamTable";

import { ModusButton } from "@components";

export default function TeamAccordion({
    teams,
    onEdit,
    onOpenOutsourceInvites,
    publishFormula,
    publishTeam,
    publishSkill,
    isAdmin,
}) {
    const skillTitle = (skill, index) => {
        return (
            <Accordion.Title
                //className={skill.skillIsGlobal ? "isGlobal" : ""}
                style={{ padding: "5px" }}
                index={index}
                key={uuidv4()}
            >
                <div className="accordionTitleSec">
                    <div className="accTitleNicon">
                        <Icon name="dropdown" />
                        <span style={{ marginLeft: "20px" }}></span>

                        {skill.skillIsGlobal &&
                            <span style={{ color: "#333", fontWeight: "bold", fontSize: "16px" }}>
                                {skill.skillName}
                            </span>
                        }
                        {!skill.skillIsGlobal &&
                            <span style={{ color: "#333", fontWeight: "normal", fontSize: "15px" }}>
                                {skill.skillName}
                            </span>
                        }

                    </div>
                    {isAdmin && (
                        <div className="accEdit">
                            <ModusButton
                                style={{ display: "none" }}
                                type="button"
                                circular
                                icon="rss"
                                popup="Publish to all Users."
                                className={
                                    skill.skillStatus && skill.skillStatus == 3
                                        ? "Published"
                                        : ""
                                }
                                onClick={() => publishSkill(skill.skillId)}
                            />
                        </div>
                    )}
                </div>
            </Accordion.Title>
        );
    };

    const skillpanels = (skills = [], teamName, isGlobalTeam) => {
        return (
            !!(_.isArray(skills) && skills.length) &&
            skills.map((skill, index) => ({
                key: uuidv4(),
                title: skillTitle(skill, index),
                content: {
                    content:
                        _.isArray(skill.formulas) && skill.formulas.length ? (
                            <TeamTable
                                teamName={teamName}
                                isGlobalTeam={isGlobalTeam}
                                skillName={skill.skillName}
                                isGlobalSkill={skill.skillIsGlobal}
                                tableData={skill.formulas}
                                onOpenOutsourceInvites={onOpenOutsourceInvites}
                                publishFormula={publishFormula}
                                isAdmin={isAdmin}
                            />
                        ) : (
                                <div>There are no tasks</div>
                            ),
                },
            }))
        );
    };

    const skillPanelContent = (skills, teamName, isGlobalTeam) => {
        return (
            <div>
                {/* <table
                    className="ui sortable table"
                    style={{ marginBottom: "0px" }}
                >
                    <thead className="" style={{ lineHeight: "13px" }}>
                        <tr className="">
                            <th>Skill</th>
                            <th></th>
                            <th></th>
                        </tr>
                    </thead>
                </table> */}
                <Accordion.Accordion
                    className="skillTable"
                    // defaultActiveIndex={0}
                    panels={skillpanels(skills, teamName, isGlobalTeam)}
                />
            </div>
        );
    };

    const teamTitle = (team, index) => {
        const fontWeight = team.isGlobal ? "bold" : "";
        return (
            <Accordion.Title
                //className={team.isGlobal ? "isGlobal" : ""}
                style={{ padding: "5px" }}
                index={index}
                key={uuidv4()}
            >
                <div className="accordionTitleSec">
                    <div className="accTitleNicon">
                        <Icon name="dropdown" />
                        <span style={{ marginLeft: "20px" }}></span>
                        {team.isGlobal &&
                            <span style={{ color: "#333", fontWeight: "bold", fontSize: "16px" }}>
                                {team.teamName}
                            </span>
                        }

                        {!team.isGlobal &&
                            <span style={{ color: "#333", fontWeight: "normal", fontSize: "15px" }}>
                                {team.teamName}
                            </span>
                        }

                    </div>
                    {(!team.isGlobal || isAdmin) && (
                        <div className="accEdit">
                            <ModusButton
                                circular
                                icon="iauto--edit"
                                popup="Edit"
                                onClick={(e) => onEdit(e, team.id)}
                                whenPermitted="formulaModalWindow"
                            />
                            {isAdmin && (
                                <ModusButton
                                    style={{ display: "none" }}
                                    type="button"
                                    circular
                                    icon="rss"
                                    popup="Publish to all Users."
                                    className={
                                        team.teamStatus && team.teamStatus == 3
                                            ? "Published"
                                            : ""
                                    }
                                    onClick={() => publishTeam(team.id)}
                                />
                            )}
                        </div>
                    )}
                    {(team.isGlobal && !isAdmin) &&
                        <div className="field">
                            <button className="ui mini circular icon button iauto" role="button">
                                <i aria-hidden="true" className="icon iauto iauto--unlock undefined"></i>
                            </button>
                        </div>
                    }
                </div>
            </Accordion.Title>
        );
    };

    const teamPanels =
        (!!(_.isArray(teams) && teams.length && teams[0].id > 0) &&
            teams.map((team, index) => {
                return {
                    key: uuidv4(),
                    title: teamTitle(team, index),
                    content: {
                        content:
                            team.skill && team.skill.length > 0 ? (
                                skillPanelContent(
                                    team.skill || [],
                                    team.teamName,
                                    team.isGlobal
                                )
                            ) : (
                                    <div>There are no skills</div>
                                ),
                    },
                };
            })) ||
        [];

    return useMemo(() => (
        <div>
            <table
                className="ui sortable table"
                style={{ marginBottom: "0px", lineHeight: "13px" }}
            >
                <thead className="">
                    <tr className="">
                        <th>Team, Skills &amp; Formulas</th>
                        <th></th>
                    </tr>
                </thead>
            </table>
            <div className="teamAcc">
                {teams &&
                    teams.length > 0 &&
                    _.isArray(teams) &&
                    teams[0].id > 0 ? (
                        <Accordion
                            className="accordion"
                            // defaultActiveIndex={0}
                            panels={teamPanels}
                            styled
                        ></Accordion>
                    ) : (
                        <div
                            className="accordion"
                            style={{
                                border: "1px solid #e5eaf2",
                                borderRadius: "3px",
                                padding: "10px",
                                backgroundColor: "white",
                            }}
                        >
                            There are no Teams
                        </div>
                    )}
            </div>
        </div>
    ));
}
