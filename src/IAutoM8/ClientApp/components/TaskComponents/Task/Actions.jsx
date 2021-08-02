import React from 'react'
import { Dropdown } from 'semantic-ui-react'

import { WhenPermitted, ModusIcon } from '@components'

import './styles/Actions.less'

export default function Actions({
    id,
    canDo, canReview, canChangeProcessingUser, hasProcessingUser,
    onReviewCard, onDoCard, onEditCard, onChangeProcessingUser, onDeleteCard,
    canDelete, onStopOutsource, hasAcceptedVendor, canDisable, onDisableCard,
    isEditable, onEnableCard,publicCheck,isAdmin,
    global, isFormulaTask
 }) {

    return (
        <Dropdown
            item
            className="task-actions"
            icon={<ModusIcon name="iauto--dropdown" />}
        >
            <Dropdown.Menu>
                {canReview &&
                    <Dropdown.Item onClick={() => onReviewCard(id)}>
                        <a>
                            <ModusIcon name="iauto--play" />
                            Review task
                        </a>
                    </Dropdown.Item>
                }

                {canDo &&
                    <Dropdown.Item onClick={() => onDoCard(id)}>
                        <a>
                            <ModusIcon name="iauto--play" />
                            Start the task
                        </a>
                    </Dropdown.Item>
                }

                {isFormulaTask && isAdmin === false && publicCheck.type === "public" ? "" :
                    (!isEditable) &&
                    <WhenPermitted rule="editProjectTask">
                        {(permitted) =>
                            <Dropdown.Item onClick={onEditCard}>
                                <a>
                                    <ModusIcon name="iauto--edit" />
                                    {permitted ? 'Edit' : 'View'}
                                </a>
                            </Dropdown.Item>
                        }
                    </WhenPermitted>
                }

                {hasAcceptedVendor &&
                    <WhenPermitted rule="canStopOutsource">
                        <Dropdown.Item onClick={() => onStopOutsource(id)}>
                            <a>
                                <ModusIcon name="iauto--remove" />
                                Stop outsource
                            </a>
                        </Dropdown.Item>
                    </WhenPermitted>
                }

                {hasProcessingUser &&
                    <WhenPermitted rule="canChangeProcessingUser">
                        <Dropdown.Item onClick={() => onChangeProcessingUser(id)}>
                            <a>
                                <ModusIcon name="iauto--play" />
                                Change processing user
                            </a>
                        </Dropdown.Item>
                    </WhenPermitted>
                }
                {isFormulaTask && isAdmin === false && publicCheck.type === "public" ? "" :
                    (canDelete === undefined || canDelete) &&  //isAdmin === false ? "" :
                    <WhenPermitted rule="deleteProjectTask">
                        <Dropdown.Item onClick={() => onDeleteCard(id)}>
                            <a>
                                <ModusIcon name="iauto--remove" />
                                Delete
                            </a>
                        </Dropdown.Item>
                    </WhenPermitted>
                }
                {
                    (canDisable !== undefined && canDisable) && 
                    (!isEditable ? 
                    <Dropdown.Item onClick={() => onDisableCard(id)}>
                        <a>
                            <ModusIcon name="iauto--remove" />
                            Disable
                        </a>
                    </Dropdown.Item> :
                    <Dropdown.Item onClick={() => onEnableCard(id)}>
                        <a>
                            <ModusIcon name="iauto--edit" />
                            Enable
                        </a>
                    </Dropdown.Item>)
                }
            </Dropdown.Menu>
        </Dropdown>
    );
}
