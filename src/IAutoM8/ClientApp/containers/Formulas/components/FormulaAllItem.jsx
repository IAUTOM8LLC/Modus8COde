import React from 'react'
import { Link } from 'react-router-dom'
import { Icon, Table, Header, Popup, Checkbox } from 'semantic-ui-react'

import { ModusButton, TimeAgo, WhenPermitted, Inline } from '@components'

export default function FormulaAllItem({ formula, isSelected, onView, onChange,
    onDownloadProject, onAddStar, onRemoveStar }) {

    return (
        <Table.Row className={formula.isOwned ? "iauto-formulas_owned" : ""}>
            <Table.Cell collapsing className="center aligned">
                {
                    !formula.isOwned && <Checkbox
                        checked={isSelected}
                        onChange={() => onChange(formula.id)}
                        onBlur={() => { /* looks like checkbox dispathes broken data on blur somewhy */ }}
                    />
                }
            </Table.Cell>
            <Table.Cell>
                {formula.isStarred
                    ? (
                        <Icon 
                            as="i" 
                            color="yellow"
                            className="star" 
                            size="large"
                            onClick={() => onRemoveStar(formula.id)}
                        />
                    ) : (
                        <Icon 
                            as="i" 
                            className="star empty" 
                            size="large"
                            onClick={() => onAddStar(formula.id)}
                        />
                    )
                }
            </Table.Cell>
            <Table.Cell className="wrap-word">
                <WhenPermitted rule="accessToFormulaUiEditor">
                    {
                        (permitted) => permitted && formula.isOwned
                            ? <Header size="small" as={Link} to={`/formulas/${formula.id}`}>
                                    {formula.name}
                                </Header>
                            : <Header size="small" as="a" onClick={() => onView(formula.id)}>
                                    {formula.name}
                                </Header>
                    }
                </WhenPermitted>
                <br />
                <Popup flowing hoverable
                    trigger={
                        <div className="iauto-formulas-item-category">
                            {formula.categories.join(", ")}
                        </div>}>
                    {formula.categories.map((category, i, arr) => {
                        const divider = i < arr.length - 1 && ", ";
                        return(
                            <div key={i}
                                className="iauto-formulas-hover-item-divider">
                                <div className="iauto-formulas-hover-item-category">
                                    {category}
                                </div>
                                {divider}
                            </div>)
                    })}
                </Popup>
            </Table.Cell>
            <Table.Cell>{formula.owner.fullName}</Table.Cell>
            <Table.Cell>
                <TimeAgo date={formula.dateCreated} />
            </Table.Cell>
            <Table.Cell collapsing className="center aligned">
                <Inline>
                    {
                        formula.isOwned
                            ? <ModusButton
                                disabled
                                size="mini"
                                content="Added"
                            />
                            : <ModusButton
                                filled
                                size="mini"
                                content="Add Now"
                                onClick={() => onDownloadProject(formula.id)}
                                whenPermitted="createProjectFromFormula"
                            />
                    }
                </Inline>
            </Table.Cell>
        </Table.Row>
    );
}
