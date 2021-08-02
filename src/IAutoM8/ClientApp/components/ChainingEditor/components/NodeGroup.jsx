import React, { Component } from 'react'
import Cookies from 'js-cookie'
import autobind from 'autobind-decorator'
import axios from 'axios'
import classnames from 'classnames'

import { getAuthHeaders } from '@infrastructure/auth'

const CollapseStateCookieKey = id => `modus.task-group-${id}.collapsed`

import * as settings from '../settings'

import Node from './Node'

export default class NodeGroup extends Component {
    static defaultProps = {
        onLoadInternalFormulaTasks: () => Promise.resolve
    }

    constructor(props) {
        super(props);

        this.state = {
            collapsed: true,
            fetchedNodesCount: 0,
            group: null
        }
    }

    initGroup(nextProps) {
        const { isForProjects, group, dragGroupEvent, resizeGroup,
            instance, isParentCollapsed } = nextProps;
        if (!isForProjects)
            this.storeCollapsedState(true)
        if (instance.getGroups().some(g => g.id === group.id)) {
            return
        }
        const groupJsplumb = instance.addGroup({
            el: this.element,
            id: group.id,
            anchor: "Continuous",
            endpoint: "Blank",
            orphan: false,
            constrain: true,
            droppable: false,
            collapsed: isForProjects ? this.state.collapsed : true,
            dragOptions: {
                stop: (event) => {
                    dragGroupEvent(event, group)
                    if (group.parentTaskId) {
                        resizeGroup();
                    }
                    instance.repaintEverything();
                },
                drag: () => {
                    instance.repaint(groupJsplumb);
                }
            }
        });
        const nodeId = group.id.toString()
        instance.makeTarget(nodeId, settings.target);
        instance.makeSource(nodeId, settings.source);

        const css = classnames({
            'grayed': group.isGrayed,
            'from-formula': resizeGroup !== undefined,
            'colapsed-connection': isParentCollapsed
        });
        for (const id of group.parentTasks) {
            instance.connect({
                source: id.toString(),
                target: nodeId,
                type: settings.CustomConnectionType,
                cssClass: css,
                doNotFireConnectionEvent: true
            })
        }
        this.setState({
            ...this.state,
            group: groupJsplumb
        })
        instance.repaint(groupJsplumb);
    }

    addToGroup = (el) => {
        const { group: groupJsplumb } = this.state;
        this.instance.addToGroup(groupJsplumb, el)
        this.resizeGroup()
    }

    componentWillMount() {
        this.instance = this.props.instance;
    }
    componentDidMount() {
        this.initGroup(this.props);
        let id = this.props.group.id;
        const { group: { isForProjects, parentTaskId } } = this.props;
        if (!isForProjects) {
            if (parentTaskId) {
                id = Number((id + '').replace(parentTaskId, ''));
            }
            axios
                .get(`/api/formulaTask/${id}/group-members-count`, getAuthHeaders())
                .then(({ data }) => this.setState({
                    ...this.state,
                    fetchedNodesCount: data
                }))
        }
    }

    componentWillUnmount() {
        const { group, instance } = this.props;
        const connections = instance.getConnections({
            target: group.id.toString()
        }).concat(instance.getConnections({
            source: group.id.toString()
        }));
        for (const con of connections) {

            instance.deleteConnection(con);
        }
        instance.repaintEverything();
    }

    resizeGroup = (immediately = true, updateParent = true) => {
        const { group, instance, resizeGroup } = this.props;
        const { group: groupJsplumb } = this.state;
        NodeGroup.autosize(groupJsplumb, instance, immediately, group.groupTasks,
            () => {
                if (updateParent)
                    resizeGroup && resizeGroup(immediately);
            })
    }

