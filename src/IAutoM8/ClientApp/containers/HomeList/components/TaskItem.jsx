import React from "react";

import { Image, Icon, Popup, Table } from "semantic-ui-react";

import {
    Inline,
    ModusButton,
    ModusIcon,
    TaskFooter,
    TaskDueDate,
    TaskStartDate,
    UserAcronym, CountdownTimer
} from "@components";

export default function TaskItem({
    id,
    title,
    projectName,
    status,
    isOverdue,
    dueDate,
    isConditional,
    isRecurrent,
    isInterval,
    projectId,
    // averageTAT,
    isRead,
    onEditTask,
    proccessingUserId,
    proccessingUserName,
    //reviewingUserName,
    onGoToProject,
    handleDoCard,
    // projects,
    // handleReviewCard,
    // filterUserId,
    // onOutsourceTabOpen,
    formulaName,
    teamName,
    skillName,
    completionTime,
    startDate,
    isVendor,
    // isWorker,
    price,
    isCancel,
    sendNudgeNotification,
    onCancelNudgeNotification,
    onOpenComments,
    onOpenUserProfile,
    onChangeProccessingUser,
    profileImage,
    //showCancelButton,
    //eta,
    //iS_PAST80_ETA,
    //etA_10_REMAINING_SECONDS,
    //iS_PAST80_ETA_10,
    //etA_10,
    deadline,
    deadlinE_REMAINING_SECONDS,
    iS_PAST80_DEADLINE,
    nudgecount,
    onReviewCard
}) {
    //console.log('status', status);  
    // console.log('proccessingUserId',proccessingUserId);
    let secondsRemaining;

    if (deadlinE_REMAINING_SECONDS < 0) {
        secondsRemaining = 0;
    }
    else {
        secondsRemaining = deadlinE_REMAINING_SECONDS;
    }

    const eta80 = Math.trunc((deadline * 80) / 100);

    const openModal = () => onEditTask(id);
    const sendNotification = () => sendNudgeNotification(id);
    //const openOutsourceTab = () => onOutsourceTabOpen(id);
    //const onReviewTask = () => handleReviewCard(id);
    const onReviewTask = () => onReviewCard(id);
    const onDoTask = () => handleDoCard(id);
    const onDoOwnerTask = () => handleDoCard(id);
    const handleGoToProject = () => {
        onGoToProject(projectId);
    };
    const openCommentModal = () => onOpenComments(id);
    const onOpenUserModal = () => onOpenUserProfile(proccessingUserId);
    const openProccessingUserModal = () => onChangeProccessingUser(id);

    const renderNudgeButtons = () => {
        //const circularCount = 
        if (status.toLowerCase() !== "completed") {
            if (showCancelNudge) {
                return (
                    
                    <Inline>
                        <div style={{ marginBottom: "25px" ,left: "39px",top: "-11px "}} 
                        className="iauto-notify--bid-message">{nudgecount}</div>
                        <ModusButton
                            circular
                            popup={"Nudge"}
                            icon="hand pointer icon"
                            className="btn-nudge"
                            onClick={() => sendNotification(id)}
                        />
                        <ModusButton
                            circular
                            icon="iauto--remove"
                            popup="Cancel Nudge"
                            className="btn-nudge-cancel"
                            onClick={() =>
                                onCancelNudgeNotification(id, price)
                            }
                        />
                    </Inline>
                );
            } else {
                return (
                    
                    <Inline>
                        <div style={{ marginBottom: "25px",left: "39px",top: "-11px "}}
                            className="iauto-notify--bid-message">{nudgecount}</div>
                        <ModusButton
                            circular
                            popup={"Nudge"}
                            icon="hand pointer icon"
                            className="btn-nudge"
                            onClick={() => sendNotification(id)}
                        />
                    </Inline>
                );
            }
        }
    };

    const showCancelNudge = isCancel === 1 ? true : false;
    const showNotification = isRead === 0 ? true : false;

    let taskIcon = null;
    // let colorText = null;

    if (status.toLowerCase() === "completed") {
        taskIcon = (
            <ModusIcon name={`iauto--completed`} />
        );
    }
    else if (
         status.toLowerCase() === "overdue" ||
        status.toLowerCase() === "tasksoverdue"
    ) {
        taskIcon = (
            <React.Fragment>
                <ModusIcon
                    style={{ cursor: "pointer" }}
                    name={`task overdue`}
                    onClick={openModal}
                />
                <ModusIcon
                    style={{ cursor: "pointer" }}
                    name={`iauto--doing`}
                    onClick={openModal}
                />
            </React.Fragment>

        );
    }
    else if (status.toLowerCase() === 'notstarted') {
        taskIcon = (
            <ModusIcon
                name="iauto--do"
                popup="Start the task"
                onClick={onDoTask}
            />
        );
    }
    // else if (status.toLowerCase() === 'todo') {
    //     taskIcon = (
    //         <ModusIcon
    //         name="iauto--doing"
    // />
    //     )
    // }
    else if (status.toLowerCase() === "review" && proccessingUserId ) {
        // colorText = "#566063";
        taskIcon = (
            <React.Fragment>
            <ModusIcon
                style={{ cursor: "pointer" }}
                name={`iauto--review`}
                onClick={openModal}
            />
            <ModusIcon
                    style={{ cursor: "pointer" }}
                    name={`iauto--doing`}
                    onClick={openModal}
                />
            </React.Fragment>
        );
    } else if (status.toLowerCase() === "review" && proccessingUserId === null ) {
        // colorText = "#566063";
        taskIcon = (
            <React.Fragment>
            <ModusIcon
                style={{ cursor: "pointer" }}
                name={`iauto--review`}
                onClick={openModal}
            />
            <Icon
                size="large"
                name="clock outline"
                style={{ 
                    cursor: "pointer",
                    color: "#637b94",
                    fontSize: "22px" 
                }}
                onClick={openModal}
            />
            </React.Fragment>
        );
    }
    else if (status.toLowerCase() === "active") {
        // colorText = "#566063";
        taskIcon = (
            <ModusIcon
                style={{ cursor: "pointer" }}
                name={`iauto--doing`}
                onClick={openModal}
            />
        );
    } 
    // else if (status.toLowerCase() === "todo") { 
    //     //By AT for owner todo status icon changed on proccessingUserId
    //     // colorText = "#566063";
    //     taskIcon = (
    //         <ModusIcon
    //             style={{ cursor: "pointer" }}
    //             name={`iauto--doing`}
    //             onClick={openModal}
    //         />
    //     );
    // }
    else if (status.toLowerCase() === "todo" && proccessingUserId === null) {
        // colorText = "#566063";
        taskIcon = (
            <Icon
                size="large"
                name="clock outline"
                style={{ 
                    cursor: "pointer",
                    color: "#637b94",
                    fontSize: "22px" 
                }}
                //onClick={onDoOwnerTask}
            />
        );
    }
    else if (status.toLowerCase() === "todo-notstarted" && proccessingUserId === null) {
        // colorText = "#566063";
        taskIcon = (
            <ModusIcon
                name="iauto--do"
                popup="Start the task"
                onClick={onDoOwnerTask}
            />
        );
    }
    else if (status.toLowerCase() === "review-notstarted" && proccessingUserId === null) {
        // colorText = "#566063";
        taskIcon = (
            <React.Fragment>
            <ModusIcon
                style={{ cursor: "pointer" }}
                name={`iauto--review`}
                onClick={onReviewTask}
            />
            <ModusIcon
                name="iauto--do"
                popup="Start the task"
                onClick={onReviewTask}
            />
            </React.Fragment>
        );
    }
    else if (status.toLowerCase() === "todo" && proccessingUserId) {
        // colorText = "#566063";
        taskIcon = (
            <ModusIcon
                style={{ cursor: "pointer" }}
                name={`iauto--doing`}
                size="large"
                //name="clock outline"
                //onClick={onDoOwnerTask}
                onClick={openModal}
            />
        );
    }
    else {
        taskIcon = (
            // <ModusIcon
            //     style={{ cursor: "pointer" }}
            //     name={`clock outline`}
            // />
            <Icon
                size="large"
                name="clock outline"
                style={{
                    color: "#637b94",
                    fontSize: "22px"
                }} />
        );
    }

    return (
        <Table.Row className="todo">
            {!isVendor && (
                <React.Fragment>
                    <Table.Cell style={{ width: "10%" }}>{taskIcon}</Table.Cell>
                    <Table.Cell
                        style={{
                            // color: colorText,
                            fontWeight: "bold",
                            width: "25%",
                        }}
                    >
                        <span style={{ cursor: "pointer" }} onClick={openModal}>
                            {title}
                        </span>
                    </Table.Cell>
                    {/* <Table.Cell style={{ width: "10%" }}>
                        <span style={{ cursor: "pointer" }}>{status}</span>
                    </Table.Cell> */}
                    <Table.Cell style={{ width: "25%" }}>
                        {formulaName && (
                            <span
                                style={{ cursor: "pointer" }}
                                onClick={handleGoToProject}
                            >
                                <i
                                    aria-hidden="true"
                                    className="icon formulas"
                                />
                                {formulaName}
                            </span>
                        )}
                    </Table.Cell>
                    <Table.Cell style={{ width: "25%" }}>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {projectName}
                        </span>
                    </Table.Cell>
                    <Table.Cell style={{ width: "5%" }}>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={onOpenUserModal}
                        >
                            {/* {proccessingUserName} */}
                            {profileImage && (
                                <Image avatar src={profileImage} />
                            )}
                            {!profileImage && proccessingUserName && (
                                <UserAcronym fullname={proccessingUserName} />
                            )}
                        </span>
                    </Table.Cell>
                    {/* <Table.Cell style={{ width: "5%" }}>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {averageTAT || 0}
                        </span>
                    </Table.Cell>
                    <Table.Cell style={{ width: "10%" }}>
                        <span style={{ cursor: "pointer" }}>
                            {startDate && <TaskStartDate date={startDate} />}
                        </span>
                    </Table.Cell> */}
                    <Table.Cell style={{ width: "10%" }} textAlign="center">
                        <span style={{ cursor: "pointer" }}>
                            {dueDate && <TaskDueDate date={dueDate} />}
                        </span>
                    </Table.Cell>
                    <Table.Cell style={{ width: "5%" }}>
                        {status.toLowerCase() !== "completed" && (
                            <div style={{ display: "inline-flex" }}>
                                <Popup
                                    content={"Add/View Comments"}
                                    trigger={
                                        <Icon
                                            size="large"
                                            style={{
                                                cursor: "pointer",
                                                paddingRight: "5px",
                                            }}
                                            className={`file text`}
                                            onClick={openCommentModal}
                                        />
                                    }
                                />
                                {showNotification && (
                                    <div className="iauto-notify-new-message">
                                        <Icon
                                            size="large"
                                            className={`comment`}
                                        />
                                    </div>
                                )}
                                <div style={{ display: "none" }}>
                                    <Popup
                                        content={"Change Processing User"}
                                        trigger={
                                            <Icon
                                                size="large"
                                                style={{
                                                    cursor: "pointer",
                                                    paddingLeft: "10px",
                                                }}
                                                className={`retweet`}
                                                onClick={
                                                    openProccessingUserModal
                                                }
                                            />
                                        }
                                    />
                                </div>
                            </div>
                        )}
                    </Table.Cell>
                </React.Fragment>
            )}

            {isVendor && (
                <React.Fragment>
                    <Table.Cell style={{ width: "5%" }}>{taskIcon}</Table.Cell>
                    <Table.Cell
                        style={{
                            // color: colorText,
                            fontWeight: "bold",
                        }}
                    >
                        <span style={{ cursor: "pointer" }} onClick={openModal}>
                            {title}
                        </span>
                    </Table.Cell>
                    <Table.Cell>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {projectName}
                        </span>
                    </Table.Cell>
                    <Table.Cell>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {formulaName}
                        </span>
                    </Table.Cell>
                    <Table.Cell style={{ width: "7%" }}>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {teamName}
                        </span>
                    </Table.Cell>
                    <Table.Cell style={{ width: "7%" }}>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {skillName}
                        </span>
                    </Table.Cell>
                    <Table.Cell style={{ width: "5%" }}>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {startDate && <TaskStartDate date={startDate} />}
                        </span>
                    </Table.Cell>
                    {/* <Table.Cell>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {dueDate && <TaskDueDate date={dueDate} />}
                        </span>
                    </Table.Cell> */}
                    {/* <Table.Cell textAlign="center">
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {completionTime}
                        </span>
                    </Table.Cell>
                    <Table.Cell textAlign="center">
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            0
                        </span>
                    </Table.Cell> */}
                    <Table.Cell textAlign="center" style={{ width: "7%" }}>
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={handleGoToProject}
                        >
                            {secondsRemaining > 0 ? (
                                <CountdownTimer
                                    secondsRemaining={secondsRemaining}
                                    //onPendingInviteRefresh={onPendingInviteRefresh}
                                    //taskId={pendingInvite.taskId}
                                    color={"#c3922e"}
                                    eta80={eta80}
                                    startDate={startDate}
                                    id={id}
                                    iS_PAST80_ETA_10={iS_PAST80_DEADLINE}
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
                        </span>
                    </Table.Cell>
                    <Table.Cell style={{ width: "3%" }}>
                        <TaskFooter
                            // dueDate={dueDate}
                            isOverdue={isOverdue}
                            isConditional={isConditional}
                            isRecurrent={isRecurrent}
                            isInterval={isInterval}
                        />
                    </Table.Cell>
                    {/* <Table.Cell style={{ width: "20%" }}>
                        <Dropdown
                            item
                            className="task-actions"
                            icon={<ModusIcon name="iauto--dropdown" />}
                        >
                            <Dropdown.Menu>
                                <Dropdown.Item onClick={openModal}>
                                    Details
                                </Dropdown.Item>
                            </Dropdown.Menu>
                        </Dropdown>
                    </Table.Cell> */}
                    <Table.Cell style={{ width: "3%" }}>
                        {renderNudgeButtons()}
                        {/* <input
                            type="button"
                            style={{ minWidth: "80px" }}
                            onClick={() => sendNotification(id)}
                            className="ui right floated button iauto purple btn-nudge"
                            value="NUDGE"
                        />
                        <br />
                        {!showCancelNudge && (
                            <input
                                type="button"
                                style={{ minWidth: "80px" }}
                                className="ui right floated button iauto red btn-nudge-cancel"
                                value="Cancel"
                                onClick={() =>
                                    onCancelNudgeNotification(id, price)
                                }
                            />
                        )} */}
                    </Table.Cell>
                </React.Fragment>
            )}
        </Table.Row>
    );
}
