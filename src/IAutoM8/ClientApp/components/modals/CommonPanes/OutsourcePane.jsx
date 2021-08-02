import React, { Component } from 'react'

import { formatMoney } from '@utils/formatMoney'

import InfiniteScroll from 'react-infinite-scroll-component';

import {
    Inline
} from '@components'

import './OutsourcePane.less'

export default class OutsourcePane extends Component {
    componentDidMount() {
        this.props.fetchData();
    }
    render() {
        const { items, fetchData, hasMore,
            endMessage, node, credits } = this.props;
        return (
            <div className="task-details-modal__pane">
                {credits &&
                    <Inline className="credits-information">
                        <label>Reserved Credits: $
                    {formatMoney(credits.reservedCredits, 2, ".", " ")}</label>
                        <label>Available Credits: $
                    {formatMoney(credits.availableCredits, 2, ".", " ")}</label>
                    </Inline>
                }
                <InfiniteScroll
                    dataLength={items.length}
                    next={fetchData}
                    hasMore={hasMore}
                    height={400}
                    endMessage={
                        <p className="end-of-scroll">
                            <b>{endMessage}</b>
                        </p>
                    }
                >
                    {items.map((i, index) => node(i, index))}
                </InfiniteScroll>
            </div>)
    }
}
