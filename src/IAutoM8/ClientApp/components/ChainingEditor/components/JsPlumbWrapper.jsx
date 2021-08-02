import React, { Component } from 'react'
import { connect } from 'react-redux'
import { jsPlumb } from 'jsplumb'

import * as settings from '../settings'

import NodeList from './NodeList'
import NodeGroup from './NodeGroup'

@connect(store => ({
    forceRedraw: store.layout.forceRedrawUIEditor
}))
export default class JsPlumbWrapper extends Component {
    static defaultProps = {
        nodes: [],
        connections: [],
        onNodeMoved: () => { },
        renderNode: null
    }
    parsedNodes = {
        nodes: [],
        groups: []
    }

    shouldComponentUpdate(nextProps) {
        if (this.parsedNodes.nodes.length === 0 && this.parsedNodes.groups.length === 0 &&
            nextProps.nodes.length !== 0) {
            this.parsedNodes = this.processNodes(nextProps)
            return true
        }
        else if (this.props.nodes.length !== nextProps.nodes.length) {
            this.parsedNodes = this.processNodes(nextProps)
            return true
        }
        else if (this.props.grayedOutNodes.length !== nextProps.grayedOutNodes.length) {
            this.parsedNodes = this.processNodes(nextProps)
            return true
        }
        else {
            const { nodes } = this.props;
            if (nextProps.nodes.some((n, i) => n.posX !== nodes[i].posX || n.posY !== nodes[i].posY ||
                n.status !== nodes[i].status || n.proccessingUserName !== nodes[i].proccessingUserName ||
                n.reviewingUserName !== nodes[i].reviewingUserName ||
                n.title !== nodes[i].title)) {
                this.parsedNodes = this.processNodes(nextProps)
                return true
            }
            else if (this.props.style !== nextProps.style) {
                return true
            }
            else
                return false
        }
    }
    componentDidMount() {
        if (!this.instance) {
            jsPlumb.ready(this.initJsPlumb);
        }
    }
    initJsPlumb = () => {
        const instance = jsPlumb.getInstance(settings.instance);
        instance.registerConnectionType(
            settings.CustomConnectionType,
            settings.connectionType);

        instance.registerConnectionType(
            settings.SelectedConditionConnectionType,
            settings.selectedCondition);

        instance.bind('click', (c) => {
            if (this.props.isForProjects) {
                this.props.onRemoveConnection(c, instance);
            }
            else {
                const parentTaskId = Number(c.sourceId);
                if (this.props.nodes.some((n) => n.id === parentTaskId))
                    this.props.onRemoveConnection(c, instance);
            }
        });

        instance.bind('connection', (info) => {
            const parentTaskId = Number(info.sourceId);
            if (this.props.nodes.filter((n) => n.id === parentTaskId)[0]
                .parentTaskId) {
                info.connection.addClass('from-formula')
            }
            this.props.onAddConnection(info, instance);
        });

        instance.bind('group:collapse', (g) => {
            setTimeout(() => {
                instance.updateOffset({
                    elId: g.group.id,
                    recalc: true
                })
                instance.repaintEverything();
            }, 350);
        });

        this.instance = instance;
    }
    calculatePos = (event, id) => {
        const { nodes } = this.props;
        const isNotFromGroup = nodes.some(n => n.id === id && n.parentTaskId === null);
        const padLeft = isNotFromGroup ? Number.MIN_SAFE_INTEGER : settings.DefaultPadding.left;
        const padTop = isNotFromGroup ? Number.MIN_SAFE_INTEGER : settings.DefaultPadding.top;
        return {
            posX: Math.floor(event.pos[0] < padLeft ? padLeft : event.pos[0]),
            posY: Math.floor(event.pos[1] < padTop ? padTop : event.pos[1])
        }
    }
    dragNodeEvent = (event, id) => {
        const update = []


        update.push({
            id: id,
            ...this.calculatePos(event, id)
        });
        this.props.onNodeMoved(update);
    }
    dragGroupEvent = (event, group) => {

        const update = []
        update.push({
            id: group.id,
            ...this.calculatePos(event, group.id)
        });

        this.props.onNodeMoved(update);
    }

    processNodes(props) {
        const { nodes: allNodes, grayedOutNodes = [] } = props;
        
        const findParent = (node, id) => {
            if (node.id === id) {
                return node;
            }
            else if (node.groupTasks.length !== 0) {
                let result = null;
                for (const groupNode of node.groupTasks) {
                    if (result)
                        break;
                    result = findParent(groupNode, id);
                }
                return result;
            }
            return null;
        }
        const processNodes = {
            nodes: [],
            groups: []
        };
        for (const node of allNodes) {
            const isGrayed = grayedOutNodes.includes(node.id)
            if (node.parentTaskId === null) {
                if (node.formulaId === null ||
                    node.internalFormulaProjectId === null)
                    processNodes.nodes.push({
                        ...node,
                        isGrayed
                    })
                else
                    processNodes.groups.push({
                        ...node,
                        isGrayed,
                        groupTasks: []
                    })
            }
            else {
                let result = null;
                for (const treeNode of processNodes.groups) {
                    if (result)
                        break;
                    else
                        result = findParent(treeNode, node.parentTaskId);
                }
                result.groupTasks.push({
                    ...node,
                    isGrayed,
                    groupTasks: []
                });
            }
        }
        return processNodes;
    }


    componentWillUnmount() {
        this.destroyGraph();
    }


    destroyGraph = () => {
        if (this.instance) {
            for (const groupId in this.instance._groups) {
                const group = this.instance._groups[groupId];

                // store jsPlumb data
                const tempFunc = group.getEl;
                const tempCollapsed = group.collapsed;

                // mock
                group.collapsed = false
                group.getEl = () => document.createElement('div');

                // DOM operations
                this.instance.removeGroup(group, false, false, false);

                // restore jsPlumb
                group.getEl = tempFunc;
                group.collapsed = tempCollapsed;
            }
            this.instance.unbind('connection');
            this.instance.unbind('click');

            this.instance.deleteEveryEndpoint();
            this.instance.deleteEveryConnection();
            this.instance = null
        }
    }
    render() {
        const {
            style,
            renderNode,
            renderGroup,
            isForProjects
        } = this.props;
        return (
            <div
                id="canvas"
                style={style}
                className="jtk-surface"
            >
                {
                    this.parsedNodes.groups.map(g =>
                        <NodeGroup
                            key={g.id}
                            group={g}
                            renderGroup={renderGroup}
                            renderNode={renderNode}
                            onLoadInternalFormulaTasks={this.props.onLoadInternalFormulaTasks}
                            instance={this.instance}
                            isForProjects={isForProjects}
                            addEndpoints={this.addEndpoints}
                            dragGroupEvent={this.dragGroupEvent}
                            dragNodeEvent={this.dragNodeEvent}
                        />
                    )
                }
                <NodeList
                    nodes={this.parsedNodes.nodes}
                    renderNode={renderNode}
                    jsPlumbInstance={this.instance}
                    addEndpoints={this.addEndpoints}
                    dragNodeEvent={this.dragNodeEvent}
                />
            </div>
        );
    }
}
