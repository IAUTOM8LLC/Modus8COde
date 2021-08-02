import React from 'react'
import { Radio, Form } from 'semantic-ui-react'
import { Field } from 'redux-form'

function renderRadio(field) {
    /* eslint-disable no-unused-vars */
    const {
        label,
        input: {
            checked,
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
    };

    return (
        <Form.Field>
            <Radio
                checked={checked}
                {...inputProps}
                label={label}
                onChange={(e, { value }) => { onChange(value) }}
            />
        </Form.Field>
    );
}

export default function RadioInput(props) {
    return (
        <Field
            type="radio"
            {...props }
            component={renderRadio}
        />
    );
}
