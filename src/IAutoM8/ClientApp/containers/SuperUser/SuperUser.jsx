/* eslint-disable max-len */
import React, { Component } from "react";
import { connect } from "react-redux";
import { Table, Header } from "semantic-ui-react";
import Axios from "axios";

import { reset } from 'redux-form'

import {
    buyCredits, loadCredits
} from '@store/credits'

import { getAuthHeaders } from "@infrastructure/auth";

import setSearchQuery from "@store/layout";

import { success, error } from "react-notification-system-redux";

import ChangeEmail from "./components/ChangeEmail";
import ConfirmDialog from "./components/ConfirmDialog";

import { ModusButton, BraintreeModal } from '@components'

class SuperUser extends Component {

    constructor(props) {
        super(props);

        //this.state = {
        //    braintreeModalOpen: false
        //};

        this.openBraintreeModal = this.toggleBraintreeModal.bind(this, true);
        this.closeBraintreeModal = this.toggleBraintreeModal.bind(this, false);
    }

    handleBraintreeSubmit = (payment, userId) => {
        this.props.buyCredits(payment)
            .then(() => this.closeBraintreeModal())
            .catch((error) => {
                this.props.error({ title: error.data.message })
            });
    }

    toggleBraintreeModal = (opened, userId) => {
        if (opened) {
            this.setState({
                braintreeModalOpen: true,
                userId: userId
            });
        } else {
            this.props.reset('braintreeModal');
            this.setState({
                braintreeModalOpen: false
            });
        }
    }

    state = {
        users: [],
        searchQ: this.props.searchQuery,
        braintreeModalOpen: false,
        userId: '',
    };

    getAllUsers = () => {
        Axios.get("/api/SuperAdmin/all-users?filterSearch=", getAuthHeaders())
            .then((response) => {
                this.setState({
                    users: response.data,
                });
            })
            .catch((error) => {
                this.props.error({
                    title: error.data.message,
                });
            });
    };



    handleAddCredit = (amount, userId) => {
        Axios.get(`api/Credits/add-credits/${amount}/${userId}`, getAuthHeaders())
            .then((response) => {
                this.setState({
                    braintreeModalOpen: false
                });
                this.props.success({
                    title: "Amount credited successfully",
                })
            });
    }

    componentDidMount() {
        this.getAllUsers();
        this.props.loadCredits();
    }

    componentDidUpdate(prevProps) {
        if (prevProps.searchQuery !== this.props.searchQuery) {
            //this.getAllUsers();
            this.setState({ searchQ: this.props.searchQuery });
        }
    }

    render() {

        return (
            <React.Fragment>
                <Table celled>
                    <Table.Header>
                        <Table.Row>
                            <Table.HeaderCell>Full Name</Table.HeaderCell>
                            <Table.HeaderCell>Email</Table.HeaderCell>
                            <Table.HeaderCell>Role</Table.HeaderCell>
                            <Table.HeaderCell>Team</Table.HeaderCell>
                            <Table.HeaderCell>Action</Table.HeaderCell>
                        </Table.Row>
                    </Table.Header>

                    <Table.Body>
                        {this.state.users.map((user, index) => {
                            if (
                                user.fullName
                                    .toLowerCase()
                                    .includes(this.state.searchQ) ||
                                user.email
                                    .toLowerCase()
                                    .includes(this.state.searchQ) ||
                                user.role
                                    .toLowerCase()
                                    .includes(this.state.searchQ)
                            ) {
                                return (
                                    <Table.Row key={index}>
                                        <Table.Cell>
                                            <Header as="h2">
                                                {user.fullName}
                                            </Header>
                                        </Table.Cell>
                                        <Table.Cell>{user.email}</Table.Cell>
                                        <Table.Cell>{user.role}</Table.Cell>
                                        <Table.Cell><div>
                                            <span> Managers: <b>{user.managerCount}</b></span><br />
                                            Workers: <b>{user.workerCount}</b><br />
                                            Total: <b>{user.totalCount}</b><br />
                                        </div></Table.Cell>
                                        <Table.Cell>
                                            <ChangeEmail
                                                userEmail={user.email}
                                                onUpdate={this.getAllUsers}
                                            />                                           

                                            <ModusButton
                                                circular
                                                icon="icon iauto iauto--credit undefined dollarSign"
                                                className="ui mini circular icon button iauto"                                               
                                                popup="Add Credit"
                                                onClick={() => this.openBraintreeModal(user.userId)}
                                                whenPermitted="formulaModalWindow"
                                            />
                                            
                                            <ConfirmDialog
                                                title={`Do you want to login as ${user.fullName}?`}
                                                message="Confirm login"
                                                userEmail={user.email}
                                                action="LogInAsUser"
                                                btnProp="circular"
                                            />
                                            &nbsp; &nbsp;&nbsp;
                                            <ConfirmDialog
                                                title={`Do you want to send confirmation email for ${user.fullName}?`}
                                                message="Confirm send"
                                                userEmail={user.email}
                                                action="ResendEmail"
                                                popUpContent="Resend confirmation email"
                                                mailConfirmed={
                                                    user.emailConfirmed
                                                }
                                            />
                                        </Table.Cell>
                                    </Table.Row>
                                );
                            }
                        })}
                    </Table.Body>
                </Table>

                <BraintreeModal
                    open={this.state.braintreeModalOpen}
                    addCredit={this.handleAddCredit}
                    onClose={this.closeBraintreeModal}
                    hidePayment={"yes"}
                    userId={this.state.userId}
                />
            </React.Fragment>
        );
    }
}

export default connect((state) => ({ searchQuery: state.layout.searchQuery }), {
    success,
    error,
    setSearchQuery,
    buyCredits,
    loadCredits,
    reset
})(SuperUser);
