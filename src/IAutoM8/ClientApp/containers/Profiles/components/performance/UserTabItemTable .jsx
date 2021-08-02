import React from "react";
import { Grid, Icon, Table, Header, Rating } from "semantic-ui-react";

import { ModusButton, Inline, UserAcronym, } from "@components";


export default function UserTabItemTable({users, item, toggleAddButtonModal, onEdit,onDelete }) {

    return (
        <Table.Row>
            <Table.Cell>{item.team}</Table.Cell>
            <Table.Cell>{item.task}</Table.Cell>
            <Table.Cell>{item.formula}</Table.Cell>
            <Table.Cell>{item.skill}</Table.Cell>
            <Table.Cell>{item.reviews}</Table.Cell>

            <Table.Cell>
                <Rating
                    disabled
                    maxRating={5}
                    rating={item.rating}
                    icon="star"
                /></Table.Cell>
            <Table.Cell>{item.price}</Table.Cell>
            <Table.Cell>
                <Grid>
                    <Grid.Row>
                        {/* <Grid.Column width={6}>
                            <div style={{ marginTop: "8px" }}>
                                $ {formatMoney(item.price, 2, ".", " ")}
                            </div>
                        </Grid.Column> */}
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
                                    onClick={() => onDelete(users.id,item.formulaTaskId)}
                                />
                            </Inline>
                        </Grid.Column>
                    </Grid.Row>
                </Grid>
            </Table.Cell>
        </Table.Row>
    )
}
