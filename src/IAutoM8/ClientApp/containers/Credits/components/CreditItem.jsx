import React, { Component } from 'react'

import cn from 'classnames'

import { formatMoney } from '@utils/formatMoney'

import { Card } from 'semantic-ui-react'

import './CreditItem.less'

export default class CardItem extends Component {

    render() {
        const {
            title,
            type,
            availableCredits,
            lastAdded,
            prepaidTasksCount
        } = this.props;

        return (
            <li className="credit-card-item">
                <Card style={{ height: "100%" }}>
                    <Card.Content style={{ padding: 0 }}>
                        <Card.Header className={cn(["credit-item-header", `credit-item-header--${type}`])}>
                            {title}
                        </Card.Header>

                        <div className="credit-item-price">
                            $ {formatMoney(availableCredits, 2, ".", " ")}
                        </div>

                        {type === "available" &&
                            <div className="credit-item-footer">
                                {!lastAdded ? "You haven't made any transactions yet"
                                    : `Last credits added ${lastAdded}`}
                            </div>
                        }

                        {type === "reserved" &&
                            <div className="credit-item-footer">
                                {prepaidTasksCount} prepaid tasks
                            </div>
                        }
                    </Card.Content>
                </Card>
            </li>
        )
    }
}
