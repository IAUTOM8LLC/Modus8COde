import React from 'react';
import { Header } from 'semantic-ui-react'

import { ModusButton } from '@components'

import './CalendarViewHeader.less'

export default function CalendarViewHeader({ messages, label, onNavigate }) {
    return (
        <div className="calendar-view-header">
            <div className="calendar-view-header__items">

                <div className="calendar-view-header__days">
                    <div className="calendar-view-header__days-switcher">
                        <ModusButton
                            grey
                            circular
                            icon="long arrow left"
                            onClick={() => onNavigate('PREV')}
                            className="calendar-view-header__days-switcher--prev"
                        />
                        <ModusButton
                            grey
                            content={messages.today}
                            onClick={() => onNavigate('TODAY')}
                            className="calendar-view-header__days-switcher--today"
                        />
                        <ModusButton
                            grey
                            circular
                            icon="long arrow right"
                            onClick={() => onNavigate('NEXT')}
                            className="calendar-view-header__days-switcher--next"
                        />
                    </div>

                    <Header
                        as="h3"
                        className="calendar-view-header__days-label"
                        content={label}
                    />
                </div>
            </div>
        </div>
    );
}