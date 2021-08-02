import React from 'react'
import { Dropdown, Popup } from 'semantic-ui-react'

import splitToLowerCase from '@utils/splitToLowerCase'

import { ModusIcon } from '@components'

export default function GroupNode({
    id,
    node,
    membersCount,
    collapsed,
    toggle,
    onDelete
}) {
    const { title } = node;

    const titleWithCount = `${title} (${membersCount})`;
    const Header = <p className="task-group__header">
        {titleWithCount}
    </p>

    return (
        <div className="task-group__head" id={id}>
            <i className={`ep ${splitToLowerCase(node.status)}`} />

            {collapsed ?
                <Popup
                    position="top center"
                    trigger={Header}
                    content={titleWithCount}
                />
                : Header
            }

            <div className="task-group__actions">
                <Dropdown
                    item
                    className="task-group__dropdown"
                    icon={<ModusIcon name="iauto--dropdown" />}
                >
                    <Dropdown.Menu>
                        <Dropdown.Item onClick={() => onDelete(id)}>
                            <a>
                                <ModusIcon name="iauto--remove" />
                                Delete
                            </a>
                        </Dropdown.Item>
                    </Dropdown.Menu>
                </Dropdown>

                <span
                    className="task-group__toggle"
                    onClick={toggle}
                >
                    <i />
                </span>
            </div>
        </div>
    )
}
