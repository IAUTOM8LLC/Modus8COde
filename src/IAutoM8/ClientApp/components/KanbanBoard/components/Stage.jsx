import React from 'react'

import splitToLowerCase from '@utils/splitToLowerCase'

import CardItem from './CardItem'

export default function Stage({
    stage,
    title,
    blocks,
    highlightedBlocks,
    dragulaDecorator,
    onEditCard,
    onChangeProcessingUser,
    onStopOutsource,
    onDeleteCard,
    onReviewCard,
    onDoCard,
    onDoVendorCard,
    grayedOutNodes,
    onOutsourceTabOpen,
    canAssignVendor,
    isVendor
}) {
    return (
        <li className={`drag-column drag-column--${splitToLowerCase(stage)}`}>
            <span className="drag-column--header">
                <h2>
                    <strong>{title}</strong>{` (${blocks.length})`}
                </h2>
            </span>

            <ul className="drag-inner-list" ref={dragulaDecorator} data-status={stage}>
                {
                    blocks.map(block =>
                        <CardItem
                            {...block}
                            key={block.id}
                            highlighted={highlightedBlocks.includes(block.id)}
                            onDoCard={onDoCard}
                            onDoVendorCard={onDoVendorCard}
                            onReviewCard={onReviewCard}
                            onEditCard={onEditCard}
                            onChangeProcessingUser={onChangeProcessingUser}
                            onStopOutsource={onStopOutsource}
                            onDeleteCard={onDeleteCard}
                            isGrayed={grayedOutNodes.includes(block.id)}
                            onOutsourceTabOpen={onOutsourceTabOpen}
                            hasVendor={canAssignVendor && block.hasVendor}
                            isVendor={isVendor}
                        />
                    )
                }
            </ul>
        </li>
    );
}
