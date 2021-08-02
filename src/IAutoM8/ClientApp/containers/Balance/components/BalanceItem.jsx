import React, { Component } from 'react'

import cn from 'classnames'

import { formatMoney } from '@utils/formatMoney'

import { Card } from 'semantic-ui-react'

import './BalanceItem.less'

import moment from 'moment'

import TasksList from './TasksList'

import { ModusButton } from '@components'

export default class BalanceItem extends Component {

    render() {
        const {
            title,
            type,
            credits,
            lastAdded,
            tasks,
            tasksFinishedCount,
            handleTransferRequest,
            activeTransferRequest,
            loading,
            requestedAmount
        } = this.props;

        const buttonTitle = !activeTransferRequest ? "You have not finished request" : "";

        return (
            <li className="balance-card-item">
                <Card style={{ width: "100%" }}>
                    <Card.Content style={{ padding: 0 }}>
                        <Card.Header className={cn(["balance-item-header", `balance-item-header--${type}`])}>
                            {title}
                        </Card.Header>

                        <div className="balance-item-price">
                            $ {formatMoney(credits, 2, ".", " ")} <br />

                        </div>
                        {type === "unpaid" && <div style={{
                            marginTop: "20px",
                            marginBottom: "20px", display: "none"
                        }}>
                            <Card.Header style={{ fontWeight: "bold" }}
                                className={cn(["balance-item-header", `balance-item-header--${type}`])}>
                                Requested Amount
                        </Card.Header>

                            <div className="balance-item-price">
                                $ {formatMoney(requestedAmount, 2, ".", " ")}

                            </div>
                        </div>}
                        

                        {type === "unpaid" &&
                            <div>
                                <div className="balance-item-footer">
                                    {lastAdded != "Invalid date" && `Last transfer ${lastAdded}`}
                                    {lastAdded == "Invalid date" && "You haven't had any transactions yet"}
                                </div>

                                {activeTransferRequest &&
                                    <div className="balance-item-footer">
                                        <div>Last transfer request: {moment(activeTransferRequest.requestTime)
                                            .format('MMM DD, YYYY  h:mmA')}</div>
                                        <div>Requested amount: {activeTransferRequest.requestedAmountWithTax}
                                        </div>
                                    </div>
                                }

                                <ModusButton
                                    filled
                                    className="balance-button"
                                    content="Request transfer funds"
                                    disabled={activeTransferRequest ||
                                        !!(credits != undefined && credits === 0)}
                                    title={buttonTitle}
                                    loading={loading}
                                    onClick={handleTransferRequest} />
                            </div>
                        }

                        {type === "expected" &&
                            <div>
                                <div className="balance-item-footer">
                                    {tasks !== undefined ? tasks.length : 0} tasks
                                in progress or waiting for review
                                </div>

                                <TasksList tasks={tasks} />
                            </div>
                        }

                        {type === "total" &&
                            <div>
                                <div className="balance-item-footer">
                                    {tasksFinishedCount} tasks finished successfully
                                </div>
                            </div>
                        }

                    </Card.Content>
                </Card>
            </li>
        )
    }
}
