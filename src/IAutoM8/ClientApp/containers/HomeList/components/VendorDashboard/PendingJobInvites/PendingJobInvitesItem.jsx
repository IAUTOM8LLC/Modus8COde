import React from 'react';
import moment from 'moment';
import { Header, Table } from 'semantic-ui-react';

import { 
    CountdownTimer,
    Inline,
    ModusButton,
    TaskDueDate,
    TaskStartDate,
    Prompt
} from '@components';

const PendingJobInvitesItem = ({
    pendingInvite,
    onPendingInviteRefresh,
    onAccepInvite,
    onDeclineInvite,
}) => {

    Date.prototype.addHours = function (h) {
        this.setTime(this.getTime() + (h * 60 * 60 * 1000));
        return this;
    }

    const todaysDate = moment(new Date().toISOString());//moment(new Date());
    //const targetDate = moment(pendingInvite.sentOn || new Date().addHours(4).toISOString()).add(
    //    1,
    //    "days"
    //);

    const targetDate = moment(new Date(pendingInvite.sentOn).addHours(24).toISOString());
   
    const onAccept = () => onAccepInvite({
        notificationId: pendingInvite.projectTaskVendorId,
        answer: 5
    });


    const onDecline = async () => {
        const confirmed = await Prompt.confirm(
            `Do you want to decline the invite?`,
            "Confirm decline invite",
            "tasks"
        );

        if (confirmed) {
            onDeclineInvite({
                notificationId: pendingInvite.projectTaskVendorId,
                answer: 0
            });
        }
    };

    let secondsRemaining;
    if (targetDate < todaysDate) {
        secondsRemaining = 0;
    } else {
        secondsRemaining = targetDate.diff(todaysDate, "seconds");
    }

    return (
        <Table.Row className="inprogress">
            <Table.Cell style={{ width: "2%" }}>Pending</Table.Cell>
            <Table.Cell style={{ width: "10%" }}>
                {pendingInvite.taskName}
            </Table.Cell>
            <Table.Cell style={{ width: "10%" }}>
                {pendingInvite.formulaName}
            </Table.Cell>
            <Table.Cell style={{ width: "10%" }}>
                {pendingInvite.teamName}
            </Table.Cell>
            <Table.Cell style={{ width: "10%" }}>
                {pendingInvite.skillName}
            </Table.Cell>
            <Table.Cell style={{ width: "3%" }}>
                {pendingInvite.startDate && (
                    <TaskStartDate date={pendingInvite.startDate} />
                )}
            </Table.Cell>
            <Table.Cell style={{ width: "3%" }}>
                {pendingInvite.dueDate && (
                    <TaskDueDate date={pendingInvite.dueDate} />
                )}
            </Table.Cell>
            <Table.Cell style={{ width: "3%", textAlign: "center" }}>
                {pendingInvite.durationHours}
            </Table.Cell>
            <Table.Cell style={{ width: "3%" }}>
                {secondsRemaining > 0 ? (
                    <CountdownTimer
                        secondsRemaining={secondsRemaining}
                        onPendingInviteRefresh={onPendingInviteRefresh}
                        taskId={pendingInvite.taskId}
                    />
                ) : (
                    <div
                        style={{
                            fontWeight: "900",
                            fontSize: "1.2em",
                            color: "#c3922e",
                        }}
                    >
                        {"00:00:00"}
                    </div>
                )}
            </Table.Cell>

            <Table.Cell style={{ width: "3%" }}>
                {/* {targetDate > todaysDate ? (
                    <Link
                        to={`/notification/project-task/${pendingInvite.projectTaskVendorId}`}
                        className="ui right floated button iauto purple wide"
                        value="Accept"
                    >
                        Accept
                    </Link>
                ) : (
                    <Header as="h4" className="ui floated">
                        Invite Expired!
                    </Header>
                )} */}
                <Inline>
                    {targetDate > todaysDate ? (
                        <ModusButton
                            filled
                            type="button"
                            content="Accept"
                            className="button-flex-order2"
                            onClick={onAccept}
                        />
                    ) : (
                        <Header as="h4" className="ui floated">
                            Invite Expired!
                        </Header>
                    )}
                    <ModusButton
                        circular
                        color="red"
                        type="button"
                        icon="iauto--remove-white"
                        className="delete"
                        popup="Decline the Invite"
                        onClick={onDecline}
                    />
                </Inline>
            </Table.Cell>
        </Table.Row>
    );
};

export default PendingJobInvitesItem;
