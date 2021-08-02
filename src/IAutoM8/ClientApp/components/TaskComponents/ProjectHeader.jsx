import React from 'react'
import { Header, Label } from 'semantic-ui-react'
import cn from 'classnames'

import { SimpleSegment } from '@components'

import './styles/ProjectHeader.less'

export default function ProjectHeader({ name, tasks, overdue, className, ...otherProps }) {
    return (
        <SimpleSegment className={cn(['project-header', className])} {...otherProps}>
            <Header content={name} />
            {/* <span className="project-header__label">
                <Label className="all-tasks" circular empty />
                {tasks} tasks
            </span>

            <span className="project-header__label">
                <Label className="overdue" circular empty />
                {overdue} overdue
            </span> */}
        </SimpleSegment>
    )
}