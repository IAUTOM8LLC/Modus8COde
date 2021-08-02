import React, { Component } from 'react'
import { Form } from 'semantic-ui-react'
import DateTimePicker from 'react-widgets/lib/DateTimePicker'
import moment from 'moment'

import { onlyFutureDate } from '@utils/validators'

import { DateTimeInput, DurationInput, SimpleSegment } from '@components'

import IsAutomatedInput from './IsAutomatedInput'

export default class SchedulePane extends Component {
    state = {
        selectedOption: '1'
    }

    shouldUseDuration() {
        return this.state.selectedOption === '1';
    }

    componentDidMount() {
        this.props.changeField('useDateRangeAsDuration', false);
    }

    handleChange = (e, { value }) => {
        this.props.changeField('useDateRangeAsDuration', value === '0');
        this.setState({ selectedOption: value })
    }

    renderEditableDuration() {
        const { duration, dueDate, startDate } = this.props;

        const estimatedDuration = dueDate && startDate
            ? moment.duration(moment(dueDate).diff(moment(startDate))).asMinutes()
            : 0;

        const estimatedDueDate = startDate
            ? moment(startDate).add(duration, 'minutes').toDate()
            : null;

        if (this.shouldUseDuration()) {
            return (
                <Form.Field>
                    <DurationInput
                        min={1}
                        label="Duration"
                        name="duration"
                    />

                    <Form.Group inline>
                        <label>Estimated due date</label>
                        <DateTimePicker
                            className="modus-date-time"
                            readOnly
                            value={estimatedDueDate}
                            onChange={() => { }}
                        />
                    </Form.Group>
                </Form.Field>
            );
        } else {
            return (
                <div>
                    <DateTimeInput
                        label="Due Date"
                        required
                        validate={[onlyFutureDate]}
                        name="dueDate"
                    />

                    <Form.Group inline>
                        <label>Estimated dutaion</label>
                        <Form.Input readOnly value={estimatedDuration} />
                    </Form.Group>
                </div>
            );
        }
    }

    renderDuration() {
        const { duration, startDate } = this.props;

        const estimatedDueDate = startDate
            ? moment(startDate).add(duration, 'minutes').toDate()
            : null;

        return (
            <Form.Field>
                <DurationInput
                    disabled
                    label="Duration"
                    name="duration"
                />

                <label>Estimated due date</label>
                <DateTimePicker
                    className="modus-date-time"
                    readOnly
                    value={estimatedDueDate}
                    onChange={() => { }}
                />
            </Form.Field>
        );

    }

    render() {
        const { canEdit, formMeta } = this.props;

        const shouldValidateStartDate = canEdit && formMeta &&
            formMeta.startDate &&
            formMeta.startDate.touched &&
            formMeta.startDate.visited;

        return (
            <div className="project-details-modal__pane">
                <Form as="section" >
                    <SimpleSegment>
                        <div style={{ marginBottom: 20 }}>
                            <DateTimeInput
                                readOnly={!canEdit}
                                required
                                name="startDate"
                                label="Start Date"
                                validate={[shouldValidateStartDate && onlyFutureDate]}
                            />
                        </div>

                        {canEdit &&
                            <Form.Group>
                                <Form.Radio
                                    label="Duration"
                                    name="selectedOption"
                                    value="1"
                                    checked={this.shouldUseDuration()}
                                    onChange={this.handleChange}
                                />
                                <Form.Radio
                                    label="Due date"
                                    name="selectedOption"
                                    value="0"
                                    checked={!this.shouldUseDuration()}
                                    onChange={this.handleChange}
                                />
                            </Form.Group>
                        }

                        {canEdit ? this.renderEditableDuration() : this.renderDuration()}
                    </SimpleSegment>

                    <div style={{ marginTop: 20 }}>
                        <IsAutomatedInput
                            disabled={!canEdit}
                            isAutomated={this.props.isAutomated}
                            isConditional={this.props.isConditional}
                        />
                    </div>
                </Form>
            </div>
        );
    }
}
