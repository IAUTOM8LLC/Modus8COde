import React  from "react";
import { 
    Table, 
    Header, 
    Icon, 
    Visibility,
    // Loader,
    // Dimmer,
} from "semantic-ui-react";

import FormulaItem from "./FormulaItem";

export default function FormulasTable({
    formulas,
    onDelete,
    onEdit,
    onCreateProject,
    // onOpenFormulaOverview,
    orderColumn,
    onSort,
    sortDirection,
    onUnlock,
    onLock,
    onAddStar,
    onRemoveStar,
    nextPage,
    loggedInUserRole,
    changeFormulaStatus,
    onCopy,
    // isAdmin,
    loading
}) {
        
    const formulaItems = formulas.map((f) => (
        <FormulaItem
            key={f.id}
            formula={f}
            onDelete={onDelete}
            onEdit={onEdit}
            onCreateProject={onCreateProject}
            onUnlock={onUnlock}
            onLock={onLock}
            onAddStar={onAddStar}
            onRemoveStar={onRemoveStar}
            loggedInUserRole={loggedInUserRole}
            changeFormulaStatus={changeFormulaStatus}
            onCopy={onCopy}
            // onOpenFormulaOverview={onOpenFormulaOverview}
            // isAdmin={isAdmin}
        />
    ));

    return (
        <React.Fragment>
            {/* <Dimmer active={loading}>
            <Loader size="huge">Loading</Loader>
        </Dimmer> */}
            <Table structured sortable className="formulaTable">
                <Table.Header>
                    <Table.Row>
                        <Table.HeaderCell rowSpan="2" />
                        <Table.HeaderCell rowSpan="2" />
                        <Table.HeaderCell
                            verticalAlign="bottom"
                            rowSpan="2"
                            sorted={
                                orderColumn === "name" ? sortDirection : null
                            }
                            onClick={onSort("name")}
                        >
                            Name
                        </Table.HeaderCell>
                        {/* Commenting the code, as per the client feedback dated: August 27, 2020 */}
                        {/* <Table.HeaderCell
                        sorted={
                            orderColumn === "owner.fullName"
                                ? sortDirection
                                : null
                        }
                        onClick={onSort("owner.fullName")}
                    >
                        Owner
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={
                            orderColumn === "dateCreated" ? sortDirection : null
                        }
                        onClick={onSort("dateCreated")}
                    >
                        Created
                    </Table.HeaderCell> */}
                        {/* <Table.HeaderCell
                            sorted={
                                orderColumn === "lastUpdated"
                                    ? sortDirection
                                    : null
                            }
                            onClick={onSort("lastUpdated")}
                        >
                            Last updated
                        </Table.HeaderCell> */}
                        {/*  <Table.HeaderCell textAlign="center" colSpan="2">
                            Approx. Turn Around Times
                        </Table.HeaderCell>*/}

                        <Table.HeaderCell textAlign="" style={{paddingLeft:"46px"}} colSpan="2">
                            Approx. Turn Around Times
                        </Table.HeaderCell>
                        <Table.HeaderCell  rowSpan="2" collapsing />
                    </Table.Row>
                    <Table.Row>
                        <Table.HeaderCell
                            textAlign="center"
                            // sorted={
                            //     orderColumn === "name" ? sortDirection : null
                            // }
                            // onClick={onSort("name")}
                        >
                            Outsource
                        </Table.HeaderCell>
                        <Table.HeaderCell 
                            textAlign="center"
                            // sorted={
                            //     orderColumn === "name" ? sortDirection : null
                            // }
                            // onClick={onSort("name")}
                        >
                            {/* Total*/}
                        </Table.HeaderCell>
                    </Table.Row>
                </Table.Header>

                <Visibility
                    as="tbody"
                    continuous={false}
                    once={false}
                    onBottomVisible={() => nextPage && nextPage()}
                >
                    {formulaItems}
                    {!loading && formulaItems.length === 0 && (
                        <Table.Row>
                            <Table.HeaderCell
                                colSpan={5}
                                style={{ height: 300 }}
                            >
                                <Header as="h2" icon textAlign="center">
                                    <Icon name="list layout" circular />
                                    <Header.Content>
                                        No formulas created
                                    </Header.Content>
                                    <Header.Subheader>
                                        Try to create a new formula
                                        {loggedInUserRole === "Admin"
                                            ? ""
                                            : " or ask friends to share their formulas with you"}
                                    </Header.Subheader>
                                </Header>
                            </Table.HeaderCell>
                        </Table.Row>
                    )}
                </Visibility>
            </Table>
        </React.Fragment>
    );
}
