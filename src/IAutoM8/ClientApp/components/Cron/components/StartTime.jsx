import React from 'react'
import { Form } from 'semantic-ui-react'

import { startTimeOptions } from '../data'

export default function StartTime({ onChange, startTime, noLabel = false,
    className, onChangeAsap }) {

    return (
        <Form.Field className={className}>
            {
                !noLabel &&
                <label>Start time</label>
            }
            <Form.Dropdown
                selection
                name="startTime"
                placeholder="Start ASAP"
                options={startTimeOptions(startTime)}
                value={startTime}
                onChange={(e, data) => {
                    onChange(e, data); onChangeAsap(false);
                }}
            />
        </Form.Field>
    )
}
