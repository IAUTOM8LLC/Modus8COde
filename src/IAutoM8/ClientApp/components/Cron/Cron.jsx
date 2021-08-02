import React, { Component } from 'react'

import moment from 'moment'

import CronTabs from './components/CronTabs'
import { getDefaultCronModel } from './data'
import { parseCron, convertCron } from './parser'

export default class Cron extends Component {
    static defaultProps = {
        tab: 0,
    }
    constructor(props) {
        super(props);
        const initialCronModel =
            convertCron(props.defaultCron, props.tab, false, props.isAsap);
        this.state = {
            initialCronModel,
            activeIndex: props.tab,
            cronModel: initialCronModel
        }
    }

    componentDidMount() {
        this.emit();
    }

    componentDidUpdate() {
        this.emit();
    }

    emit() {
        const { activeIndex, cronModel } = this.state;

        this.props.onChange({
            data: parseCron(cronModel, activeIndex),
            cronTab: activeIndex
        });
    }

    handleChangeAsap = (section, isAsap) => {
        const { cronModel, initialCronModel } = this.state;
        this.setState({
            initialCronModel: {
                ...initialCronModel,
                [section]: {
                    ...cronModel[section],
                    isAsap
                }
            },
            cronModel: {
                ...cronModel,
                [section]: {
                    ...cronModel[section],
                    isAsap
                }
            }
        }, this.emit);
    }

    handleChange = (section, event, data) => {
        if (this.props.disabled) {
            return;
        }

        const { cronModel } = this.state;
        const { name, value } = data;

        if (name === 'checkedDays') {
            const newValues = [...cronModel.weeks.checkedDays];
            if (data.checked) {
                newValues.push(value);
            } else {
                newValues.splice(newValues.indexOf(value), 1);
            }
            cronModel.weeks.checkedDays = newValues;
        } else {
            cronModel[section][name] = value;
            const nowUtc = moment().utc();
            if (section === 'minutes') {
                cronModel[section].startFrom = nowUtc.minute();
            }
            else if (section === 'hours') {
                cronModel[section].startFrom = nowUtc.hour();
            }
            else if (section === 'days') {
                cronModel[section].startFrom = nowUtc.date();
            }
        }

        this.setState({
            cronModel: { ...cronModel }
        });
    }

    handleTabChange = (e, { activeIndex }) => {
        const cronModel = this.props.tab === activeIndex
            ? this.state.initialCronModel
            : getDefaultCronModel();

        this.setState({ cronModel, activeIndex });
    }

    render() {
        return (
            <CronTabs
                values={this.state.cronModel}
                defaultActiveIndex={this.props.tab}
                onChange={this.handleChange}
                onTabChange={this.handleTabChange}
                onChangeAsap={this.handleChangeAsap}
            />
        );
    }
}
