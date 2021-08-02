import React from 'react'
import { Form, Grid, Segment } from 'semantic-ui-react'

import StartTime from './StartTime'

function MultiCheckbox({ value, label, options, onChange }) {
    return (
        <Form.Checkbox
            name="checkedDays"
            value={value}
            label={label}
            checked={options.indexOf(value) !== -1}
            onChange={onChange}
        />
    );
}

export default function WeekTab({
    values: { checkedDays, startTime },
    onChange, onChangeAsap
}) {
    return (
        <Segment basic>
            <Form as="section">
                <Form.Field>
                    <label>Days of week</label>
                    <Grid columns={3} style={{ maxWidth: 500 }}>
                        <Grid.Row columns={3}>
                            <Grid.Column>
                                <Form.Group grouped>
                                    <MultiCheckbox
                                        value="MON"
                                        label="Monday"
                                        onChange={onChange}
                                        options={checkedDays}
                                    />
                                    <MultiCheckbox
                                        value="TUE"
                                        label="Tuesday"
                                        onChange={onChange}
                                        options={checkedDays}
                                    />
                                    <MultiCheckbox
                                        value="WED"
                                        label="Wednesday"
                                        onChange={onChange}
                                        options={checkedDays}
                                    />
                                </Form.Group>
                            </Grid.Column>
                            <Grid.Column>
                                <Form.Group grouped>
                                    <MultiCheckbox
                                        value="THU"
                                        label="Thursday"
                                        onChange={onChange}
                                        options={checkedDays}
                                    />
                                    <MultiCheckbox
                                        value="FRI"
                                        label="Friday"
                                        onChange={onChange}
                                        options={checkedDays}
                                    />
                                </Form.Group>
                            </Grid.Column>
                            <Grid.Column>
                                <Form.Group grouped>
                                    <MultiCheckbox
                                        value="SAT"
                                        label="Saturday"
                                        onChange={onChange}
                                        options={checkedDays}
                                    />
                                    <MultiCheckbox
                                        name="checkedDays"
                                        value="SUN"
                                        label="Sunday"
                                        onChange={onChange}
                                        options={checkedDays}
                                    />
                                </Form.Group>
                            </Grid.Column>
                        </Grid.Row>
                    </Grid>
                </Form.Field>

                <StartTime onChange={onChange} startTime={startTime} onChangeAsap={onChangeAsap}/>
            </Form>
        </Segment>
    )
}
