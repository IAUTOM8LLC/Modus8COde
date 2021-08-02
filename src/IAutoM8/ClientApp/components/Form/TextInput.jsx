import React from 'react'
import { Form, Input, TextArea } from 'semantic-ui-react'
import { Field } from 'redux-form'
import cn from 'classnames'

import { required as isRequired } from '@utils/validators'


const renderTextInput = ({
    maxLengthValue,
    label,
    placeholder,
    fieldClassName,
    icon,
    additionalClass,
    withValidationMessage,
    type,
    input,
    rows,
    meta: { touched, error },
    ...props
}) => {

    const hasError = touched && error !== undefined;

    const iconOptions = icon ? {
        icon,
        iconPosition: 'left'
    } : {};

    const elementProps = {
        type,
        placeholder,
        ...props,
        ...input,
        ...iconOptions
    }
    const shouldLabelRender = type === 'label' ? (elementProps.value ? true : false) : true;
    return (
        <Form.Field
            className={fieldClassName}
            error={hasError}
            required={false}>
            {
                label && shouldLabelRender &&
                <label>{label}</label>
            }
            {
                type === 'label'
                    ? shouldLabelRender
                        ? <div className={additionalClass}>{elementProps.value}</div>
                        : null
                    : type === 'textarea'
                        ? <TextArea {...elementProps} rows={rows} />
                        :
                        maxLengthValue !== undefined ? <Input
                            {...elementProps} max={maxLengthValue} min="1" />
                            : <Input {...elementProps} />                        
            }
            {
                withValidationMessage && hasError &&
                <span className={cn([additionalClass, 'error'])}>
                    {error}
                </span>
            }
        </Form.Field>
    );
}

export default function TextInput({
    required = false,
    validate = [],
    withValidationMessage = true,
    ...rest
}) {
    const validators = validate;

    if (required)
        validators.push(isRequired);

    return (
        <Field
            {...rest}
            withValidationMessage={withValidationMessage}
            component={renderTextInput}
            validate={validators}
        />
    );
}
