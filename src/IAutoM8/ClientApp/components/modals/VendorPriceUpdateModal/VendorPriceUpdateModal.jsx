import React from "react";
import { connect } from "react-redux";
import { formValueSelector, reduxForm } from "redux-form";

import { Modal, Label } from "semantic-ui-react";

import { TextInput, ModusButton } from "@components";

import { loadVendorTax } from "@store/credits";

import editModalHoc from "../../common/editModalHoc";

const vendorPopupModalSelector = formValueSelector("vendorPriceUpdateModal");

@editModalHoc
@reduxForm({
    form: "vendorPriceUpdateModal",
    enableReinitialize: true,
})
@connect(
    (state) => ({
        vendorTax: state.credits.vendorTax,
        amount: Number(vendorPopupModalSelector(state, "price")),
    }),
    {
        loadVendorTax,
    }
)
export default class VendorPriceUpdateModal extends React.Component {
    componentDidMount() {
        this.props.loadVendorTax();
    }

    calculateAmountWithTax = () => {
        const { amount, vendorTax } = this.props;

        if (amount && vendorTax) {
            return (
                amount -
                vendorTax.fee -
                (vendorTax.percentage / 100) * amount
            ).toFixed(2);
        }
        return 0;
    };

    render() {
        const { open, onClose, vendorTax, handleSubmit } = this.props;

        return (
            <Modal as="form" open={open} onSubmit={handleSubmit} size="tiny">
                <Modal.Header as="h3">Edit Price for the Task</Modal.Header>
                <Modal.Content>
                    <TextInput
                        fluid
                        name="price"
                        label="Price"
                        placeholder="$"
                    />

                    <Label
                        content={`Tax is $${!vendorTax ? 0 : vendorTax.fee}
+ ${!vendorTax ? 0 : vendorTax.percentage}%.
You will get $${this.calculateAmountWithTax()}`}
                    />
                </Modal.Content>
                <Modal.Actions>
                    <ModusButton
                        filled
                        content="Update"
                        type="submit"
                        order="1"
                        onClick={this.submitPriceChange}
                    />
                    <ModusButton content="Cancel" order="2" onClick={onClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}
