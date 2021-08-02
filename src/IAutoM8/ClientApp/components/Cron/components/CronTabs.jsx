import React, { Component } from 'react'
import { Tab } from 'semantic-ui-react'

import MinuteTab from './MinuteTab'
import HourTab from './HourTab'
import DayTab from './DayTab'
import WeekTab from './WeekTab'
import MonthTab from './MonthTab'
import YearTab from './YearTab'

export default class CronTabs extends Component {

    constructor(props) {
        super(props);

        this.handleMinuteChange = this.handleChange.bind(this, 'minutes');
        this.handleHoursChange = this.handleChange.bind(this, 'hours');
        this.handleDaysChange = this.handleChange.bind(this, 'days');
        this.handleWeeksChange = this.handleChange.bind(this, 'weeks');
        this.handleMonthsChange = this.handleChange.bind(this, 'months');
        this.handleYearsChange = this.handleChange.bind(this, 'years');
    }

    handleChange(section, e, data) {
        this.props.onChange(section, e, data)
    }

    render() {
        const {
            values: { minutes, hours, days, weeks, months, years },
            onChangeAsap
        } = this.props;

        const menuOptions = { borderless: true, attached: false, tabular: false };

        const panes = [
            {
                menuItem: 'Minutes',
                render: () => <MinuteTab values={minutes} onChange={this.handleMinuteChange} />
            },
            {
                menuItem: 'Hourly',
                render: () => <HourTab values={hours} onChange={this.handleHoursChange}
                    onChangeAsap={(isAsap) => onChangeAsap('hours', isAsap)} />
            },
            {
                menuItem: 'Daily',
                render: () => <DayTab values={days} onChange={this.handleDaysChange}
                    onChangeAsap={(isAsap) => onChangeAsap('days', isAsap)} />
            },
            {
                menuItem: 'Weekly',
                render: () => <WeekTab values={weeks} onChange={this.handleWeeksChange}
                    onChangeAsap={(isAsap) => onChangeAsap('weeks', isAsap)} />
            },
            {
                menuItem: 'Monthly',
                render: () => <MonthTab values={months} onChange={this.handleMonthsChange}
                    onChangeAsap={(isAsap) => onChangeAsap('months', isAsap)} />
            },
            {
                menuItem: 'Yearly',
                render: () => <YearTab values={years} onChange={this.handleYearsChange}
                    onChangeAsap={(isAsap) => onChangeAsap('years', isAsap)} />
            }
        ];

        return (
            <div>
                <Tab
                    panes={panes}
                    menu={menuOptions}
                    defaultActiveIndex={this.props.defaultActiveIndex}
                    onTabChange={this.props.onTabChange}
                />
            </div>
        );
    }
}
