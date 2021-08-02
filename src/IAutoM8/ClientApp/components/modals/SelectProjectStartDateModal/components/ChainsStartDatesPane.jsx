import React from 'react'
import { Form } from 'semantic-ui-react'
import { FieldArray, Field } from 'redux-form'
import { List, Icon, Message, Input } from 'semantic-ui-react'

import { DateTimeInput, TextInput } from '@components'

import { required, onlyFutureDate } from '@utils/validators'

const renderField = ({ input, dateImputName, taskName }) => {
    return (
        <Input>
            <TextInput
                type="hidden"
                {...input}
            />
            <DateTimeInput
                name={dateImputName}
                label={taskName}
                validate={[required, onlyFutureDate]}
            />
        </Input>
    )
}

const renderChainInputs = ({ fields, meta: { error }, rootTasks }) => {
    return (
        <List>
            {
                fields.map((field, index, fields) => {
                    const task = rootTasks.find(t => t.id == fields.get(index).key);
                    let parsedId = task.id;
                    if (task.parentTaskId) {
                        parsedId = Number(task.id.replace(task.parentTaskId, ''));
                    }
                    return (
                        <List.Item key={index}>
                            <Field
                                name={`${field}.key`}
                                value={task.id}
                                component={renderField}
                                dateImputName={`${field}.value`}
                                taskName={task.title} />
                        </List.Item>
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
        </List>
    );
}

export default function ChainsStartDatesPane({ rootTasks, ...rest }) {
    return (
        <Form.Field>
            <FieldArray
                name="rootStartDateTime"
                rootTasks={rootTasks}
                component={renderChainInputs}
                {...rest} />
        </Form.Field>
    );
}
