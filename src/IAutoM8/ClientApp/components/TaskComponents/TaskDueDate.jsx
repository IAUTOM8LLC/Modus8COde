import React from 'react'
import moment from 'moment'

import { TimeAgo, ModusIcon } from '@components'

import './styles/TaskDueDate.less'

export default function TaskDueDate({ date }) {
    const momentDate = moment(date);

    const formatter = (value, unit, suffix) => {
        if (momentDate.isBetween(moment().subtract(1, 'minute'), moment().add(1, 'minute')))
            return 'Now';

        if (value !== 1) {
            unit += 's';
        }

        if (suffix === 'ago') {
            return `${value} ${unit} ${suffix}`;
        } else {
            return `Due in ${value} ${unit}`;
        }
    }

    return (
        <div className="task-due-date">
            <ModusIcon name="iauto--clock" />
            <TimeAgo date={date} format="MMM DD h:mm a" formatter={formatter} />
        </div>
    );
}