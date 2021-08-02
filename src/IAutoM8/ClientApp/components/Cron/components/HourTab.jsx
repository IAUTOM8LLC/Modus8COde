import React from 'react'
import { Form, Segment } from 'semantic-ui-react'

import StartTime from './StartTime'

export default function HourTab({
    values: { hours, startTime, selectedOption },
    onChange,
    onChangeAsap
}) {
    return (
        <Segment basic>
            <Form as="section">
                <Form.Group inline>
                    <Form.Radio
                        name="selectedOption"
                        value="0"
                        checked={selectedOption === '0'}
                        onChange={(e, data) => { onChange(e, data); onChangeAsap(false); }}
                    />
                    <Form.Input
                        name="hours"
                        value={hours}
                        label="Every"
                        max={24}
                        min={1}
                        type="number"
                        onChange={onChange}
                    />
                    <label>hour(s)</label>
                </Form.Group>

                <Form.Group inline>
                    <Form.Radio
                        name="selectedOption"
                        value="1"
                        checked={selectedOption === '1'}
                        onChange={(e, data) => {
                            onChange(e, data); onChangeAsap(true);
                        }}
                    />
                    <label>Start time</label>
                    <StartTime onChange={onChange} startTime={startTime} noLabel onChangeAsap={onChangeAsap}/>
                </Form.Group>
            </Form>
        </Segment>
    )
}
