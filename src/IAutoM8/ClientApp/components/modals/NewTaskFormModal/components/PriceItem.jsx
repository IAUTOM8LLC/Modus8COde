import React from 'react'
import { Form, Rating, Table, Input } from 'semantic-ui-react'

import { TextInput, DropdownInput, WhenPermitted } from '@components'

export default function PriceItem({item}) {
    return (
        <Table.Row>
             <Table.Cell width="5">{item.companyWorkerFullName}</Table.Cell>
                            <Table.Cell>
                                <Rating
                                    icon="star"
                                    name= "rating"
                                    defaultRating={item.rating}
                                    maxRating={5}
                                    disabled
                                /></Table.Cell>

                            <Table.Cell width="2">
                                <TextInput
                                    name={item.companyWorkerId}
                                    //value={item.companyWorkerPrice}
                                /></Table.Cell>
        </Table.Row>
    );
}
