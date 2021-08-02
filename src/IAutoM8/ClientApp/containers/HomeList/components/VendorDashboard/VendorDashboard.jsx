import React from "react";
import { connect } from "react-redux";
import { Button, Header, Divider } from "semantic-ui-react";
import { error, success } from "react-notification-system-redux";
import InfiniteScroll from "react-infinite-scroll-component";

import {
    formulaTaskNotificationResponse,
    projectTaskNotificationResponse,
    loadVednorSnapshotDetail,
    loadVednorJobInvites,
    loadVendorFormulaBids,
    syncVendorData,
    removeJobInvite,
    removeFormulaBid,
} from "@store/vendor";

import { sendNotificationMessage } from "@store/notification";

import {
    doTask,
    doVendorTask,
    loadTasksInStatus,
    loadVendorTasksInStatus,
    loadOwnerUserTasksInStatus,
    loadSelectedUserTasksInStatus,
    cancelVendorTask,
    updateExpiredInvitesToOwner,
} from "@store/projectTasks";

import { ModusButton, ProjectHeader, Prompt, Snapshot } from "@components";

import Stage from "../Stage";

import PendingJobInvitesTable from "./PendingJobInvites/PendingJobInvitesTable";
import PendingBidRequestsTable from "./PendingJobInvites/PendingBidRequestsTable";

class VendorDashboard extends React.Component {
    static stages = [
        {
            value: "InProgress",
            title: "TASKS TO DO",
            permission: false,
        },
    ];

    state = {
        isActive: true,
        isArchive: false,
        isGig: true,
        isFormula: false,
        disabled : false
    };

    componentWillMount() {
        window.scrollTo(0, 0);
    }
    componentDidMount() {
        this.props.syncVendorData().then(() => {
            this.props.loadVendorTasksInStatus();
            this.props.loadVednorSnapshotDetail();
            this.props.loadVednorJobInvites();
            this.props.loadVendorFormulaBids();
        });
    }
    handleDoCard = (taskId)=>{
        //location.reload();
        if (this.state.disabled) {
            //console.log('disable');
            return;
        }
         //this.setState({disabled: true});
        this.props.doVendorTask(taskId)
        .then(()=>{
            //console.log('sdsfsfsfsfsfsfsf');
            this.setState({disabled: true});
            this.props.loadVendorTasksInStatus();
            this.props.loadVednorSnapshotDetail();
            this.setState({disabled: false});
        })
    }

    goToBoard = () => {
        this.props.setProjectsToLoad(this.props.projects.map((t) => t.id));
        this.props.loadTasksForAllProjects();
    };

    loadNextTasks = (status) => {
        if (this.state.filterUserId) {
            this.props.loadOwnerUserTasksInStatus(null, status);
        } else {
            this.props.loadTasksInStatus(status);
        }
    };

    sendNudgeNotification = (id) => {
        this.props
            .sendNotificationMessage(id)
            .then(() => {
                this.props.loadVendorTasksInStatus();
                if (this.props.notificationStatus.id === 3) {
                    this.props.success({
                        title: this.props.notificationStatus.message,
                    });
                } else {
                    this.props.error({
                        title: this.props.notificationStatus.message,
                    });
                }
            })
            .catch((error) =>
                this.props.error({
                    title: error.data.message,
                })
            );
    };

    handleCancelNudgeNotification = (id, price) => {
        Prompt.confirm(
            `By cancelling this you will loose $ ${price}`,
            "Are you sure",
            "lab"
        ).then((confirmed) => {
            if (confirmed) {
                this.props.cancelVendorTask(id);
            }
        });
    };

    handlePendingInviteRefresh = (id) => {
        this.props.updateExpiredInvitesToOwner(id).then(() => this.props.loadVednorJobInvites());
        this.forceUpdate();
    };

    handleVendorInviteResponse = (vendorNotification) => {
        this.props.projectTaskNotificationResponse(vendorNotification)
            .then(() => {
                this.props.removeJobInvite(vendorNotification.notificationId);
                this.props.loadVendorTasksInStatus();
                this.props.loadVednorSnapshotDetail();
            })
            .catch((error) =>
                this.props.error({ title: error.data.message })
            );
    };

    handleFormulaBidDecline = (vendorNotification) => {
        this.props.formulaTaskNotificationResponse(vendorNotification)
            .then(() => {
                this.props.removeFormulaBid(vendorNotification.notificationId);
                this.props.loadVendorTasksInStatus();
            })
            .catch((error) =>
                this.props.error({ title: error.data.message })
            );
    };

    onToggle = () => {
        this.setState((prevState) => ({
            isActive: !prevState.isActive,
            isArchive: !prevState.isArchive,
        }));
    };

    onToggleGigOrFormula = () => {
        this.setState((prevState) => ({
            isGig: !prevState.isGig,
            isFormula: !prevState.isFormula,
        }));
    }

