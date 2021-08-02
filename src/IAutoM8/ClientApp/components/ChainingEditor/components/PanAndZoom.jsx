import React, { Component } from 'react';

import panAndZoomHoc from '../PanAndZoomHoc'

const InteractiveDiv = panAndZoomHoc('div');

export default class PanAndZoom extends Component {
    static defaultProps = {
        minScale: 0.3,
        maxScale: 1.8,
        scaleFactor: 1.2,
        onPanAndZoom: () => { }
    }

    handlePanMove = (x, y) => {
        this.props.onPanAndZoom(x, y, this.props.scale);
    }

    getStyle = () => {
        const { scale, x, y } = this.props;
        return {
            transform: `scale(${scale}) translate(${-x * 100}%, ${-y * 100}%)`,
            transition: 'transform 100ms linear',
            border: x !== 0 || y !== 0 || scale !== 1 ? '1px dashed #f4f7f9' : 'none'
        }
    }

    render() {
        const {
            children,
            ...props
        } = this.props;

        return (
            <InteractiveDiv
                {...props}
                onPanMove={this.handlePanMove}
            >
                {children(this.getStyle())}
            </InteractiveDiv>
        );
    }
}