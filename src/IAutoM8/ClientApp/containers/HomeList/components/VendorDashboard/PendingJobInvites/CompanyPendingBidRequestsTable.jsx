import React from "react";
import { Header, Icon, Table } from "semantic-ui-react";
import { v4 as uuidv4 } from "uuid";

import CompanyPendingBidRequestsHeader from "./CompanyPendingBidRequestsHeader";

import CompanyPendingBidRequestsItem from "./CompanyPendingBidRequestsItem";


const CompanyPendingBidRequestsTable = ({ 
        companyFormulaBids,
        onDeclineFormulaBid
}) => {
    
    const vendorBidRequestsItems = companyFormulaBids.map((formulaBid) => (
        // <PendingBidRequestsItem
        //     
        // />
        <CompanyPendingBidRequestsItem 
            key={uuidv4()} 
             formulaBid={formulaBid}
             onDeclineFormulaBid={onDeclineFormulaBid}
        />
    ));

    return (
        <Table className="home-list" sortable>
            <Table.Header>
                <CompanyPendingBidRequestsHeader />
            </Table.Header>

            <Table.Body>
                {vendorBidRequestsItems}
                {vendorBidRequestsItems.length === 0 && (
                    <Table.Row>
                        <Table.HeaderCell colSpan={10} style={{ height: 125 }}>
                            <Header as="h2" icon textAlign="center">
                                <Header.Content>
                                    No Formula Bids
                                </Header.Content>
                            </Header>
                        </Table.HeaderCell>
                    </Table.Row>
                )} 
            </Table.Body>
        </Table>
    );
};

export default CompanyPendingBidRequestsTable;
