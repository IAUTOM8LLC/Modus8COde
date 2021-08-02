import React, { Component } from "react";
import { Table, Header } from "semantic-ui-react";
import sortBy from "lodash/sortBy";

import { toggleDirection } from "@utils/sort";

import TaskItem from "./TaskItem";

import "../HomeList.less";

export default class Stage extends Component {
    constructor(props) {
        super(props);

        this.state = {
            tasks: [],
            orderColumn: null,
            orderDirection: null,
        };

        this.onSort = this.onSort.bind(this);
    }

    componentDidMount() {
        this.setState({ tasks: this.props.tasks });
    }
    componentDidUpdate(prevProps) {
        // Typical usage (don't forget to compare props):
        if (this.props.tasks !== prevProps.tasks) {
            const { orderColumn, orderDirection } = this.state;
            if (orderColumn !== null && orderDirection !== null) {
                let orderedTasks = sortBy(this.props.tasks, orderColumn);
                if (orderDirection === "descending") {
                    orderedTasks = orderedTasks.reverse();
                }
                this.setState({ tasks: orderedTasks });
            } else {
                this.setState({ tasks: this.props.tasks });
            }
        }
        if (
            this.props.tasks.length !== prevProps.tasks.length &&
            this.props.tasks.length === 0
        ) {
            this.setState({ tasks: [] });
        }
    }

    onSort(event, sortKey) {
        const { tasks, orderColumn, orderDirection } = this.state;

        if (orderColumn !== sortKey) {
            this.setState({
                orderColumn: sortKey,
                tasks: sortBy(tasks, sortKey),
                orderDirection: "ascending",
            });
            return;
        }

        this.setState({
            tasks: tasks.reverse(),
            orderDirection: toggleDirection(orderDirection),
        });
    }

