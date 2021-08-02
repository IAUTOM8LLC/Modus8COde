import React, { Component } from 'react'
import { Form, Input, List, Icon, Message, Select, Button } from 'semantic-ui-react'
import { FormSection, FieldArray, Field } from 'redux-form'

import { TextInput } from '@components'

const ifDuplicateExist = (list)=> {
    const counts = [];
    for (let i = 0; i <= list.length; i++) {
        if (counts[list[i]] === undefined) {
            counts[list[i]] = 1;
        } else {
            return true;
        }
    }
    return false;
}
const validate = (options = []) => {
    if (options.length < 2)
        return 'You must provide at least two options';

    // check for duplicates
    const ids = options.map(t => t.assignedTaskId);
    const hasDuplicates = ids.some((id, idx) =>
        id > 0 && ids.indexOf(id) !== idx);

    if (hasDuplicates)
        return 'You cannot assign same tasks multiple times';

    const list = options.map(t => t.option);
    if (ifDuplicateExist(list))
        return 'You cannot assign same options multiple times';

    return undefined;
}

export default class ConditionalPane extends Component {
    state = {
        newOption: ''
    }

    handleChange = (event) => {
        this.setState({ newOption: event.target.value })
    }

    handlePush = (fields) => {
        let { newOption = '' } = this.state;
        newOption = newOption.trim();
        if (newOption === '') {
            return;
        }
        fields.push({
            option: newOption
        });
        this.setState({ newOption: '' })
    }
    renderSelect = ({ input }) => {
        return (
            <Select
                disabled={!this.props.canEdit}
                compact
                style={{ width: 180, border: 'none' }}
                options={[
                    { key: 'null', text: 'None', value: 0 },
                    ...this.props.tasks.map(t => ({
                        key: t.id,
                        text: t.title,
                        value: t.id
                    }))
                ]}
                value={input.value}
                onChange={(event, data) => input.onChange(data.value)}
                placeholder="Assigned task"
            />
        )
    }

    renderField = ({ handleClick, input, selectName }) => {
        return (
            <Input
                action
                icon
                iconPosition="left"
                type="text"
            >
                <Icon name="help" />
                <input {...input} />

                <Field
                    name={selectName}
                    component={this.renderSelect}
                />

                {handleClick &&
                    <Button as="a" icon="delete" onClick={handleClick} />
                }
            </Input>
        )
    }

    renderOptions = ({ fields }) => {
        return (
            <List>
                {
                    fields.map((option, index) => (
                        <List.Item key={index}>
                            <Field
                                name={`${option}.option`}
                                type="text"
                                component={this.renderField}
                                placeholder="Option"
                                selectName={`${option}.assignedTaskId`}
                            />
                        </List.Item>
                    ))
                }
            </List>
        )
    }

    renderEditableOptions = ({ fields, meta: { error } }) => {
        return (
            <List>
                <List.Item>
                    <Input
                        placeholder="Add new option"
                        input={<input value={this.state.newOption} onChange={this.handleChange} />}
                        action={{
                            as: 'a',
                            color: 'teal',
                            content: 'Add option',
                            onClick: () => { this.handlePush(fields) }
                        }}
                    />
                </List.Item>
                {
                    fields.map((option, index) => (
                        <List.Item key={index}>
                            <Field
                                name={`${option}.option`}
                                type="text"
                                component={this.renderField}
                                placeholder="Option"
                                handleClick={() => fields.remove(index)}
                                selectName={`${option}.assignedTaskId`}
                            />
                        </List.Item>
                    ))
                }
                {
                    error &&
                    <Message visible warning >
                        <Icon name="help" />
                        {error}
                    </Message>
                }
            </List>
        )
    }

    render() {
        const fieldProps = this.props.canEdit
            ? { validate, component: this.renderEditableOptions }
            : { component: this.renderOptions };

        return (
            <div className="task-details-modal__pane" >
                <FormSection name="condition">
                    <Form as="section">
                        <TextInput
                            required
                            type="textarea"
                            name="condition"
                            label="Condition"
                        />
                        {/* TODO: some white magic is going on here, check submit */}
                        <Form.Field>
                            <label>Options</label>
                            <FieldArray
                                name="options"
                                {...fieldProps}
                            />
                        </Form.Field>
                    </Form>
                </FormSection>
            </div>
        );
    }
}
