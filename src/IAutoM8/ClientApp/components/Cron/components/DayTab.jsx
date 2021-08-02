import React from 'react'
import { Form, Segment } from 'semantic-ui-react'

import StartTime from './StartTime'

import './DayTab.less'

export default function DayTab({
    values: { selectedOption, days, startTime },
    onChange, onChangeAsap
}) {
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
                        name="days"
                        value={days}
                        label="Every"
                        min={1}
                        max={365}
                        type="number"
                        onChange={onChange}
                    />
                    <label>day(s)</label>
                    <StartTime className="start-time" onChange={onChange} startTime={startTime}
                        onChangeAsap={onChangeAsap} />
                </Form.Group>

                <Form.Group inline>
                    <Form.Radio
                        name="selectedOption"
                        value="1"
                        checked={selectedOption === '1'}
                        onChange={onChange}
                    />
                    <label>Every day</label>
                </Form.Group>
            </Form>
        </Segment>
    )
}
