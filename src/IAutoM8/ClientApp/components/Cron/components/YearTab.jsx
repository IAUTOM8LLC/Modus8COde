import React from 'react'
import { Form, Segment } from 'semantic-ui-react'

import { months, days, daysOrder } from '../data'

import StartTime from './StartTime'

export default function YearTab({ values, onChange,
    onChangeAsap }) {
    const {
        selectedOption,
        everyDay,
        everyMonth,
        whichDay,
        whichMonth,
        whichDayOrder,
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
                    <Form.Dropdown
                        label="Every"
                        selection
                        placeholder="Hour"
                        name="everyMonth"
                        value={everyMonth}
                        options={months}
                        onChange={onChange}
                    />
                    <Form.Input
                        name="everyDay"
                        value={everyDay}
                        label="in day #"
                        min={1}
                        max={31}
                        type="number"
                        onChange={onChange}
                    />
                </Form.Group>

                <Form.Group inline>
                    <Form.Radio
                        name="selectedOption"
                        value="1"
                        checked={selectedOption === '1'}
                        onChange={onChange}
                    />
                    <Form.Dropdown
                        label="The"
                        selection
                        name="whichDayOrder"
                        value={whichDayOrder}
                        options={daysOrder}
                        placeholder="Hour"
                        onChange={onChange}
                    />
                    <Form.Dropdown
                        selection
                        name="whichDay"
                        value={whichDay}
                        options={days}
                        placeholder="Hour"
                        onChange={onChange}
                    />
                    <Form.Dropdown
                        label="of"
                        selection
                        name="whichMonth"
                        value={whichMonth}
                        options={months}
                        placeholder="Hour"
                        onChange={onChange}
                    />
                </Form.Group>

                <StartTime onChange={onChange} startTime={startTime} onChangeAsap={onChangeAsap}/>
            </Form>
        </Segment>
    )
}
