import React from 'react'
import { Header, Form } from 'semantic-ui-react'

import { ItemsFilter, SimpleSegment, ModusButton } from '@components'

export default function ProjectsHeader({ onAddProjectClick, filterBy, setFilterColumn ,
    onUploadBulkProject }) {

    // const orderOptions = [
    //     { key: 'name', value: 'name', text: 'Name' },
    //     { key: 'description', value: 'description', text: 'Description' },
    // ];

    return (
        <SimpleSegment clearing className="iauto-projects__header">
            <Header as="h2" size="large" floated="left">
                Projects
            </Header>

            <Form as={SimpleSegment} floated="right">
                <Form.Group inline>
                    {/* <Form.Field>
                        <ItemsFilter
                            by={orderOptions}
                            filterValue={filterBy}
                            setFilter={setFilterColumn}
                        />
                    </Form.Field> */}
                    <Form.Field>
                    {/* <ModusButton
                            filled
                            icon="upload icon"
                            content="Upload Projects"
                            onClick={onUploadBulkProject}
                            //whenPermitted="createProject"
                        /> */}
                        <ModusButton
                            filled
                            icon="iauto--add-white"
                            content="New project"
                            onClick={onAddProjectClick}
                            whenPermitted="createProject"
                        />
                    </Form.Field>
                </Form.Group>
            </Form>
        </SimpleSegment>
    );
}
