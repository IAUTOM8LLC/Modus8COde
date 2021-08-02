import React from 'react'

import { ModusIcon } from '@components'

export default function TypeIcon({ isInterval, isConditional, isRecurrent, popup }) {

    const taskTypes = {
        'interval': 'Interval task',
        'conditional-recurring': 'Conditional-Recurring task',
        'conditional': 'Conditional task',
        'one-time': 'One-time task'
    }

    const taskTypeIcon = isInterval
        ? 'interval'
        : isRecurrent
            ? isConditional
                ? 'conditional-recurring'
                : 'recurring'
            : isConditional
                ? 'conditional'
                : 'one-time';

    const text = taskTypes[taskTypeIcon];
    const icon = `task ${taskTypeIcon}`;

    if (popup)
        return <ModusIcon name={icon} popup={text} />

    return [
        <ModusIcon name={icon} key={1} />,
        <span key={2}>{text}</span>
    ];
}