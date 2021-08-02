import React from 'react'

import Node from './Node'

export default function NodeList({
    nodes,
    jsPlumbInstance,
    renderNode,
    addEndpoints,
    dragNodeEvent
}) {
    const renderNodes = nodes.map(node => {
        const style = {
            top: node.posY,
            left: node.posX
        };

        return (
            <Node
                key={node.id}
                id={node.id}
                node={node}
                style={style}
                renderNode={renderNode}
                jsPlumbInstance={jsPlumbInstance}
                isGrayed={node.isGrayed}
                addEndpoints={addEndpoints}
                dragNodeEvent={dragNodeEvent}
            />
        );
    });

    return (
        <div>
            {renderNodes}
        </div>
    )
}
