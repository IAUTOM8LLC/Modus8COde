import React from 'react'
import { Form } from 'semantic-ui-react'
import { Field } from 'redux-form'
import moment from 'moment'
import DateTimePicker from 'react-widgets/lib/DateTimePicker'
import momentLocalizer from 'react-widgets-moment'
import cn from 'classnames'

import { required as isRequired } from '@utils/validators'

import './DateTimeInput.less'

momentLocalizer();

function renderDateTimePicker({
    format,
    withoutTime = false,
    withoutDate = false,
    label,
    input,
    additionalClass,
    withValidationMessage = true,
    meta: { touched, error },
    ...rest
}) {
    const hasError = touched && error !== undefined;
    const {
        value,
        ...inputProps
    } = input;

    const setValue = (value) => {
        if (!value)
            return null;

        let dateTime = new Date(value);
        if (isNaN(dateTime.getTime())) {
            const dateStr = '2001-01-01';
            const date = moment(dateStr);
            const time = moment(value, 'hh:mm A');

            date.set({
                hour: time.get('hour'),
                minute: time.get('minute')
            });

            return date.toDate();
        }
        return dateTime;
    }

    return (
        <Form.Field error={hasError}>
            {
                label &&
                <label>{label}</label>
            }
            <DateTimePicker
                className="modus-date-time"
                {...rest}
                {...inputProps}
                format={format}
                time={!withoutTime}
                date={!withoutDate}
                value={setValue(value)}
                defaultCurrentDate={new Date(moment().add(5, 'm').format('MMM D, YYYY h:mm A'))}
            />
            {
                withValidationMessage && hasError &&
                <span className={cn([additionalClass, 'error'])}>
                    {error}
                </span>
            }
        </Form.Field>
    );
}

export default function DateTimeInput({ required = false, validate = [], ...props }) {
    if (required)
        validate.push(isRequired);

    return (
        <Field
            {...props}
            validate={validate.filter(t => t)}
            component={renderDateTimePicker}
        />
    );
}
