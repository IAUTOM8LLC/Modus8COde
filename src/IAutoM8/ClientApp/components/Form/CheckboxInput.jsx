import React from 'react'
import { Form, Checkbox } from 'semantic-ui-react'
import { Field } from 'redux-form'

function renderCheckbox(field) {
    /* eslint-disable no-unused-vars */
    const {
        label,
        input: {
            value, // dustruct value just to exclude it from rest props
            onChange,
            ...inputRest
        },
        meta,
        ...rest
    } = field;
    /* eslint-enable no-unused-vars */

    const inputProps = {
        ...rest,
        ...inputRest
    }

    return (
        <Form.Field>
            <Checkbox
                {...inputProps}
                label={label}
                onBlur={() => { /* looks like checkbox dispathes broken data on blur somewhy */ }}
                onChange={(e, { checked }) => onChange(checked)}
            />
        </Form.Field>
    );
}

export default function CheckboxInput(props) {
    return (
        <Field
            {...props }
            type="checkbox"
            component={renderCheckbox}
        />
    );
}
