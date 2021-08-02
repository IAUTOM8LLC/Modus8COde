import React, { Component, Fragment } from "react";
import SortableTree, {
    changeNodeAtPath,
    addNodeUnderParent,
    removeNode,
    getVisibleNodeCount
} from "react-sortable-tree";
import "react-sortable-tree/style.css";
import "./Table-view.less";

import FileExplorerTheme from 'react-sortable-tree-theme-minimal';

import { List, Table, Header, Button, Popup } from 'semantic-ui-react';

import { ModusButton, TimeAgo, WhenPermitted, Inline } from '@components';

import { Link } from 'react-router-dom'

import {isEqual, cloneDeep} from 'lodash';

class SortableTreeContainer extends Component {
    constructor(props) {
        super(props);
        this.state = {
            treeData: this.getNewTreeData(props.project),
            updatedNode: {
                childProjectId: null,
                parentProjectId: null
            }
        };
        this.count = getVisibleNodeCount({ treeData: this.state.treeData })
        this.inputRef = React.createRef();
    }

    // Update the component only on 3 conditions.
    componentDidUpdate(prevProps, prevState) {
        if (// 1) If a new sub-project is added or a project is deleted.
            (prevProps.project.length !== this.props.project.length) || 
            // 2) If a project is dragged and dropped to a new location.
            ( this.state.updatedNode.childProjectId &&
            this.find(this.state.updatedNode.childProjectId).parentProjectId !==
            this.find(this.state.updatedNode.childProjectId, prevProps.project).parentProjectId) ||
            // 3) If a project is edited. 
            ( this.state.updatedNode.childProjectId &&
            !isEqual(
                this.find(this.state.updatedNode.childProjectId),
                this.find(this.state.updatedNode.childProjectId, prevProps.project)))
        ) {
            this.setState(() => ({
                treeData: this.getNewTreeData(this.props.project),
                updatedNode: {childProjectId: null}
            }))
        }
    }

    addChild = (event, parentId) => {
        event.preventDefault();
        event.persist();
        const newNodeName = event.target[0].value;
        this.setState(() => ({
            updatedNode: {parentProjectId: parentId}
        }));
        this.props.onAddChildProject({
            name: newNodeName,
            parentProjectId: parentId
        });
    }

    getNewTreeData = (data, isSubLevel = false) => {
        return data.map(node => {
            if (isSubLevel || !node.parentProjectId) {
            return {...node, 
                title: node.name,
                expanded:  this.shouldNodeExpand(node.id),
                children: this.getNewTreeData(this.getTreeChildren(node.id), true)
            }
            } else {
                return false;
            }
            }).filter(Boolean)
    }

    shouldNodeExpand = (nodeId) => {
        if (!this.state) return false;

        const expandableId = this.state.updatedNode.parentProjectId;
        return nodeId === expandableId ||
        this.getTreeChildren(nodeId).map(child => child.id)
        .includes(expandableId);
    }

    getTreeChildren = (parentId) => {
        return this.props.project.filter(node => node.parentProjectId === parentId);
    }

    // isGrandChild = (node) => {
    //     return !node.parent && !node.parentProjectId 
    //     ? false : !node.parent.parent && !node.parent.parentProjectId
    //     ? false : true
    // }

    isGrandChild = (node) => {
        if (!node.parentProjectId) return false;
        const parent = this.find(node.parentProjectId);

        if (!parent || !parent.parentProjectId) return false;
        const grandParent = this.find(parent.parentProjectId);

        return grandParent ? true : false;
    }

    isChild = (node) => {
        return node.parent || node.parentProjectId;
    }

    find = (id, from = this.props.project) => {
        return from.find(node => node.id === id) || null;
    }

    onNodeMovement = ({node, nextParentNode}) => {
        const parentProjectId = nextParentNode ? nextParentNode.id : null;

        if (node.parentId !== parentProjectId) {
            this.setState(() => ({
                updatedNode: {
                    childProjectId: node.id,
                    parentProjectId
                }
            }))
            this.props.onUpdateChildProject({
                childProjectId: node.id,
                parentProjectId
            })
        }
    }

    handleEdit = (node) => {
        const {id, parentProjectId} = node;
        this.setState(() => ({
            updatedNode: {
                childProjectId: id,
                parentProjectId
            }
        }));
        this.props.onEdit(id);
    }

