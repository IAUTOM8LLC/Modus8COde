import React, { Component } from 'react'
import { Popup } from 'semantic-ui-react'
import dragula from 'react-dragula'

import { ModusIcon } from '@components'

import '../styles/ChainingEditorDraggableHeader.less'

export default class ChainingEditorDraggableHeader extends Component {
    static defaultProps = {
        onDrag: () => { },
        onDragEnd: () => { }
    }

    componentDidMount() {
        this.drake = this.initDragula();
    }

    componentWillUnmount() {
        if (this.drake) {
            this.drake.destroy();
        }
    }

    initDragula() {
        const dropArea = document.getElementById(this.props.dragToElementId);
        const drake = dragula(
            [document.querySelector('#drag-items'), dropArea],
            {
                copy: true,
                moves: (el) => el.className.includes('task-item')
            });

        drake
            .on('drag', this.props.onDrag)
            .on('drop', (element) => {
                const { dataset } = element;
                const [x, y, isValid] = this.processDrop();

                drake.cancel(true);

                if (dataset.type && isValid) {
                    this.props.onDrop(x, y, dataset.type);
                }
            })
            .on('dragend', () => {
                drake.cancel(true);
                this.props.onDragEnd();
            });

        return drake;
    }

    processDrop = () => {
        const { xPan, yPan/*, scale*/ } = this.props;

        const itemRect = document.querySelector('.gu-mirror').getBoundingClientRect();
        const dropArea = document.getElementById(this.props.dragToElementId);
        const canvasRect = dropArea.getBoundingClientRect();

        // TODO: fix zoom affect
        let x = itemRect.left - canvasRect.left;
        let y = itemRect.top - canvasRect.top;

        const isValid =
            x >= 0
            && y >= 0
            && x <= canvasRect.width
            && y <= canvasRect.height;

        const offsetX = xPan * canvasRect.width;
        const offsetY = yPan * canvasRect.height;

        x = Math.floor(x + offsetX);
        y = Math.floor(y + offsetY);

        return [x, y, isValid];
    }

    render() {
        const flag = this.props.publicCheck != undefined &&
            this.props.publicCheck.type === "public" && this.props.isAdmin === false
        const publicNav = flag ? <div></div> :
                        (<div id="drag-items" className="chaining-editor-header__items">
                        <a className="item task-item" data-type="plain">
                            <ModusIcon name="task one-time" />
                            One-time task
                        </a>
                        <a className="item task-item" data-type="recurrent">
                            <ModusIcon name="task recurring" />
                            Recurring task
                        </a>
                        <a className="item task-item" data-type="conditional">
                            <ModusIcon name="task conditional" />
                            Conditional task
                        </a>
                        <a className="item task-item" data-type="conditional,recurrent">
                            <ModusIcon name="task conditional-recurring" />
                            Conditional-Recurring task
                        </a>
                        <a className="item task-item" data-type="interval">
                            <ModusIcon name="task interval" />
                            Interval
                        </a>
                        <a className="item task-item" data-type="formula">
                            <ModusIcon name="task formula" />
                            Add a Formula
                        </a>
    </div>)
    const publicIcon = flag ? <div></div> :
                        (<div className="chaining-editor-header__buttons">
                        <Popup
                            content="Reset zoom"
                            position="top center"
                            trigger={
                                <a className="icon ui--popup item" onClick={this.props.onResetZoom}>
                                    <ModusIcon name="task reset-zoom" />
                                </a>
                            }
                        />
                        <Popup
                            content="Reset pan"
                            position="top center"
                            trigger={
                                <a className="icon icon ui--popup item" onClick={this.props.onResetPan}>
                                    <ModusIcon name="task reset-pan" />
                                </a>
                            }
                        />
                    </div>)
        return (
            <div id="drag" className="chaining-editor-header">
                
                {publicNav}
                {publicIcon}
                
            </div>
        );
    }
}