    @autobind
    async toggleGroup() {
        const { collapsed,
            group: groupJsplumb } = this.state;

        if (!groupJsplumb)
            return;

        const { group, instance } = this.props

        if (collapsed) {
            this.instance.expandGroup(groupJsplumb);
        } else {
            this.instance.collapseGroup(groupJsplumb);
        }
        this.storeCollapsedState(groupJsplumb.collapsed)

        if (collapsed) {
            if (group.groupTasks.length === 0) {
                await this.props.onLoadInternalFormulaTasks(group.internalFormulaProjectId, group.id);
            }
        }
        this.setState({
            ...this.state,
            collapsed: groupJsplumb.collapsed
        })
        if (!groupJsplumb.collapsed)
            this.resizeGroup();
        const { resizeGroup } = this.props;

        resizeGroup && resizeGroup(false);
    }

    storeCollapsedState(collapsed) {
        Cookies.set(CollapseStateCookieKey(this.props.group.id), collapsed, { expires: 365 });
    }

    isCollapsed() {
        const cookie = Cookies.get(CollapseStateCookieKey(this.props.group.id));
        return cookie === "true";
    }
    shouldComponentUpdate(nextProps) {
        this.collapseConnectionIfNeed(nextProps);
        if (this.props.isParentCollapsed && !nextProps.isParentCollapsed) {
            if (!this.state.collapsed) {
                this.resizeGroup(true, false)
            }
        }
        return true
    }
    collapseConnectionIfNeed = (nextProps) => {
        if ((!this.props.isParentCollapsed && nextProps.isParentCollapsed) ||
            (this.props.isParentCollapsed && !nextProps.isParentCollapsed)) {

            const { group, instance } = this.props;
            const nodeId = group.id.toString();
            for (const id of group.parentTasks) {
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
            for (const id of group.childTasks) {
                const conn = instance.getConnections({
                    source: nodeId,
                    target: id.toString()
                });
                if (nextProps.isParentCollapsed) {
                    conn[0].addClass(settings.ColapsedFormula)
                }
                else {
                    conn[0].removeClass(settings.ColapsedFormula)
                }
            }
        }
    }
    render() {
        const { group, renderGroup, renderNode,
            instance, isForProjects, addEndpoints,
            dragGroupEvent, dragNodeEvent,
            isParentCollapsed } = this.props;
        const { group: groupJsplumb, collapsed } = this.state;

        const style = {
            top: group.posY,
            left: group.posX
        };
        if (collapsed) {
            style.height = 'auto';
        }
        const renderNodeEl = (node) => {
            const styleNode = {
                top: node.posY,
                left: node.posX
            };
            
            return (
                <Node
                    key={node.id}
                    id={node.id}
                    node={node}
                    style={styleNode}
                    renderNode={renderNode}
                    jsPlumbInstance={instance}
                    isGrayed={node.isGrayed}
                    addToGroup={this.addToGroup}
                    addEndpoints={addEndpoints}
                    dragNodeEvent={dragNodeEvent}
                    group={groupJsplumb}
                    resizeGroup={this.resizeGroup}
                    isParentCollapsed={isParentCollapsed || collapsed}
                    groupTaskCount={group.groupTasks.length}
                />
            );
        };
        const renderGroupEl = (node) => {

            return (
                <NodeGroup
                    key={node.id}
                    group={node}
                    renderGroup={renderGroup}
                    renderNode={renderNode}
                    onLoadInternalFormulaTasks={this.props.onLoadInternalFormulaTasks}
                    instance={instance}
                    isForProjects={isForProjects}
                    addEndpoints={addEndpoints}
                    dragGroupEvent={dragGroupEvent}
                    dragNodeEvent={dragNodeEvent}
                    resizeGroup={this.resizeGroup}
                    isParentCollapsed={isParentCollapsed || collapsed}
                />
            );
        };
        
        return (
            <div
                id={group.id}
                className="task-group"
                style={style}
                ref={c => this.element = c}
                hidden={isParentCollapsed === undefined ? false : isParentCollapsed}
            >
                {

                    renderGroup(
                        group,
                        group.groupTasks.length || this.state.fetchedNodesCount,
                        this.toggleGroup,
                        collapsed)
                }
                {
                    group.groupTasks.map(t =>
                        t.formulaId === null || t.internalFormulaProjectId === null ?
                            renderNodeEl(t) :
                            renderGroupEl(t))
                }
            </div>
        )
    }
}
NodeGroup.autosize = function (g, instance, immediately, nodes,
    callback) {
    const _autosize = () => {
        const members = g.getMembers();
        const dragArea = g.getDragArea();
        dragArea.setAttribute("adjusting", true);

        const setSize = (w, h) => {
            dragArea.style.width = `${w}px`;
            dragArea.style.height = `${h}px`;
        }
        const groupDOM = document.getElementById(g.id);
        if (groupDOM) {
            for (const el of groupDOM.getElementsByClassName('task-group')) {
                if (members.filter(t => t.id === el.id).length === 0 &&
                    nodes.filter(t => t.id == el.id).length === 1) {
                    members.push(el)
                }
            }
        }

        members.sort(function (a, b) {
            const nodeA = nodes.filter(t => t.id == a.id)[0];
            const nodeB = nodes.filter(t => t.id == b.id)[0];

            if (nodeA.posY + a.offsetHeight < nodeB.posY + b.offsetHeight) {
                return -1;
            }

            if (nodeA.posY + a.offsetHeight > nodeB.posY + b.offsetHeight) {
                return 1;
            }

            return 0;
        });

        const changePosition = (member, node, memberIndex) => {
            for (const otherMember of members) {
                const premMemberNode = nodes.filter(t => t.id == otherMember.id)[0];

                const x1 = node.posX;
                const y1 = node.posY;
                const h1 = member.offsetHeight;
                const w1 = member.offsetWidth;
                const b1 = y1 + h1;
                const r1 = x1 + w1;
                const x2 = premMemberNode.posX;
                const y2 = premMemberNode.posY;
                const h2 = otherMember.offsetHeight;
                const w2 = otherMember.offsetWidth;
                const b2 = y2 + h2;
                const r2 = x2 + w2;

                if (members.indexOf(otherMember) === memberIndex) {
                    continue;
                }

                if (premMemberNode) {
                    let hasOverlapping;
                    if (b1 < y2 || y1 > b2 || r1 < x2 || x1 > r2) {
                        hasOverlapping = false;
                    } else {
                        hasOverlapping = true;
                    }

                    if (hasOverlapping) {
                        const newPosition = node.posY +
                            member.offsetHeight + 50;
                        otherMember.style.top = `${newPosition}px`;
                        otherMember.style.left = `${premMemberNode.posX}px`;
                        premMemberNode.posY = newPosition;
                        maxBottom = Math.max(maxBottom, newPosition + otherMember.offsetHeight);
                    }
                }
            }
        }

        const dragAreaRect = dragArea.getBoundingClientRect();
        let maxRight = 0;
        let maxBottom = 0;
        for (const member of members) {
            const node = nodes.filter(t => t.id == member.id)[0];
            if (node) {
                maxRight = Math.max(maxRight, node.posX + member.offsetWidth);
                maxBottom = Math.max(maxBottom, node.posY + member.offsetHeight);

                const memberIndex = members.indexOf(member);
                changePosition(member, node, memberIndex);
                member.style.left = `${node.posX}px`;
                member.style.top = `${node.posY}px`;
            }
        }

        const pad = 20;

        setSize(maxRight + pad, maxBottom + pad);

        if (instance) {
            setTimeout(() => {
                instance.updateOffset({
                    elId: g.id,
                    recalc: true
                })
                for (const member of members) {
                    instance.updateOffset({
                        elId: member.id,
                        recalc: true
                    })
                    instance.repaint(member);
                }
                instance.recalculateOffsets(dragArea.id);
                instance.repaint(dragArea);
                instance.repaint(g);
                dragArea.setAttribute("adjusting", false);
                callback && callback();
            }, 300);
        }
    }
    if (!g.collapsed) {
        setTimeout(() => _autosize(), immediately ? 0 : 300)
    }
}
