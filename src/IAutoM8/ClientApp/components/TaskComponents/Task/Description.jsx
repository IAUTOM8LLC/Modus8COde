import React from 'react'

const descriptionHtml = (description) => {
    if (!description)
        return null;

    const maxLen = 300;

    // remove possible image tags from description 
    const rowHtml = description.replace(/<(img|iframe)[^>]*>/g, '');

    // limit description max length
    const html = {
        __html: rowHtml.length > maxLen
            ? `<div>${rowHtml.substring(0, maxLen)}</div>`
            : rowHtml
    };

    return (
        <span>
            <span dangerouslySetInnerHTML={html} />
            {rowHtml.length > maxLen && <span>...</span>}
        </span>
    )
}

export default function TaskDescription({ description }) {

    return (
        <div className="calendar-event__item-description">
            {descriptionHtml(description)}
        </div>
    );
}