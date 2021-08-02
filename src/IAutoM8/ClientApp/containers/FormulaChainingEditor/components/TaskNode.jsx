import React from 'react'
import { Card } from 'semantic-ui-react'

import { TaskActions, TaskHeader, TaskTypeIcon } from '@components'

export default function TaskNode({ 
    id,
    node,
    onEdit,
    onEnable,
    onDelete,
    onDisable,
    canDisable,
    isEditable,
    formulaCheck,
    publicCheck,
    isAdmin
}) {
    const {
        title, condition,
        isConditional, isRecurrent, isInterval,
        parentTaskId
    } = node;
    
    const openModal = () => onEdit(id);

    return (
        <Card className={`card--editor modus`}>
            <Card.Content>
                <i className="ep"></i>

                <div className="card-editor-header">
                    <Card.Header>
                        <TaskHeader
                            title={title}
                            onClick={openModal}
                        />
                        {
                            condition &&
                            <Card.Meta>
                                {condition.condition}
                            </Card.Meta>
                        }
                    </Card.Header>

                    <TaskActions
                        id={id}
                        formulaCheck={formulaCheck}
                        publicCheck={publicCheck}
                        isFormulaTask={true}
                        isAdmin={isAdmin}
                        isEditable={isEditable}
                        canDisable={canDisable}
                        canDelete={parentTaskId === null}
                        onEditCard={openModal}
                        onEnableCard={() => onEnable(id)}
                        onDeleteCard={() => onDelete(id)}
                        onDisableCard={() => onDisable(id)}
                    />
                </div>

                <div className="task-footer">
                    <TaskTypeIcon
                        isInterval={isInterval}
                        isRecurrent={isRecurrent}
                        isConditional={isConditional}
                    />
                </div>
            </Card.Content>
        </Card>
    );
}