    render() {
        const { orderColumn, orderDirection } = this.state;

        return (
            <div>
                {!this.props.isVendor && (
                    <Table className="home-list" sortable compact>
                        <Table.Header>
                            <Table.Row>
                                <Table.HeaderCell
                                    style={{
                                        backgroundColor: "#f3f7fa",
                                    }}
                                    sorted={
                                        orderColumn === "status"
                                            ? orderDirection
                                            : null
                                    }
                                    onClick={(e) => this.onSort(e, "status")}
                                >
                                    Status
                                </Table.HeaderCell>
                                <Table.HeaderCell
                                    style={{
                                        backgroundColor: "#f3f7fa",
                                    }}
                                    sorted={
                                        orderColumn === "title"
                                            ? orderDirection
                                            : null
                                    }
                                    onClick={(e) => this.onSort(e, "title")}
                                >
                                    Task
                                </Table.HeaderCell>
                                {/* <Table.HeaderCell  style={{ backgroundColor: "#f3f7fa" }}>
                                    Status
                                </Table.HeaderCell> */}
                                <Table.HeaderCell
                                    style={{
                                        backgroundColor: "#f3f7fa",
                                    }}
                                    sorted={
                                        orderColumn === "formulaName"
                                            ? orderDirection
                                            : null
                                    }
                                    onClick={(e) =>
                                        this.onSort(e, "formulaName")
                                    }
                                >
                                    Formula
                                </Table.HeaderCell>
                                <Table.HeaderCell
                                    style={{
                                        backgroundColor: "#f3f7fa",
                                    }}
                                    sorted={
                                        orderColumn === "projectName"
                                            ? orderDirection
                                            : null
                                    }
                                    onClick={(e) =>
                                        this.onSort(e, "projectName")
                                    }
                                >
                                    Project
                                </Table.HeaderCell>
                                <Table.HeaderCell
                                    style={{
                                        backgroundColor: "#f3f7fa",
                                    }}
                                    sorted={
                                        orderColumn === "proccessingUserName"
                                            ? orderDirection
                                            : null
                                    }
                                    onClick={(e) =>
                                        this.onSort(e, "proccessingUserName")
                                    }
                                >
                                    Assigned
                                </Table.HeaderCell>
                                {/* <Table.HeaderCell  style={{ backgroundColor: "#f3f7fa" }}>
                                        Avg. TAT
                                    </Table.HeaderCell> */}
                                {/* <Table.HeaderCell  style={{ backgroundColor: "#f3f7fa" }}>
                                        Start Date
                                    </Table.HeaderCell> */}
                                <Table.HeaderCell
                                    textAlign="center"
                                    style={{
                                        backgroundColor: "#f3f7fa",
                                    }}
                                >
                                    Est. Turn Around
                                </Table.HeaderCell>
                                <Table.HeaderCell
                                    style={{
                                        backgroundColor: "#f3f7fa",
                                    }}
                                />
                            </Table.Row>
                        </Table.Header>

                        <Table.Body>
                            {this.state.tasks.map((task) => (
                                <TaskItem
                                    {...task}
                                    key={task.id}
                                    projects={this.props.projects}
                                    handleDoCard={
                                        this.props.handleDoCard
                                    }
                                    onReviewCard = {
                                        this.props.onReviewCard
                                    }
                                    onEditTask={this.props.onEditTask}
                                    onOpenComments={this.props.onOpenComments}
                                    onGoToProject={this.props.onGoToProject}
                                    onOpenUserProfile={
                                        this.props.onOpenUserProfile
                                    }
                                    onChangeProccessingUser={
                                        this.props.onChangeProccessingUser
                                    }
                                    isVendor={this.props.isVendor}
                                />
                            ))}
                            {this.state.tasks.length === 0 && (
                                <Table.Row>
                                    <Table.HeaderCell
                                        colSpan={10}
                                        style={{ height: 100 }}
                                    >
                                        <Header as="h2" icon textAlign="center">
                                            <Header.Content>
                                                No tasks found
                                            </Header.Content>
                                        </Header>
                                    </Table.HeaderCell>
                                </Table.Row>
                            )}
                        </Table.Body>
                    </Table>
                )}
                {this.props.isVendor && (
                    <li>
                        <ul className="inner-list">
                            <Table className="home-list" sortable>
                                <Table.Header>
                                    <Table.Row>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                        >
                                            STATUS
                                        </Table.HeaderCell>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "title"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "title")
                                            }
                                        >
                                            TASK NAME
                                        </Table.HeaderCell>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "projectName"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "projectName")
                                            }
                                        >
                                            PROJECT
                                        </Table.HeaderCell>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "formulaName"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "formulaName")
                                            }
                                        >
                                            FORMULA
                                        </Table.HeaderCell>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "teamName"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "teamName")
                                            }
                                        >
                                            TEAM
                                        </Table.HeaderCell>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "skillName"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "skillName")
                                            }
                                        >
                                            SKILL
                                        </Table.HeaderCell>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "startDate"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "startDate")
                                            }
                                        >
                                            START
                                        </Table.HeaderCell>
                                        {/* <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "dueDate"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "dueDate")
                                            }
                                        >
                                            DUE DATE
                                        </Table.HeaderCell> */}
                                        {/* <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "completionTime"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "completionTime")
                                            }
                                        >
                                            CT
                                        </Table.HeaderCell>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "tat"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "tat")
                                            }
                                        >
                                            TAT
                                        </Table.HeaderCell> */}
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                            sorted={
                                                orderColumn === "eta"
                                                    ? orderDirection
                                                    : null
                                            }
                                            onClick={(e) =>
                                                this.onSort(e, "eta")
                                            }
                                        >
                                            Deadline
                                        </Table.HeaderCell>
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                        />
                                        {/* <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                        /> */}
                                        <Table.HeaderCell
                                            style={{
                                                backgroundColor: "#f3f7fa",
                                            }}
                                        />
                                    </Table.Row>
                                </Table.Header>
                                <Table.Body>
                                    {this.state.tasks.map((task) => (
                                        <TaskItem
                                            {...task}
                                            onEditTask={this.props.onEditTask}
                                            filterUserId={
                                                this.props.filterUserId
                                            }
                                            onOutsourceTabOpen={
                                                this.props.onOutsourceTabOpen
                                            }
                                            handleDoCard={
                                                this.props.handleDoCard
                                            }
                                            handleReviewCard={
                                                this.props.handleReviewCard
                                            }
                                            onGoToProject={
                                                this.props.onGoToProject
                                            }
                                            projects={this.props.projects}
                                            key={task.id}
                                            sendNudgeNotification={
                                                this.props.sendNudgeNotification
                                            }
                                            onCancelNudgeNotification={
                                                this.props
                                                    .onCancelNudgeNotification
                                            }
                                            isVendor={this.props.isVendor}
                                        />
                                    ))}
                                    {this.state.tasks.length === 0 && (
                                        <Table.Row>
                                            <Table.HeaderCell
                                                colSpan={13}
                                                style={{ height: 100 }}
                                            >
                                                <Header
                                                    as="h2"
                                                    icon
                                                    textAlign="center"
                                                >
                                                    <Header.Content>
                                                        No tasks found
                                                    </Header.Content>
                                                </Header>
                                            </Table.HeaderCell>
                                        </Table.Row>
                                    )}
                                </Table.Body>
                            </Table>
                        </ul>
                    </li>
                )}
            </div>
        );
    }
}
