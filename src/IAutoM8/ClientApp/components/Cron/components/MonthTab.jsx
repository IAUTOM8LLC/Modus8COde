import React from 'react'
import { Form, Segment } from 'semantic-ui-react'

import { daysOrder, days } from '../data'

import StartTime from './StartTime'

export default function MonthTab({ values, onChange,
    onChangeAsap }) {
    const {
        selectedOption,
        allDay,
        allMonth,
        month,
        day,
        dayOrder,
        startTime
        } = values;
    return (
        <Segment basic>
            <Form as="section">
                <Form.Group inline>
                    <Form.Radio
                        name="selectedOption"
                        value="0"
                        checked={selectedOption === '0'}
                        onChange={onChange}
                    />
                    <Form.Input
                        name="allDay"
                        value={allDay}
                        label="Day"
                        min={1}
                        max={31}
                        type="number"
                        onChange={onChange}
                    />
                    <Form.Input
                        name="allMonth"
                        value={allMonth}
                        label="of every"
                        min={1}
                        max={12}
                        type="number"
                        onChange={onChange}
                    />
                    <label>month(s)</label>
                </Form.Group>

                <Form.Group inline>
                    <Form.Radio
                        name="selectedOption"
                        value="1"
                        checked={selectedOption === '1'}
                        onChange={onChange}
                    />
                    <Form.Dropdown
                        selection
                        name="dayOrder"
                        value={dayOrder}
                        options={daysOrder}
                        onChange={onChange}
                    />
                    <Form.Dropdown
                        selection
                        name="day"
                        value={day}
                        options={days}
                        onChange={onChange}
                    />
                    <Form.Input
                        value={month}
                        name="month"
                        label="of every"
                        min={1}
                        max={12}
                        type="number"
                        onChange={onChange}
                    />
                    <label>month(s)</label>
                </Form.Group>

                <StartTime onChange={onChange} startTime={startTime} onChangeAsap={onChangeAsap} />
            </Form>
        </Segment>
    )
}
