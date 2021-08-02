import React, { Component } from 'react'
import { Dimmer, Loader } from 'semantic-ui-react'
import { connect } from 'react-redux'
import { warning } from 'react-notification-system-redux'
import autobind from 'autobind-decorator'

import { Prompt, SimpleSegment } from '@components'

import { tasksHasCycles } from '@utils/task'

import PanAndZoom from './components/PanAndZoom'
import JsPlumbWrapper from './components/JsPlumbWrapper'
import ChainingEditorDraggableHeader from './components/ChainingEditorDraggableHeader'

import './styles/ChainingEditor.less'

@connect(
    null,
    { warning }
)
export default class ChainingEditor extends Component {
    static defaultProps = {
        onAddNewNode: () => { },
        onNodeMoved: () => { },
        onConnectionRemoved: () => { },
        onConnectionAdded: () => { },
        onOptionDetached: () => { },
        onOptionAttached: () => { },
        renderNode: null
    }

    state = {
        x: 0,
        y: 0,
        scale: 1
    }

    @autobind
    async handleRemoveConnection(connection, jsPlumbInstance) {
        const confirmed = await Prompt.confirm(
            'You are going to delete task connection',
            'Are you sure ?',
            'expand'
        );

        if (confirmed) {
            // TODO: unify this
            const params = connection.getParameters();
            if (params && params.optionId) {
                this.props.onOptionDetached({
                    optionId: params.optionId,
                    targetId: connection.targetId
                });
            } else {
                this.props.onConnectionRemoved({
                    sourceId: connection.sourceId,
                    targetId: connection.targetId
                });
            }
            jsPlumbInstance.deleteConnection(connection);
        }
    }

    handleAddConnection = (info, jsPlumbInstance) => {
        // TODO: calling stopImmediatePropagation fixed duplicate event problem
        // but it messes up UI a little, need to envistigate further.
        //event.stopPropagation();
        //event.stopImmediatePropagation();

        // prevent connection duplicates
        const con = info.connection;
        const duplicateConnections = jsPlumbInstance.select({
            source: con.sourceId,
            target: con.targetId
        });

        if (duplicateConnections.length > 1) {
            jsPlumbInstance.deleteConnection(con);
            jsPlumbInstance.repaintEverything();
            // this.props.warning({
            //     title: 'Not allowed',
            //     message: 'Duplicate connection'
            // })
            return;
        }

        const reverseConnections = jsPlumbInstance.select({
            source: con.targetId,
            target: con.sourceId
        });

        if (reverseConnections.length > 0) {
            jsPlumbInstance.deleteConnection(con);
            jsPlumbInstance.repaintEverything();
            this.props.warning({
                title: 'Not allowed',
                message: 'Child task connot depend on its parent'
            })
            return;
        }

        const revertConnection = () => {
            jsPlumbInstance.deleteConnection(con);
            jsPlumbInstance.repaintEverything();
        }

        // TODO: unify this stuff
        const params = con.getParameters();

        let edge;
        if (params && params.optionId) {
            const sourceId = this.props.nodes.find(t =>
                t.condition
                && t.condition.options.findIndex(co => co.id === params.optionId) !== -1).id;
            edge = [sourceId, Number(info.targetId)];
        } else {
            edge = [Number(info.sourceId), Number(info.targetId)];
        }

        if (tasksHasCycles(this.props.nodes, edge)) {
            revertConnection();
            this.props.warning({
                title: 'Not allowed',
                message: 'Task connections should not have any cycles'
            });
            return;
        }

        if (this.props.validateAddNewConnection) {
            this.props.validateAddNewConnection(info.sourceId, info.targetId).then(confirmed => {
                if (confirmed) {
                    this.addConnection(params, info, revertConnection);
                } else {
                    revertConnection();
                }
            });
        } else {
            this.addConnection(params, info, revertConnection);
        }
    }

    addConnection(params, info, revertConnection) {
        if (params && params.optionId) {
            this.props.onOptionAttached({
                optionId: params.optionId,
                targetId: info.targetId
            }, revertConnection);
        } else {
            this.props.onConnectionAdded({
                sourceId: info.sourceId,
                targetId: info.targetId
            }, revertConnection);
        }
    }

    handleResetPan = () => {
        this.setState({ x: 0, y: 0 });
    }

    handleResetZoom = () => {
        this.setState({ scale: 1 });
    }

    handlePanAndZoom = (x, y, scale) => {
        this.setState({ x, y, scale });
    }

    render() {
        const { scale, x, y } = this.state;
        const {
            nodes,
            connections,
            loading,
            renderNode,
            renderGroup,
            onNodeMoved,
            grayedOutNodes,
            isForProjects,
            publicCheck,
            isAdmin
        } = this.props;
        return (
            <div className="iauto-chaining-editor">
                <div className="jtk-main">
                    <SimpleSegment className="flex-editor">
                        <ChainingEditorDraggableHeader
                            xPan={x}
                            yPan={y}
                            publicCheck={publicCheck}
                            isAdmin={isAdmin}
                            scale={scale}
                            dragToElementId="right"
                            isForProjects={isForProjects}
                            onDrop={this.props.onAddNewNode}
                            onResetPan={this.handleResetPan}
                            onResetZoom={this.handleResetZoom}
                        />

                        <PanAndZoom
                            id="right"
                            x={x}
                            y={y}
                            scale={scale}
                            onPanAndZoom={this.handlePanAndZoom}
                        >
                            {(style) =>
                                <Dimmer.Dimmable dimmed={loading}>
                                    <Dimmer active={loading} inverted>
                                        <Loader>Loading</Loader>
                                    </Dimmer>
                                    <JsPlumbWrapper
                                        style={style}
                                        zoom={scale}
                                        nodes={nodes}
                                        isForProjects={isForProjects}
                                        grayedOutNodes={grayedOutNodes}
                                        connections={connections}
                                        renderNode={renderNode}
                                        renderGroup={renderGroup}
                                        onNodeMoved={onNodeMoved}
                                        onAddConnection={this.handleAddConnection}
                                        onRemoveConnection={this.handleRemoveConnection}
                                        onLoadInternalFormulaTasks={this.props.onLoadInternalFormulaTasks}
                                    />
                                </Dimmer.Dimmable>
                            }
                        </PanAndZoom>
                    </SimpleSegment>
                </div>

            </div>
        );
    }
}
