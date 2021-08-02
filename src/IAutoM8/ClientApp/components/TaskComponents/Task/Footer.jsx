import React from 'react'

import { TaskDueDate, TaskStartDate, ModusIcon, TaskTypeIcon } from '@components'

import './styles/Footer.less'

export default function Footer({
    dueDate,
    startDate,
    isOverdue,
    isInterval,
    isConditional,
    isRecurrent,
    hasVendor,
    hasAssignedVendor,
    onOutsourceTabOpen,
    status,
    completedDate
}) {

    const style = "task-vendor" + (hasAssignedVendor ? "__assigned" : "");
    return (
        <div className="task-footer">
            {
                dueDate && !startDate &&
                <TaskDueDate date={dueDate} />
            }
            {
                startDate &&
                <TaskStartDate date={startDate} />
            }

            <TaskTypeIcon
                popup
                isInterval={isInterval}
                isRecurrent={isRecurrent}
                isConditional={isConditional}/>

            {isOverdue &&
                <ModusIcon name="task overdue" popup="Overdue" />
            }
            {hasVendor &&
                <ModusIcon style={{cursor: 'pointer'}} name={style} onClick={onOutsourceTabOpen} />}
            {status === "Completed" &&
                <div className="completed">
                    <span>Completed: </span> <TaskDueDate date={completedDate} />
                </div>
            }
        </div>
    );
}
