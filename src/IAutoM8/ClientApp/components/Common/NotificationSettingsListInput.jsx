import React from 'react'
import { Form, Grid } from 'semantic-ui-react'
import { FieldArray, Field } from 'redux-form'
import { Icon, Message, Input, Checkbox } from 'semantic-ui-react'

import notificationSettings from '@utils/notificationSettings'

import './NotificationSettingsListInput.less'

const renderCheckbox = ({ input, label }) => {
    return (
        <Checkbox
            checked={input.value}
            onChange={(event, { checked }) => input.onChange(checked)}
            onBlur={() => { /* looks like checkbox dispathes broken data on blur somewhy */ }}
            label={<label>{label}</label>}
        />
    );
}

const renderField = ({ input, label, settingFieldName, settingType, enabledName }) => {
    return (
        <Input
            type="text"
        >
            <input {...input} type="hidden" />
            <input name={settingFieldName} value={settingType} type="hidden" />

            <Field
                name={enabledName}
                label={label}
                component={renderCheckbox}
            />
        </Input>
    );
}

const renderChainInputs = ({ fields, meta: { error } }) => {
    return (
        <Grid columns={2} className="notification-settings-list-input__grid">
            <Grid.Row>
                {
                    fields.map((field, index, fields) => {
                        const setting = fields.get(index);
                        const label = notificationSettings(setting.type);
                        return (
                            <Grid.Column key={index}>
                                <Field
                                    name={`${field}.id`}
                                    value={setting.id}
                                    settingFieldName={`${field}.type`}
                                    enabledName={`${field}.enabled`}
                                    label={label}
                                    settingType={setting.type}
                                    component={renderField} />
                            </Grid.Column>
                        );
                    })
                }
                {
                    error &&
                    <Message visible warning >
                        <Icon name="help" />
                        {error}
                    </Message>
                }
            </Grid.Row>
        </Grid>
    );
}

export default function NotificationSettingsListInput(props) {

    return (
        <Form.Field className="notification-settings-list-input">
            <FieldArray
                {...props}
                component={renderChainInputs}
            />
        </Form.Field>
    );
}
