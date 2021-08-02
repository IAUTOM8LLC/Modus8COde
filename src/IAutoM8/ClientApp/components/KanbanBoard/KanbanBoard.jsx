import React, { Component } from 'react'
import dragula from 'react-dragula'

import { sortTasksByParentChildRelation } from '@utils/task'

import Stage from './components/Stage'

import './KanbanBoard.less'

export default class KanbanBoard extends Component {
    static defaultProps = {
        validateStatusChange: () => true
    }
    
    static stages = [
        {
            value: 'New',
            title: 'Upcoming Tasks'
        },
        {
            value: 'InProgress',
            title: 'To Do'
        },
        {
            value: 'NeedsReview',
            title: 'Needs Review'
        },
        {
            value: 'Completed',
            title: 'Completed'
        }
    ];

    state = {
        stageInstances: [],
        highlightIds: [],
        drake: null
    }
    
    componentDidUpdate(prevProps) {
        const { blocks } = this.props;
        const { stageInstances, drake } = this.state;

        const initialUpdate =
            !drake
            && stageInstances.length === KanbanBoard.stages.length
            && blocks.length > 0;

        const reupdate = prevProps.blocks.length !== blocks.length;

        if (initialUpdate || reupdate)
            this.initDragula();
    }

    componentWillUnmount() {
        this.tryDestroyDragula();
    }

    initDragula = () => {
        this.tryDestroyDragula();

        const { blocks } = this.props;
        const options = { copy: true };
        const drake = dragula(this.state.stageInstances, options);

        this.setState({ drake });

        drake.on('drag', el => {
            if (el) {
                // TODO: fix this
                // el.classList.add('is-moving');
                const movedBlock = blocks.find(b => b.id === Number(el.dataset.blockId));
                if (movedBlock) {
                    this.setState({
                        highlightIds: [
                            ...(movedBlock.parentTasks || []),
                            ...(movedBlock.conditionalParentTasks || [])
                        ]
                    });
                }
            }

        }).on('drop', (block, list) => {
            if (!block || !list) {
                drake.cancel(true);
                return;
            }

            const blockId = Number(block.dataset.blockId);

            const task = this.props.blocks.find(t => t.id === blockId);
            let newStatus;
            if (task.reviewingUserIds &&
                task.reviewingUserIds.length > 0 &&
                list.dataset.status === "Completed"
                && task.status === "InProgress"
            ) {
                newStatus = "NeedsReview";
            } else {
                newStatus = list.dataset.status;
            }

            this.props.validateStatusChange(blockId, newStatus)
                .then(() => {
                    list.removeChild(block);
                    this.props
                        .onCardStatusUpdate(blockId, newStatus)
                        .catch(error => {
                            drake.cancel(true);
                            this.props.onUpdateStatusError(error, blockId);
                        });
                })
                .catch(error => {
                    list.removeChild(block);
                    drake.cancel(true);
                    this.props.onUpdateStatusError(error, blockId);
                });

            return;

        }).on('dragend', el => {
            if (el) {
                el.classList.remove('is-moving');
            }

            this.setState({
                highlightIds: []
            });
        });
    }

    tryDestroyDragula() {
        if (this.state.drake) {
            this.state.drake.destroy();
        }
    }

    dragulaDecorator = (componentBackingInstance) => {
        if (componentBackingInstance) {
            this.setState(prevState => ({
                stageInstances: [...prevState.stageInstances, componentBackingInstance]
            }));
        }
    }

    render() {
        const {
            blocks,
            onEditCard,
            onChangeProcessingUser,
            onStopOutsource,
            onDeleteCard,
            onReviewCard,
            onDoCard,
            onDoVendorCard,
            grayedOutNodes,
            onOutsourceTabOpen,
            isVendor,
            canAssignVendor
        } = this.props;
        return (
            <div className="drag-container">
                <ul className="drag-list">
                    {
                        KanbanBoard.stages.map(stage =>
                            <Stage
                                key={stage.value}
                                stage={stage.value}
                                title={stage.title}
                                blocks={sortTasksByParentChildRelation(
                                    blocks.filter(b => b.status === stage.value))}
                                highlightedBlocks={this.state.highlightIds}
                                dragulaDecorator={this.dragulaDecorator}
                                onStopOutsource={onStopOutsource}
                                onDoCard={onDoCard}
                                onDoVendorCard={onDoVendorCard}
                                onReviewCard={onReviewCard}
                                onEditCard={onEditCard}
                                onChangeProcessingUser={onChangeProcessingUser}
                                onDeleteCard={onDeleteCard}
                                grayedOutNodes={grayedOutNodes}
                                onOutsourceTabOpen={onOutsourceTabOpen}
                                canAssignVendor={canAssignVendor}
                                isVendor={isVendor}
                            />
                        )
                    }
                </ul>
            </div>
        );
    }
}
