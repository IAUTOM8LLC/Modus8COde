import React, { Component } from 'react'
import { Grid, Form, Input } from 'semantic-ui-react'
import { Field } from 'redux-form'
import cn from 'classnames'

import './DurationInput.less'

class RenderInputs extends Component {
    constructor(props) {
        super(props);
        this.handleMinutesChange = this.handleChange.bind(this, 'minutes');
        this.handleHoursChange = this.handleChange.bind(this, 'hours');
        this.handleDaysChange = this.handleChange.bind(this, 'days');

        this.state = this.split(props.value);
    }

    componentDidUpdate(prevProps) {
        const { days, hours, minutes } = this.state;

        if (this.props.value !== prevProps.value || this.join(days, hours, minutes) !== this.props.value) {
            this.setState(this.split(this.getValue()))
        }
    }

    getValue() {
        const { value } = this.props;
        return Number(value);
    }

    split(totalMinutes) {
        const days = Math.trunc(totalMinutes / (24 * 60));
        const hours = Math.trunc(totalMinutes / 60 - days * 24);
        const minutes = totalMinutes - days * 24 * 60 - hours * 60;
        return { days, hours, minutes };
    }

    join(d, h, m) {
        return (d * 24 + h) * 60 + m;
    }

    handleChange = (type, e, { value }) => {
        if (this.props.disabled) {
            return;
        }
        const { onChange } = this.props;
        this.setState({ [type]: Number(value) }, () => {
            const { days, hours, minutes } = this.state;
            onChange(this.join(days, hours, minutes));
        });
    }

    render() {
        const elementProps = { min: 0, type: 'number' }
        const { days, hours, minutes } = this.state;

        return (
            <Form.Field className="duration-input">
                <Grid columns="equal">
                    <Grid.Row>
                        <Grid.Column>
                            <Input
                                {...elementProps}
                                label="Days"
                                placeholder="Days"
                                value={days}
                                onChange={this.handleDaysChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <Input
                                {...elementProps}
                                label="Hours"
                                placeholder="Hours"
                                max={23}
                                value={hours}
                                onChange={this.handleHoursChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <Input
                                {...elementProps}
                                label="Minutes"
                                placeholder="Minutes"
                                max={59}
                                min={days + hours > 0 ? 0 : this.props.min}
                                value={minutes}
                                onChange={this.handleMinutesChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                </Grid>
            </Form.Field>
        );
    }
}


function renderWrapper({
    label,
    input,
    withValidationMessage = true,
    min = 0,
    additionalClass,
    meta: { touched, error },
    ...rest
}) {
    const hasError = touched && error !== undefined;
    /* eslint-disable */
    const {
        value,
        onChange,
        onBlur,
        ...inputProps
    } = input;
    /* eslint-enable */

    return (
        <Form.Field error={hasError}>
            {
                label &&
                <label>{label}</label>
            }
            <RenderInputs
                className="modus-date-time"
                {...rest}
                {...inputProps}
                min={min}
                value={Math.max(value, min)}
                onChange={v => onChange(Math.max(v, min))}
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

export default function DurationInput({ validate = [], ...props }) {
    return (
        <Field
            {...props}
            validate={validate}
            component={renderWrapper}
        />
    );
}
