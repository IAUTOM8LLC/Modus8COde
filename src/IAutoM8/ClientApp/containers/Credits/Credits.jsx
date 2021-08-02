
import React, { Component } from 'react'

import { Header, Divider, Card } from 'semantic-ui-react'

import { error } from 'react-notification-system-redux'

import { connect } from 'react-redux'

import moment from 'moment'

import { reset } from 'redux-form'

import {
    buyCredits, loadCredits
} from '@store/credits'

import { ModusButton, BraintreeModal } from '@components'

import CreditItem from './components/CreditItem'

import './Credits.less'

@connect(
    (state, props) => ({
        credits: state.credits.credits
    }),
    {
        buyCredits,
        loadCredits,
        reset
    }
)
export default class Credits extends Component {

    constructor(props) {
        super(props);

        this.state = {
            braintreeModalOpen: false
        };

        this.openBraintreeModal = this.toggleBraintreeModal.bind(this, true);
        this.closeBraintreeModal = this.toggleBraintreeModal.bind(this, false);
    }

    componentDidMount() {
        this.props.loadCredits();
    }

    toggleBraintreeModal = (opened) => {
        if (opened) {
            this.setState({
                braintreeModalOpen: true
            });
        } else {
            this.props.reset('braintreeModal');
            this.setState({
                braintreeModalOpen: false
            });
        }
    }

    handleBraintreeSubmit = (payment) => {
        this.props.buyCredits(payment)
            .then(() => this.closeBraintreeModal())
            .catch((error) => {
                this.props.error({ title: error.data.message })
            });
    }

    handleOpenBraintreeModal = () => {
        this.openBraintreeModal();
    }

    render() {
        const { credits } = this.props;
        const format = 'MMM DD, YYYY  h:mmA';

        return (
            <div>
                <Header content="Your credits" />
                <Divider />

                {credits &&
                    <div style={{ display: "inline-block" }}>
                        <ul className="credits-container">
                            <CreditItem
                                title="Available credits"
                                type="available"
                                availableCredits={credits.availableCredits}
                                lastAdded={moment(credits.lastUpdate).format(format)}
                            />
                            <CreditItem
                                title="Reserved credits"
                                type="reserved"
                                availableCredits={credits.reservedCredits}
                                prepaidTasksCount={credits.prepaidTasksCount}
                            />
                        </ul>
                        <ModusButton
                            filled
                            className="credits-button"
                            content="Add credits now"
                            onClick={this.openBraintreeModal}
                        />
                    </div>
                }

                <BraintreeModal
                    open={this.state.braintreeModalOpen}
                    onSubmit={this.handleBraintreeSubmit}
                    onClose={this.closeBraintreeModal}
                />
            </div>
        );
    }
}
