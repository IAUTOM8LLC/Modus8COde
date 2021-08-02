import React from 'react'
import { Card } from 'semantic-ui-react'
import moment from 'moment'

import { friendlyStatus } from '@utils/task'
import splitToLowerCase from '@utils/splitToLowerCase'

import { TaskFooter, TaskHeader, TaskActions } from '@components'

export default function TaskNode({
    id,
    node,
    skills,
    onEdit,
    onDelete,
    onChangeProcessingUser,
    onOutsourceTabOpen
}) {
    const {
        title,
        status,
        startDate = '',
        duration,
        isOverdue,
        isConditional,
        isRecurrent,
        isInterval,
        assignedSkillId,
        reviewingUserName,
        proccessingUserName,
        onStopOutsource,
        hasAcceptedVendor,

        hasVendor,
        hasAssignedVendor,
        isDisabled
    } = node;

    const hasProcessingUser = (proccessingUserName && status === 'InProgress')
        || (reviewingUserName && status === 'NeedsReview');

    const dueDate = moment(startDate).add(duration, 'm').toDate();
    const assignedSkill = skills.find(t => t.id === assignedSkillId) || {};

    const openModal = () => onEdit(id);
    const isDisabledStyle = isDisabled ? { pointerEvents: "none", opacity: "0.4" } : {};
    return (
        <Card className={`modus card--editor  ${status}`} style={isDisabledStyle}>
            <Card.Content>
                <i className={`ep ${splitToLowerCase(status)}`} />

                {!isDisabled && (<div className="card-editor-header">
                    <div className={`card-status card-status--${splitToLowerCase(status)}`}>
                        {friendlyStatus(status)}
                    </div>

                    <TaskActions
                        id={id}
                        onEditCard={openModal}
                        onDeleteCard={() => onDelete(id)}
                        onChangeProcessingUser={onChangeProcessingUser}
                        hasProcessingUser={hasProcessingUser}
                        onStopOutsource={onStopOutsource}
                        hasAcceptedVendor={hasAcceptedVendor &&
                            (status === 'InProgress' || status === 'New')}
                    />
                </div>)}

                <TaskHeader
                    title={title}
                    status={status}
                    username={reviewingUserName || proccessingUserName || assignedSkill.name}
                    doing={proccessingUserName !== null}
                    reviewing={reviewingUserName !== null}
                    onClick={openModal}
                />

                {!isDisabled && <TaskFooter
                    dueDate={dueDate}
                    isOverdue={isOverdue}
                    isConditional={isConditional}
                    isRecurrent={isRecurrent}
                    isInterval={isInterval}
                    hasVendor={hasVendor}
                    hasAssignedVendor={hasAssignedVendor}
                    onOutsourceTabOpen={() => onOutsourceTabOpen(id)}
                />}
            </Card.Content>
        </Card>
    );
}