    handleAdd = (nodeInfo) => {
        if (nodeInfo.node.children.some(child => child.new)) return;
        
        this.setState((state) => ({
            treeData: addNodeUnderParent({
                treeData: state.treeData,
                parentKey: nodeInfo.path[nodeInfo.path.length - 1],
                expandParent: true,
                getNodeKey: this.getNodeKey,
                newNode: {
                    title: (
                        <form 
                        className="subproject__form"
                        onSubmit={e => this.addChild(e, nodeInfo.node.id)}>
                            <input 
                            className="subproject__form__text-input"
                            required
                            ref={this.inputRef} />
                            <input 
                            className="subproject__form__submit" 
                            type="submit" 
                            value="ADD" />
                            <button
                            className="subproject__form__cancel"
                            type="button"
                            onClick={() => this.clearChild(nodeInfo)}>
                                CANCEL
                            </button>
                        </form>
                    ),
                    new: true
                },
            }).treeData,
        }), () => {
            if (this.inputRef.current) {
                this.inputRef.current.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center'
                });
                this.inputRef.current.focus();
            }
            this.inputRef = React.createRef();
        })
    }

    clearChild = (nodeInfo) => {
        this.setState((state) => ({
            treeData: removeNode({
                treeData: state.treeData,
                path: [
                    ...nodeInfo.path,
                    nodeInfo.path[nodeInfo.path.length - 1] + nodeInfo.node.children.length + 1
                ],
                getNodeKey: this.getNodeKey
            }).treeData
        }))
    }

    getNodeProps = (nodeInfo) => {
        if (!nodeInfo.node.new) {
            return {
                className: this.isGrandChild(nodeInfo.node) 
                ? 'grand-child' : this.isChild(nodeInfo.node) ?
                 'child' : 'parent',
                buttons: [
                    <Fragment  key="add-sub-project">
                    <Inline>
                        <Button.Group >
                            <ModusButton
                                as={Link}
                                to={`/projects/${nodeInfo.node.id}`}
                                size="mini"
                                content="Open"
                                whenPermitted="viewKanbanBoard"
                                />
                                <ModusButton
                                    style={{display:"none"}}
                                    as={Link}
                                    to={`/projects/${nodeInfo.node.id}/calendar`}
                                    content="Calendar"
                                    whenPermitted="viewKanbanBoard"
                                />
                                {/* code commented to hide UI Editor button 12-jan by AT */}
                            {/* <ModusButton 
                                as={Link}
                                to={`/projects/${nodeInfo.node.id}/editor`}
                                size="mini"
                                content="Ui Editor"
                                whenPermitted="accessToProjectUiEditor"
                            /> */} 
                            <ModusButton
                                // as={Link}
                                // to={`/projects/${nodeInfo.node.id}/editor`}
                                size="mini"
                                content="Files"
                                //whenPermitted="accessToProjectUiEditor"
                                onClick={ () =>this.props.onShowResources(nodeInfo.node.id)} //||this.props.role === "vendor"
                            />
                            {(this.props.role === "Vendor" || this.props.role === "CompanyWorker")
                            ?"" :(<ModusButton
                                // as={Link}
                                // to={`/projects/${nodeInfo.node.id}/editor`}
                                size="mini"
                                content="Notes"
                                //whenPermitted="accessToProjectUiEditor"
                                onClick={ () =>this.props.handleNotesModal(nodeInfo.node.id)}
                            />)}
                                
                        </Button.Group>
                        <ModusButton
                            icon="iauto--add"
                            circular
                            onClick={this.isGrandChild(nodeInfo.node) ?
                                () => {} :
                                () => this.handleAdd(nodeInfo)
                            }
                            popup={this.isGrandChild(nodeInfo.node) ? 'Not Allowed' : 'Add'}
                            whenPermitted="editProject"
                        />

                        <ModusButton
                            icon="iauto--edit"
                            circular
                            onClick={() => this.handleEdit(nodeInfo.node)}
                            popup="Edit"
                            whenPermitted="editProject"
                        />
                        <ModusButton
                            icon="iauto--remove"
                            circular
                            onClick={() => this.props.onDelete(nodeInfo.node.id, this.isChild(nodeInfo.node))}
                            popup="Delete"
                            whenPermitted="deleteProject"
                        />
                    </Inline>
                    </Fragment>
                ],
            };
        }
    };

    getNodeKey = ({treeIndex}) => treeIndex;

    

    render() {
        return (
            <Table.Row>
                <Table.Cell className="sortable-project" colSpan="2" style={{height: '100%', width: "100%"}}>
                    <SortableTree
                        treeData={this.state.treeData}
                        onChange={(treeData) => this.setState({ treeData })}
                        generateNodeProps={(nodeInfo) => this.getNodeProps(nodeInfo)}
                        theme={FileExplorerTheme}
                        maxDepth={3}
                        onMoveNode={this.onNodeMovement}
                        rowHeight={45}
                    />
                 </Table.Cell>
            </Table.Row>    
        );
    }
}

export default SortableTreeContainer;
