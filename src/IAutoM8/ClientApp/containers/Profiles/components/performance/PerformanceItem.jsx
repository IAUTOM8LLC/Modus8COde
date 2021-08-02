import React from "react";
import { Grid, Table, Rating, Popup } from "semantic-ui-react";

import { formatMoney } from "@utils/formatMoney";

import { ModusButton, Inline } from "@components";

export default function PerformanceItem({ item, onDelete, onEdit }) {
    //console.log('item', item);
    const ctMessage = [
        "Completion Time(CT): ",
        "The amount of time it takes to complete a job from when ",
        "the job is started to when it is marked 'complete'",
    ].join("");

    const tatMessage = [
        "Turnaround Time(TAT): ",
        "The amount of time it takes from when the job ",
        "is accepted to when it is completed",
    ].join("");

    return (
        <Table.Row>
            <Table.Cell>{item.team}</Table.Cell>
            <Table.Cell>{item.skill}</Table.Cell>
            <Table.Cell>{item.formula}</Table.Cell>
            <Table.Cell>{item.task}</Table.Cell>
            <Table.Cell textAlign="center">{item.reviews}</Table.Cell>
            <Table.Cell>
                <Rating
                    icon="star"
                    defaultRating={item.rating}
                    maxRating={5}
                    disabled
                />
            </Table.Cell>
            <Table.Cell>
                    <div style={{ marginTop: "8px" }}>
                                $ {formatMoney(item.price, 2, ".", " ")}
                            </div>
            </Table.Cell>
            <Table.Cell>
                <Grid>
                    <Grid.Row>
                        <Grid.Column width={6}>
                            <div onClick={() => onEdit(item.formulaTaskId, item.price)}
                             style={{ marginTop: "8px" , cursor: "pointer" }}>
                                {item.noOfWorkers}
                            </div>
                        </Grid.Column>
                        <Grid.Column width={3}>
                            <Inline>
                                <ModusButton
                                    circular
                                    icon="iauto--edit"
                                    popup="Edit"
                                    onClick={() => onEdit(item.formulaTaskId, item.price)}
                                />
                                <ModusButton
                                    className="btn-per-delete"
                                    circular
                                    size="mini"
                                    icon="iauto--remove-white"
                                    popup="Delete"
                                    onClick={() => onDelete(item.id,item.formulaTaskId)}
                                />
                            </Inline>
                        </Grid.Column>
                    </Grid.Row>
                </Grid>
            </Table.Cell>
            <Table.Cell textAlign="right">
                <Popup
                    trigger={
                        <span className="vendor-time">{item.dwellTime}</span>
                    }
                    size="tiny"
                    content={"Dwell Time: The amount of time that a job"}
                />
            </Table.Cell>
            <Table.Cell textAlign="right">
                <span className="mean-vendor-time">{item.avgDwellTime}</span>
            </Table.Cell>
            <Table.Cell textAlign="right">
                <Popup
                    trigger={
                        <span className="vendor-time">
                            {item.completionTime}
                        </span>
                    }
                    size="tiny"
                    content={ctMessage}
                />
            </Table.Cell>
            <Table.Cell textAlign="right">
                <span className="mean-vendor-time">
                    {item.avgCompletionTime}
                </span>
            </Table.Cell>
            <Table.Cell textAlign="right">
                <Popup
                    trigger={
                        <span className="vendor-time">
                            {item.turnaroundTime}
                        </span>
                    }
                    size="tiny"
                    content={tatMessage}
                />
            </Table.Cell>
            <Table.Cell textAlign="right">
                <span className="mean-vendor-time">
                    {item.avgTurnaroundTime}
                </span>
            </Table.Cell>
        </Table.Row>
    );
}
