import React, { Component } from 'react'

import { formatMoney } from '@utils/formatMoney'

import './TasksList.less'

export default class TasksList extends Component {
    render() {

        const {
            tasks
        } = this.props;

        return (
            <ul className="tasks-container">
                {tasks &&
                    tasks.map(task =>
                        <li className="task-item" key={task.name}>
                        <span className="task-item-price">$ {formatMoney(task.price, 2, ".", " ")} - </span>
                        <span>{task.name}</span>
                        </li>
                    )
                }
            </ul>
        );
    }
}
