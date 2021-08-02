import React from 'react'
import { Header, Form } from 'semantic-ui-react'

import { ItemsFilter, SimpleSegment, ModusButton } from '@components'

export default function ClientsHeader({ onAddClientClick, setFilterColumn, filterBy }) {

    const filterOptions = [
        { key: 'companyName', value: 'companyName', text: 'Company Name' },
        { key: 'representative', value: 'representative', text: 'Representative' },
        { key: 'address', value: 'address', text: 'Address' }
    ];

    return (
        <SimpleSegment clearing className="iauto-projects__header">
            <Header as="h2" size="large" floated="left">
                Clients
            </Header>

            <Form as={SimpleSegment} floated="right">
                <Form.Group inline>
                    <Form.Field>
                        <ItemsFilter
                            by={filterOptions}
                            defaultValue="companyName"
                            filterValue={filterBy}
                            setFilter={setFilterColumn}
                        />
                    </Form.Field>
                    <Form.Field>
                        <ModusButton
                            wide
                            filled
                            icon="iauto--add-white"
                            content="New client"
                            onClick={onAddClientClick}
                            whenPermitted="createClient"
                        />
                    </Form.Field>
                </Form.Group>
            </Form>
        </SimpleSegment>
    );
}
