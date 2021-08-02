import React, { Component } from 'react'
import { Form, Header } from 'semantic-ui-react'
import { FormSection, Field } from 'redux-form'

import { onlyFutureDate } from '@utils/validators'

import {
    Cron,
    TextInput,
    DateTimeInput,
    DropdownInput,
    DurationInput,
    SimpleSegment
} from '@components'

/**
 * Small inner component for 2 purposes:
 * 1) avoid dynamic lambda onChange handler redraw Cron component on every render
 * 2) manually dispatch changeField to avoid nested cron:{cron, cronTab} form data
 */
class CronField extends Component {
    static defaultProps = { changeField: () => { } }

    handleChange = (data) => {
        if (!this.props.disabled) {
            this.props.changeField('recurrenceOptions.cronTab', data.cronTab);
            this.props.changeField('recurrenceOptions.dayDiff', data.data.dayDiff);
            this.props.input.onChange(data.data.cron);
        }
    }

    render = () =>
        <Cron
            disabled={this.props.disabled}
            tab={this.props.cronTab}
            defaultCron={this.props.input.value}
            onChange={this.handleChange}
        />
}

export default function RecurrencePane({
    changeField,
    cronTab,
    recurrenceType,
    isFormulaTask,
    isAutomated,
    isConditional,
    canEdit
}) {

    const recurrenceOptions = [
        { key: 'EndNever', value: 0, text: 'Never' },
        { key: 'EndAfterCertainAmount', value: 1, text: 'After occurrences' },
        { key: 'EndOnDate', value: 2, text: 'On date' }
    ];

    return (
        <div className="task-details-modal__pane">
            <Form as="section" style={{ minHeight: 450 }}>
                <Header as="h3" >
                    When should this task recur ?
                </Header>
                <SimpleSegment>
                    <Form.Field>
                        <Field
                            disabled={!canEdit}
                            readOnly={!canEdit}
                            name="recurrenceOptions.cron"
                            component={CronField}
                            changeField={changeField}
                            cronTab={cronTab}
                        />
                    </Form.Field>
                </SimpleSegment>

                <Header as="h3" >
                    Recurrence options
                </Header>
                <SimpleSegment>
                    <DurationInput
                        min={1}
                        disabled={!canEdit}
                        readOnly={!canEdit}
                        label="Duration"
                        name="duration"
                    />
                    {
                        isFormulaTask &&
                        <DurationInput
                            readOnly={!canEdit}
                            name="startDelay"
                            label="Start delay"
                        />
                    }
                    <FormSection name="recurrenceOptions">
                        <DropdownInput
                            disabled={!canEdit}
                            readOnly={!canEdit}
                            label="Reccurrence Type"
                            name="recurrenceType"
                            options={recurrenceOptions}
                        />
                        {
                            recurrenceType === 1 &&
                            <TextInput
                                disabled={!canEdit}
                                readOnly={!canEdit}
                                label="Max Occurrences"
                                name="maxOccurrences"
                                type="number"
                                min={1}
                            />
                        }
                        {
                            recurrenceType === 2 &&
                            <DateTimeInput
                                label="End Recurrence Date"
                                readOnly={!canEdit}
                                /*TODO: date should be greater than at least start date*/
                                validate={[canEdit && onlyFutureDate]}
                                name="endRecurrenceDate"
                                required
                            />
                        }
                    </FormSection>
                </SimpleSegment>
            </Form>
        </div>
    );
}


