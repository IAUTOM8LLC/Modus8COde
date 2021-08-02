import React from 'react'
import { Dropdown, Form } from 'semantic-ui-react'
import { Field } from 'redux-form'

import { ModusButton } from '@components'

import cn from 'classnames'

import './SkillsDropdownInput.less'

function ensureArray(value) {
    return Array.isArray(value) ? value : [];
}

function renderDropdown({
    label,
    placeholder,
    onDeleteOutsorsers,
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

    const fieldStyle = {
        paddingTop: "20px"
    };

    return (
        <Form.Field style={fieldStyle} error={hasError}>
            {
                label &&
                <label>{label}</label>
            }
            <Dropdown
                className="dropdown-outsource"
                {...rest}
                {...inputProps}
                selection
                options={options}
                placeholder={placeholder}
                value={rest.multiple ? ensureArray(input.value) : input.value}
                onChange={(event, data) => {
                    onDeleteOutsorsers(data.value);
                }}
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

export default function SkillsDropdownInput({
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
