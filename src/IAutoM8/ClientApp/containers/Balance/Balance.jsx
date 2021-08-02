import axios from "axios";

import React, { Component } from 'react'

import { Header, Divider, Card } from 'semantic-ui-react'

import { connect } from 'react-redux'

import { getAuthHeaders } from "@infrastructure/auth";

import moment from 'moment'

import { success, error, info } from "react-notification-system-redux";

import { reset } from 'redux-form'

import {
    loadBalance, transferRequest, loadActiveTransferRequest
} from '@store/credits'

import BalanceItem from './components/BalanceItem'

import { VendorBalanceModal } from "@components";

import './Balance.less'

@connect(
    (state, props) => ({
        loading: state.credits.loading,
        balance: state.credits.balance,
        activeTransferRequest: state.credits.activeTransferRequest
    }),
    {
        loadBalance,
        transferRequest,
        loadActiveTransferRequest,
        reset,
        success
    }
)
export default class Balance extends Component {
    state = {
        showVendorBalancePopup: false,
    }
    componentDidMount() {
        this.props.loadBalance()
            .then(() => this.props.loadActiveTransferRequest());
    }

    requestFundTransfer = (amount) => {
        if (amount === "") {
            this.props.error({
                title: "Please add amount",
            })
            return false;
        }

        if (this.props.balance.unpaid < amount) {
            this.props.error({
                title: "You can maximum request for amount $" +
                    (this.props.balance.unpaid - this.props.balance.requestedAmount),
            })
            return false;
        }

        axios
            .get(`/api/credits/add-fund-request/${amount}`, getAuthHeaders())
            .then((response) => {
                this.setState({
                    showVendorBalancePopup: false,
                });
                this.props.success({
                    title: "Fund request sent successfully",
                })
                // this.toggleModal(true);
                this.props.loadBalance();
            });
    };

    redirectToProfile = () => {
        window.open(window.location.origin + '/profile', "_blank");
    }

    handleTransferRequest = () => {

        if (!this.props.balance.payoneerEmailAvailable) {
            this.setState({
                showVendorBalancePopup: true
            });
            return false;
        }
        this.props.transferRequest();
    }

    hideFormulaModal = () => {
        this.setState({
            showVendorBalancePopup: false
        });
    }

    render() {
        const { balance, activeTransferRequest } = this.props;
        const format = 'MMM DD, YYYY  h:mmA';
        return (
            <div>
                <Header content="Your balance" />
                <Divider />

                {balance &&
                    <div>
                        <ul className="balance-container">
                            <BalanceItem
                                title="Unpaid earnings"
                                type="unpaid"
                                handleTransferRequest={this.handleTransferRequest}
                                credits={balance.unpaid}
                                requestedAmount={balance.requestedAmount}
                                loading={this.props.loading}
                                lastAdded={moment(balance.lastTransfer).format(format)}
                                activeTransferRequest={activeTransferRequest}
                            //showFormulaModal={this.showFormulaModal}
                            />
                            <BalanceItem
                                title="Expected payments"
                                type="expected"
                                credits={balance.expected}
                                tasks={balance.tasks}
                            />
                            <BalanceItem
                                title="Total earnigns"
                                type="total"
                                credits={balance.total}
                                tasksFinishedCount={balance.finishedTasksCount}
                            />
                        </ul>
                    </div>
                }
                <VendorBalanceModal
                    open={this.state.showVendorBalancePopup}
                    onClose={this.hideFormulaModal}
                    requestFundTransfer={this.requestFundTransfer}
                    maxAmount={balance && balance.unpaid != null ?
                        balance.unpaid - balance.requestedAmount : 0}
                    amount={""}
                    payoneerEmailAvailable={balance && balance.payoneerEmailAvailable}
                    redirectToProfile={this.redirectToProfile}
                />
            </div>
        );
    }
}
