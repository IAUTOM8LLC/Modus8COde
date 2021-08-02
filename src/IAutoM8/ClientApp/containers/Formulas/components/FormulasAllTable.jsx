import React from 'react'
import { Table, Header, Icon, Checkbox, Visibility } from 'semantic-ui-react'

import FormulaAllItem from './FormulaAllItem'

export default function FormulasAllTable({
    formulas,
    onView,
    onChange,
    onDownloadProject,
    orderColumn,
    onSort,
    sortDirection,
    selectedFormulas,
    onToggleSelection,
    onAddStar,
    onRemoveStar,
    nextPage
}) {
    const formulaItems = formulas.map(f =>
        <FormulaAllItem
            key={f.id}
            formula={f}
            onView={onView}
            onChange={onChange}
            isSelected={selectedFormulas.some(id => id === f.id)}
            onDownloadProject={onDownloadProject}
            onAddStar={onAddStar}
            onRemoveStar={onRemoveStar}
        />
    );
    const isAnyUnselected = formulas.some(f => !f.isOwned &&
        !selectedFormulas.some(id => id === f.id));
    const isAnySelected = formulas.some(f => selectedFormulas
        .some(id => id === f.id));
    return (
        <Table sortable>
            <Table.Header>
                <Table.Row>
                    <Table.Cell collapsing className="center aligned">
                        <Checkbox
                            indeterminate={isAnySelected && isAnyUnselected}
                            checked={!isAnyUnselected}
                            onChange={onToggleSelection}
                        />
                    </Table.Cell>
                    <Table.HeaderCell width={1} />
                    <Table.HeaderCell
                        sorted={orderColumn === 'name' ? sortDirection : null}
                        onClick={onSort('name')}
                    >
                        Name / Description
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={orderColumn === 'owner.fullName' ? sortDirection : null}
                        onClick={onSort('owner.fullName')}
                    >
                        Owner
                    </Table.HeaderCell>
                    <Table.HeaderCell
                        sorted={orderColumn === 'dateCreated' ? sortDirection : null}
                        onClick={onSort('dateCreated')}
                    >
                        Created
                    </Table.HeaderCell>
                    <Table.HeaderCell collapsing />
                </Table.Row>
            </Table.Header>
            <Visibility
                as="tbody"
                continuous={false}
                once={false}
                onBottomVisible={() => nextPage && nextPage()}
            >
                {formulaItems}
                {
                    formulaItems.length === 0 &&
                    <Table.Row>
                        <Table.HeaderCell colSpan={5} style={{ height: 300 }}>
                            <Header as="h2" icon textAlign="center">
                                <Icon name="list layout" circular />
                                <Header.Content>
                                    No formulas created
                                </Header.Content>
                                <Header.Subheader>
                                    Try to create a new formula or ask friends
                                    to share their formulas with you
                                </Header.Subheader>
                            </Header>
                        </Table.HeaderCell>
                    </Table.Row>
                }
            </Visibility>
        </Table>
    );
}
