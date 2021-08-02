import React from 'react'
import BaseTimeAgo from 'react-timeago'
import moment from 'moment'


const baseFormatter = (value, unit, suffix) => {
    if (value !== 1) {
        unit += 's';
    }

    if (suffix === 'ago') {
        return `${value} ${unit} ${suffix}`;
    } else {
        return `In ${value} ${unit}`;
    }
}

export default function TimeAgo({
    date,
    span = 1,
    type = 'd',
    format = 'MMM DD, YYYY  h:mmA',
    formatter = baseFormatter,
    className = null
}) {
    if (!date)
        return null;

    const dateIsBetweenPerion = moment(date).isBetween(
        moment().subtract(span, type),
        moment().add(span, type),
        'minute'
    );

    return dateIsBetweenPerion
        ? <BaseTimeAgo date={date} minPeriod={20} formatter={formatter} className={className}/>
        : <span className={className}>{moment(date).format(format)}</span>;
}
