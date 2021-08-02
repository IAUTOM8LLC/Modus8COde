import React from 'react'

export default function TimeEstimate({
    minutes, className }) {
    const minute = Math.ceil(minutes % 60);
    const hours = Math.floor(minutes / 60);
    const hour = Math.round(hours % 24);
    const day = Math.floor(hours / 24);
    const text = (day > 0 ? `${day}d ` : '') +
        (hour > 0 ? `${hour}h ` : '') +
        (minute > 0 ? `${minute}m ` : '');
    return <span className={className}>{text.trimRight()}</span>;
}
