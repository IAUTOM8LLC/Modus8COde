import React, { Component } from 'react'
import classnames from 'classnames'

import * as settings from '../settings'

export default class Node extends Component {

    constructor(props) {
        super(props);

        if (!props.renderNode)
            throw 'You must specify a render node function.'

        this.removed = false;
        this.groupContainer = null;
    }
    componentDidMount() {
        const { node, jsPlumbInstance: instance,
            dragNodeEvent, resizeGroup,
            isParentCollapsed, addToGroup} = this.props;
        const nodeId = node.id.toString()

        const css = classnames({
            'grayed': node.isGrayed,
            'from-formula': resizeGroup !== undefined,
            'colapsed-connection': isParentCollapsed
        });

        instance.draggable(nodeId, {
            stop: (event) => {
                dragNodeEvent(event, node.id)
                resizeGroup && resizeGroup()
            }
        });
        if (node.isConditional) {
            node.condition.options.forEach(opt => {
                const endpoint = instance.addEndpoint(nodeId, {
                    ...settings.conditional,
                    maxConnections: 1,
                    parameters: {
                        "optionId": opt.id
                    },
                    cssClass: node.parentTaskId && 'from-formula'
                });
                if (opt.assignedTaskId) {
                    instance.connect({
                        source: endpoint,
                        target: opt.assignedTaskId.toString(),
                        type: opt.isSelected
                            ? settings.SelectedConditionConnectionType
                            : settings.CustomConnectionType,
                        overlays: [
                            ['Label', {
                                id: 'label',
                                label: opt.option,
                                cssClass: `aLabel ${css}`
                            }]
                        ],
                        cssClass: css,
                        doNotFireConnectionEvent: true
                    });
                }
                else {
                    this.setConnectionLabel(endpoint, opt.option);
                }
            });
            instance.makeTarget(nodeId, settings.target);
        } else {
            instance.makeTarget(nodeId, settings.target);
            instance.makeSource(nodeId, settings.source);
        }
        for (const id of node.parentTasks) {
            instance.connect({
                source: id.toString(),
                target: nodeId,
                type: settings.CustomConnectionType,
                cssClass: css,
                doNotFireConnectionEvent: true 
            })
        }
        if (this.props.group) {
            addToGroup(this.el);
        }
        instance.repaint(instance.getElement(nodeId));
    }
    setConnectionLabel(connection, label) {
        connection.bind('mouseover', conn =>
            conn.addOverlay([
                'Label', {
                    label,
                    location: [0.5, -0.7], // [horizontall%, vertical%]
                    id: 'connLabel'
                }
            ])
        );

        connection.bind('mouseout', conn => conn.removeOverlay('connLabel'));
    }
    shouldComponentUpdate(nextProps) {
        if (!this.props.group && nextProps.group) {
            const { addToGroup } = this.props;
            addToGroup(this.el);
        }
        if ((!this.props.isParentCollapsed && nextProps.isParentCollapsed) ||
            (this.props.isParentCollapsed && !nextProps.isParentCollapsed)) {
            const { node, jsPlumbInstance: instance } = this.props;
            const nodeId = node.id.toString();
            if (node.isConditional) {
                node.condition.options.forEach(opt => {
                    if (opt.assignedTaskId) {
                        const conn = instance.getConnections({
                            source: nodeId,
                            target: opt.assignedTaskId.toString()
                        });
                        if (nextProps.isParentCollapsed) {
                            conn[0].addClass(settings.ColapsedFormula)
                            conn[0].hideOverlays()
                        }
                        else {
                            conn[0].removeClass(settings.ColapsedFormula)
                            conn[0].showOverlays()
                        }
                    }
                });
            }
            for (const id of node.parentTasks) {
                const conn = instance.getConnections({
                    source: id.toString(),
                    target: nodeId
                });
                if (nextProps.isParentCollapsed) {
                    conn[0].addClass(settings.ColapsedFormula)
                }
                else {
                    conn[0].removeClass(settings.ColapsedFormula)
                }
            }
            instance.repaintEverything()
        }
        return true
    }
    componentWillUnmount() {
        const { node, jsPlumbInstance } = this.props;
        const connections = jsPlumbInstance.getConnections({
            target: node.id.toString()
        });
        for (const con of connections) {
            jsPlumbInstance.deleteConnection(con);
        }
        const endpoints = jsPlumbInstance.getEndpoints(node.id.toString());
        for (const endpoint of endpoints) {
            jsPlumbInstance.deleteEndpoint(endpoint);
        }
    }

    safelyRemove = () => {
        this.removed = true;
        const { node, jsPlumbInstance } = this.props;
        if (this.el && jsPlumbInstance && node.parentTaskId) {
            const group = jsPlumbInstance.getGroupFor(this.el);
            jsPlumbInstance.removeFromGroup(group, this.el);
        }
    }

    getGroupContainer() {
        if (this.groupContainer === null) {
            this.groupContainer = document.getElementById(this.props.node.parentTaskId);
        }
        return this.groupContainer;
    }

    render() {
        const {
            style,
            node,
            renderNode,
            isGrayed,
            groupTaskCount
        } = this.props;

        const NodeEl = (
            <div
                ref={c => this.el = c}
                className={`w node ${isGrayed ? 'grayed' : ''}`}
                id={node.id}
                style={style}
            >
                {renderNode(node, this.safelyRemove, groupTaskCount)}
            </div>
        );

        if (this.removed === true)
            return null;
        return NodeEl;
    }
}
