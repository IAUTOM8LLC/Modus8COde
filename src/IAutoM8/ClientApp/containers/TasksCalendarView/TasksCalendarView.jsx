import React, { Component } from 'react'
import BigCalendar from 'react-big-calendar'
import moment from 'moment'

import splitToLowerCase from '@utils/splitToLowerCase'

import { ProjectTasksHeader, CalendarViewSelect } from '@components'

import CalendarViewHeader from './components/CalendarViewHeader'
import EventPopup from './components/EventPopup'

import 'react-big-calendar/lib/less/styles.less'
import './TasksCalendarView.less'

BigCalendar.setLocalizer(
    BigCalendar.momentLocalizer(moment)
);

const defaultFormatter = (date, culture, localizer) =>
    localizer.format(date, 'MMMM DD, YYYY', culture);

export default class TasksCalendarView extends Component {
    static views = ['month', 'week', 'day']

    static formats = {
        dayFormat: defaultFormatter,
        dayHeaderFormat: defaultFormatter,

        dayRangeHeaderFormat: ({ start, end }, culture, local) =>
            `${defaultFormatter(start, culture, local)} — ${defaultFormatter(end, culture, local)}`,

        monthHeaderFormat: (date, culture, localizer) =>
            localizer.format(date, 'MMMM, YYYY', culture)
    }

    state = {
        calendarView: 'month'
    }

    buildEvents() {
        const events = this.props.tasks
            .filter(t =>
                moment(t.startDate).isValid()
                && moment(t.dueDate).isValid()
                && t.formulaId === null
            )
            .map(t => ({
                id: t.id,
                status: t.status,
                'title': t.title,
                'start': moment(t.startDate).toDate(),
                'end': moment(t.dueDate).toDate()
            }));

        return events;
    }

    eventPropGetter = (event) => {
        return {
            className: splitToLowerCase(event.status)
        };
    }

    handleSelectSlot = (slotInfo) => {
        const { start, end } = slotInfo;
        const isMonthView = this.state.calendarView === 'month';

        if (moment(start).isBefore(moment(), isMonthView ? 'day' : 'minute')) {
            this.props.warning({
                title: 'Cannot create a task in the past'
            });
            return;
        }

        const startDate = moment(start);
        const dueDate = moment(end);

        if (isMonthView) {
            dueDate.add(1, 'd').subtract(1, 'm');
        }

        const diffMilliseconds = dueDate.toDate().getTime() - startDate.toDate().getTime();
        const diffMinutes = Math.floor(diffMilliseconds / 1000 / 60);

        this.props.newTask({
            startDate: startDate.format('MMM D, YYYY h:mm A'),
            dueDate: dueDate.format('MMM D, YYYY h:mm A'),
            duration: diffMinutes
        });
    }

    handleSelectCalendarView = (e, data) => {// eslint-disable-line no-unused-vars
        this.setState({ calendarView: data.value });
    }

    renderEventPopup = (props) => {
        const tasks = this.props.tasks || [];
        const task = tasks.find(t => t.id === props.event.id) || {};
        return <EventPopup
            {...props}
            task={task}
            onEdit={this.props.editTask}
            onDelete={this.props.deleteTask}
            onOutsourceTabOpen={() => this.props.onOutsourceTabOpen(task.id)}
        />
    }

    render() {
        const components = {
            toolbar: CalendarViewHeader,
            eventWrapper: this.renderEventPopup
        }

        return (
            <div className="modus-calendar-view">
                <ProjectTasksHeader
                    {...this.props}
                    projectId={this.props.projectId}
                    path={this.props.match.path}
                    name={this.props.projectName}
                    taskCount={this.props.tasks.length}
                    overdueCount={this.props.overdueCount}
                    onShowResources={this.props.onShowResources}
                >
                    <CalendarViewSelect
                        view={this.state.calendarView}
                        onChangeView={this.handleSelectCalendarView}
                    />
                </ProjectTasksHeader>

                <BigCalendar
                    selectable
                    popup
                    className="demo"
                    step={60}
                    view={this.state.calendarView}
                    onView={() => { }}
                    defaultDate={new Date()}
                    events={this.buildEvents()}
                    components={components}
                    views={TasksCalendarView.views}
                    formats={TasksCalendarView.formats}
                    eventPropGetter={this.eventPropGetter}
                    onSelectSlot={this.handleSelectSlot}
                />
            </div>
        );
    }
}
