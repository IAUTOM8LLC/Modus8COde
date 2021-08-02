import React from 'react'
import { List, Table, Header, Button } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

import { ModusButton, TimeAgo, WhenPermitted, Inline } from '@components'

export default function ProjectItem({ project, onDelete, onEdit }) {
    
    const managers = project.managers.map(s => s.userName);
    return (
        <Table.Row>
            <Table.Cell>
                <WhenPermitted rule="viewKanbanBoard">
                    {
                        (permitted) => permitted
                            ? (
                                <Header size="small" as={Link} to={`/projects/${project.id}`}>
                                    {project.name}
                                </Header>
                            )
                            : <h4>{project.name}</h4>
                    }
                </WhenPermitted>
                <br />
                {project.description &&
                    <span>{project.description}</span>
                }
            </Table.Cell>
            <Table.Cell>
                <span>{project.client}</span>
            </Table.Cell>
            <Table.Cell>
                <List items={managers} />
            </Table.Cell>
            <Table.Cell>
                <TimeAgo date={project.dateCreated} />
            </Table.Cell>
            <Table.Cell>
                <TimeAgo date={project.lastUpdated} />
            </Table.Cell>
            <Table.Cell collapsing className="center aligned">
                <Inline>
                    <Button.Group>
                        <ModusButton
                            as={Link}
                            to={`/projects/${project.id}`}
                            size="mini"
                            content="Smart board"
                            whenPermitted="viewKanbanBoard"
                        />
                        <ModusButton
                            as={Link}
                            to={`/projects/${project.id}/editor`}
                            size="mini"
                            content="UI Editor"
                            whenPermitted="accessToProjectUiEditor"
                        />
                        <ModusButton
                            as={Link}
                            to={`/projects/${project.id}/calendar`}
                            content="Calendar"
                            whenPermitted="viewKanbanBoard"
                        />
                    </Button.Group>
                    <ModusButton
                        icon="iauto--edit"
                        circular
                        onClick={() => onEdit(project.id)}
                        popup="Edit"
                        whenPermitted="editProject"
                    />
                    <ModusButton
                        icon="iauto--remove"
                        circular
                        onClick={() => onDelete(project.id)}
                        popup="Delete"
                        whenPermitted="deleteProject"
                    />
                </Inline>
            </Table.Cell>
        </Table.Row>
    );
}
