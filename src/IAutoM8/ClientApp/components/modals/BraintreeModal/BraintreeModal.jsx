import React, { Component } from 'react'

import { connect } from 'react-redux'

import { reduxForm, change, formValueSelector } from 'redux-form'

import { Modal, Form, Label } from 'semantic-ui-react'

import { required, number, minValue } from '@utils/validators'

import DropIn from 'braintree-web-drop-in-react'

import autobind from 'autobind-decorator'

import { error } from 'react-notification-system-redux'

import {
    TextInput,
    DropdownInput,
    WhenPermitted,
    ModusButton
} from '@components'

import {
    loadToken
} from '@store/credits'

import editModalHoc from '../../common/editModalHoc'

const braintreeFormValueSelector = formValueSelector('braintreeModal');

const initialOptions = [{
    key: 10,
    value: 10,
    text: '$10'
},
{
    key: 25,
    value: 25,
    text: '$25'
},
{
    key: 50,
    value: 50,
    text: '$50'
},
{
    key: 100,
    value: 100,
    text: '$100'
}];

@editModalHoc
@reduxForm({
    form: 'braintreeModal',
    enableReinitialize: true
})

@connect(
    state => ({
        loading: state.credits.loading,
        credits: state.credits.credits,
        amount: Number(braintreeFormValueSelector(state, 'amount')),
        token: state.credits.token
    }),
    {
        loadToken,
        error
    }
)
export default class BraintreeModal extends Component {
    instance;
    state = {
        clientToken: null,
        options: [...initialOptions]
    };;

    constructor(props) {
        super(props);
        this.handleAddAmountValue = this.handleAddAmount.bind(this, 'amount');
    }

    handleClose = () => {
        this.setState({ options: [...initialOptions] });
        this.props.onClose();
    }

    componentDidMount() {
        this.props.loadToken()
            .then(() => {
                this.setState({
                    clientToken: this.props.token
                });
            });
    }

    handleAddAmount(field, e, { value }) {
        if (isNaN(Number(value)) || value < 0) {
            return;
        }
        if (this.state.options.find(t => t.value == value)) {
            this.props.change("amount", value);
        } else {
            this.state.options.push({
                key: value,
                value: value,
                text: `$${value}`
            });
            this.props.change("amount", value);
        }
    }

    calculateAmountWithTax = () => {
       
        const { amount, credits } = this.props;        
        
        if (amount && credits) {
            return (amount + credits.fee + (credits.percentage / 100 * amount)).toFixed(4);
        }
        return 0;
    }

    async handlePayment(payment) {
        const { nonce } = await this.instance.requestPaymentMethod()
        this.props.change("nonce", nonce);
        this.props.handleSubmit(payment);
    }


    render() {
        const { open, loading, onClose, amount, credits, userId } = this.props;

        return (
            <Modal
                as="form"
                open={open}
                className="task-details-modal"
                size="small"
                onClose={this.handleClose}
            >
                <Modal.Header>
                    Recharge your balance
                </Modal.Header>

                <Modal.Content>
                    <Form as="section">
                        <DropdownInput
                            fluid
                            search
                            allowAdditions
                            onAddItem={this.handleAddAmountValue}
                            name="amount"
                            label="Amount"
                            options={this.state.options}
                            required
                            validate={[required, minValue(5)]}
                        />
                        <Label content={`+ processing fee = ${this.calculateAmountWithTax()}$`} />
                        <TextInput
                            name="nonce"
                            type="hidden"
                        />
                        <div className="bt-drop-in-wrapper">
                            <div id="bt-dropin"></div>
                        </div>
                        {this.state.clientToken && this.props.hidePayment == undefined &&
                            <DropIn
                                container="#bt-dropin"
                                options={{
                                    authorization: this.state.clientToken,
                                    paypal: { flow: 'vault' }, vaultManager: true
                                }}
                                onInstance={instance => (this.instance = instance)}
                            />
                        }
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    {this.props.hidePayment == undefined &&
                        <ModusButton
                            className="button-flex-order1"
                            filled
                            content="Confirm payment"
                            type="button"
                            disabled={!amount || amount < 0}
                            loading={loading}
                            onClick={this.handlePayment.bind(this)}
                        />
                    }

                    {this.props.hidePayment != undefined &&
                        <ModusButton
                            className="button-flex-order1"
                            filled
                            content="Confirm payment"
                            type="button"
                            disabled={!amount || amount < 0}
                            loading={loading}
                            onClick={() => this.props.addCredit(amount, userId)}
                        />
                    }

                    <ModusButton
                        className="button-flex-order2"
                        grey type="button" content="Close" onClick={onClose} />
                </Modal.Actions>

            </Modal>
        );
    }
}
