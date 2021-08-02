import React, { Fragment } from 'react'
import { Table, Header, Icon } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

// import ProjectItem from './ProjectItem'

import SortableTree from '../../../components/Common/SortableTree';

export default function ProjectTable({
    projects,
    onDelete,
    onEdit,
    orderColumn,
    onSort,
    sortDirection,
    onAddChildProject,
    onUpdateChildProject,
    onShowResources,
    handleNotesModal,
    isVendor,
    role
}) {
    const projectItems = (<SortableTree
            project={projects}
            onDelete={onDelete}
            onEdit={onEdit}
            onAddChildProject={onAddChildProject}
            onUpdateChildProject={onUpdateChildProject}
            onShowResources = {onShowResources}
            handleNotesModal = {handleNotesModal}
            isVendor ={isVendor}
            role ={role}
        />);

    // const projectItems = projects.map(p =>
    //     <ProjectItem
    //         key={p.id}
    //         project={p}
    //         onDelete={onDelete}
    //         onEdit={onEdit}
    //     />
    // );

    return (
        <Table sortable>
            <Table.Header>
                <Table.Row>
                    <Table.HeaderCell
                        sorted={orderColumn === 'name' ? sortDirection : null}
                        onClick={onSort('name')}
                    >
                        Name
                    </Table.HeaderCell>
                    <Table.HeaderCell collapsing />
                </Table.Row>
            </Table.Header>

            <Table.Body>
                {(projects.length > 0) && projectItems}
                {
                    projects.length === 0 &&
                    <Table.Row>
                        <Table.HeaderCell colSpan={2} style={{ height: 300 }}>
                            <Header as="h2" icon textAlign="center">
                                <Icon name="list layout" circular />
                                <Header.Content>
                                    No projects created
                                </Header.Content>
                                <Header.Subheader>
                                    Try to create a new project or create project from
                                    {' '}<Link to="/formulas">formula</Link>
                                </Header.Subheader>
                            </Header>
                        </Table.HeaderCell>
                    </Table.Row>
                }
            </Table.Body>
        </Table>
    );
}
