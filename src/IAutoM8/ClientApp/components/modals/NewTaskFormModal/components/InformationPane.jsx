import React from 'react'
import { Form, Rating, Table, Input , Modal } from 'semantic-ui-react'

import { TextInput, DropdownInput, WhenPermitted } from '@components'

import PriceItem from './PriceItem'

export default function InformationPane({modalItems}) {

    const PriceItems = modalItems.map((p) => (
        <PriceItem
            key={p.id}
            item={p}
            // onDelete={onDelete}
            // onEdit={onEdit}
        />))

     const companyPrice = modalItems[0].companyPrice
     //.map(item => item.companyPrice)
    return (
        <div className="project-details-modal__pane">
            <Form as="section">
                <Table.Row>
                    <Table.Cell width="2">
                        Gig Price</Table.Cell>
                    <Modal.Content>
                    <Table.Cell>
                    
                        <TextInput name="companyPrice" /></Table.Cell>
                    </Modal.Content>
                        {/* <TextInput
                    required
                    name="companyPrice"
                    label="Project name"
                /> */}
                </Table.Row>
                <Table sortable>
                    <Table.Header>
                        <Table.Row>
                            <Table.HeaderCell
                            >
                                Worker Name
                            </Table.HeaderCell>
                            <Table.HeaderCell
                            >
                                Rating
                            </Table.HeaderCell>
                            <Table.HeaderCell
                            >
                                Gig Payout
                            </Table.HeaderCell>
                        </Table.Row>
                    </Table.Header>

                    <Table.Body>
                        {PriceItems}
                    </Table.Body>
                </Table>

            </Form>
        </div>
    );
}