    render() {
        const {
            userTasks,
            hasMoreTasks,
            vendorSnapshots,
            vendorJobInvites,
            vendorFormulaBids,
        } = this.props;

        let filteredTasks = [];

        if (this.state.isActive) {
            filteredTasks = userTasks.filter(t => t.status !== "COMPLETED");
        } else {
            filteredTasks = userTasks.filter(t => t.status === "COMPLETED");
        }

        const unreadBidMessageCount = vendorFormulaBids.length;

        return (
            <div className="container">
                <div>
                    <Snapshot vendorSnapshots={vendorSnapshots} />
                    <Divider />
                </div>

                <ul className="lists">
                    {VendorDashboard.stages.map((stage) => {
                        const filterUserId = null;
                        const stageTasks = userTasks;
                        let height = 200;

                        if (stageTasks.length > 0) {
                            height =
                                stageTasks.length < 4
                                    ? stageTasks.length * 175
                                    : 400;
                        }

                        return (
                            <div key={stage.value} className="list-item">
                                <div className="iauto-vendor-task-header">
                                    {/* <ProjectHeader
                                        name={stage.title}
                                        tasks={stageTasks.length}
                                        overdue={
                                            stageTasks.filter(
                                                (t) =>
                                                    t.status.toLowerCase() ===
                                                    "tasksoverdue"
                                            ).length
                                        }
                                        // className="iauto-task-header__left"
                                        style={{ paddingTop: "9px" }}
                                    /> */}
                                    <div style={{ paddingTop: "9px" }}>
                                        <Header size="medium">TO DO</Header>
                                    </div>
                                    <div>
                                        <Button.Group>
                                            <ModusButton
                                                filled={this.state.isActive}
                                                grey={!this.state.isActive}
                                                onClick={() => this.onToggle()}
                                                content="ACTIVE"
                                            />
                                            <ModusButton
                                                filled={this.state.isArchive}
                                                grey={!this.state.isArchive}
                                                onClick={() => this.onToggle()}
                                                content="ARCHIVE"
                                            />
                                            {/* <ModusButton
                                                filled={this.state.isArchive}
                                                grey={!this.state.isArchive}
                                                onClick={() => this.onToggle()}
                                                content="In Review"
                                            /> */}
                                        </Button.Group>
                                    </div>
                                </div>

                                <Divider />

                                <div className="taskTodoList">
                                    <InfiniteScroll
                                        dataLength={filteredTasks}
                                        next={() =>
                                            this.loadNextTasks(stage.value)
                                        }
                                        hasMore={hasMoreTasks[stage.value]}
                                        height={height}
                                        loader={"Loading..."}
                                        endMessage={"No more tasks"}
                                    >
                                        <Stage
                                            sendNudgeNotification={
                                                this.sendNudgeNotification
                                            }
                                            key={stage.value}
                                            title={stage.title}
                                            filterUserId={filterUserId}
                                            tasks={filteredTasks}
                                            onEditTask={this.props.editTask}
                                            onOutsourceTabOpen={
                                                this.props.onOutsourceTabOpen
                                            }
                                            handleDoCard={this.handleDoCard}
                                            disabled = {this.state.disabled}
                                            handleReviewCard={
                                                this.props.reviewTask
                                            }
                                            onGoToProject={
                                                this.props.onGoToProject
                                            }
                                            projects={this.props.projects}
                                            onCancelNudgeNotification={
                                                this
                                                    .handleCancelNudgeNotification
                                            }
                                            isVendor={true}
                                        />
                                    </InfiniteScroll>
                                </div>
                            </div>
                        );
                    })}
                </ul>

                <div style={{ marginBottom: "25px" }}>
                    <Divider />
                    <div className="iauto-vendor-task-header">
                        {/* <div className="ui header">INVITES</div> */}
                        <div style={{ paddingTop: "9px" }}>
                            <Header size="medium">INVITES</Header>
                        </div>
                        <div>
                            <Button.Group>
                                <ModusButton
                                    filled={this.state.isGig}
                                    grey={!this.state.isGig}
                                    onClick={this.onToggleGigOrFormula}
                                    content="TASK"
                                />
                                <ModusButton
                                    filled={this.state.isFormula}
                                    grey={!this.state.isFormula}
                                    onClick={this.onToggleGigOrFormula}
                                    content="NEW BID"
                                />
                            </Button.Group>
                            {
                                unreadBidMessageCount !== 0 &&
                                <div className="iauto-notify--bid-message">{unreadBidMessageCount}</div>
                            }
                        </div>
                    </div>
                    <Divider />
                    {this.state.isGig && (
                        <div style={{ overflow: "auto" }}>
                            <PendingJobInvitesTable
                                vendorJobInvites={vendorJobInvites}
                                onPendingInviteRefresh={this.handlePendingInviteRefresh}
                                onAccepInvite={this.handleVendorInviteResponse}
                                onDeclineInvite={this.handleVendorInviteResponse}
                            />
                        </div>
                    )}
                    {this.state.isFormula && (
                        <div style={{ overflow: "auto" }}>
                            <PendingBidRequestsTable 
                                vendorFormulaBids ={vendorFormulaBids}
                                onDeclineFormulaBid ={this.handleFormulaBidDecline}
                            />
                        </div>
                    )}
                </div>
            </div>
        );
    }
}

export default connect(
    (state) => ({
        ...state.notification,
        userTasks: state.projectTasks.userTasks,
        notificationStatus: state.notification.notificationStatus,
        vendorSnapshots: state.vendor.vendorSnapshots,
        vendorJobInvites: state.vendor.vendorJobInvites,
        vendorFormulaBids: state.vendor.vendorFormulaBids,
    }),
    {
        error,
        success,
        doTask,
        doVendorTask,
        syncVendorData,
        cancelVendorTask,
        loadVednorJobInvites,
        loadVendorFormulaBids,
        loadVendorTasksInStatus,
        loadVednorSnapshotDetail,
        loadTasksInStatus,
        loadOwnerUserTasksInStatus,
        loadSelectedUserTasksInStatus,
        sendNotificationMessage,
        updateExpiredInvitesToOwner,
        formulaTaskNotificationResponse,
        projectTaskNotificationResponse,
        removeJobInvite,
        removeFormulaBid,
    }
)(VendorDashboard);
