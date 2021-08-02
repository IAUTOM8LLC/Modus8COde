import React from 'react'
import { Dropdown } from 'semantic-ui-react'

import './styles/CalendarViewSelect.less'

export default function CalendarViewSelect({ view = 'month', onChangeView }) {

    const options = [
        {
            key: 0,
            value: 'month',
            text: 'Monthly View'
        },
        {
            key: 1,
            value: 'week',
            text: 'Weekly View'
        },
        {
            key: 2,
            value: 'day',
            text: 'Daily View'
        }
    ];

    return (
        <div className="calendar-view-select">
            <Dropdown
                options={options}
                defaultValue={view}
                onChange={onChangeView}
            />
        </div>
    )
}