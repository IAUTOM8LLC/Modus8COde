import React from 'react'
import { Form, Segment } from 'semantic-ui-react'

export default function MinuteTab({ values: { minutes }, onChange }) {
    return (
        <Segment basic>
            <Form as="section">
                <Form.Group inline>
                    <Form.Input
                        name="minutes"
                        value={minutes}
                        label="Every"
                        min={1}
                        max={59}
                        type="number"
                        onChange={onChange}
                    />
                    <label>minute(s)</label>
                </Form.Group>
            </Form>
        </Segment>
    )
}
