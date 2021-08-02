import React from 'react'
import { Dropdown, Form } from 'semantic-ui-react'
import { Field } from 'redux-form'
import cn from 'classnames'

function ensureArray(value) {
    return Array.isArray(value) ? value : [];
}

function renderDropdown({
    label,
    placeholder,
    options,
    input,
    additionalClass,
    withValidationMessage,
    meta: { touched, error },
    ...rest
}) {
    const {
        onChange,
        onBlur, // eslint-disable-line no-unused-vars
        ...inputProps
    } = input;

    const hasError = touched && error !== undefined;

    return (
        <Form.Field error={hasError}>
            {
                label &&
                <label>{label}</label>
            }
            <Dropdown
                {...rest}
                {...inputProps}
                selection
                options={options}
                placeholder={placeholder}
                value={rest.multiple ? ensureArray(input.value) : input.value}
                onChange={(event, data) => onChange(data.value)}
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

export default function DropdownInput({
    withValidationMessage = false,
    ...props
}) {
    if (props.validate && props.validate.length)
        withValidationMessage = true;
    return (
        <Field
            {...props}
            withValidationMessage={withValidationMessage}
            component={renderDropdown}
        />
    );
}
