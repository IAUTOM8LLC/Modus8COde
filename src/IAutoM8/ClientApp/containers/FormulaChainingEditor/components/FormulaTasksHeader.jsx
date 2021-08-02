import React from 'react'
import { Header, Segment, Label, Divider } from 'semantic-ui-react'
import cn from 'classnames'

import { FormulaShareLink, SimpleSegment } from '@components'

export default function FormulaTasksHeader({ name, formulaId, isShared, taskCount = 0 }) {
    return (
        <div>
            <Segment clearing basic className="iauto-task-header">
                <SimpleSegment
                    floated="left"
                    className={cn(['project-header', 'iauto-task-header__left'])}
                >
                    <Header content={name} />
                    <span className="project-header__label">
                        <Label className="all-tasks" circular empty />
                        {taskCount} tasks
                    </span>
                </SimpleSegment>

                <SimpleSegment floated="right" className="iauto-task-header__right">
                    {isShared &&
                        <FormulaShareLink formulaId={formulaId} />
                    }
                </SimpleSegment>
            </Segment>

            <Divider style={{ marginBottom: 39 }} />
        </div>
    );
}
