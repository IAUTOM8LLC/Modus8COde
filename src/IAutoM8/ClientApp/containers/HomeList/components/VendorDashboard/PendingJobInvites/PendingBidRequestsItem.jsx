import React from 'react';
import { Link } from 'react-router-dom';
import { Table } from 'semantic-ui-react';

import {
    Inline,
    ModusButton,
    Prompt,
    TimeAgo,
} from '@components';


const PendingBidRequestsItem = ({
    formulaBid,
    onDeclineFormulaBid
}) => {

    const onDecline = async () => {
        const confirmed = await Prompt.confirm(
            `Are you sure you want to decline this formula bid?`,
            "Confirm decline formula bid",
            "tasks"
        );
        
        if (confirmed) {
            onDeclineFormulaBid({
                notificationId: formulaBid.bidId,
                answer: 0
            });
        }
    };

    return (
        <Table.Row className="inprogress">
            <Table.Cell>
                {formulaBid.taskName}
            </Table.Cell>
            <Table.Cell>
                {formulaBid.formulaName}
            </Table.Cell>
            <Table.Cell>
                {formulaBid.teamName}
            </Table.Cell>
            <Table.Cell>
                {formulaBid.skillName}
            </Table.Cell>
            <Table.Cell>
                <TimeAgo date={formulaBid.created} />
            </Table.Cell>
            <Table.Cell style={{ width: "3%" }}>
                <Inline>
                    <Link
                        to={`/notification/formula-task/${formulaBid.bidId}`}
                        className="ui right floated button iauto purple"
                        value="Accept"
                    >
                        Accept
                    </Link>
                    <ModusButton
                        circular
                        color="red"
                        type="button"
                        icon="iauto--remove-white"
                        className="delete"
                        popup="Decline the Bid"
                        onClick={() => onDecline(1)}
                    />
                </Inline>
            </Table.Cell>
        </Table.Row>
    );
};

export default PendingBidRequestsItem;
