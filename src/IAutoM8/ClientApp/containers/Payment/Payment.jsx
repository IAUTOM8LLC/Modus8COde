import React, { Component } from "react";

import axios from 'axios'

import { success, error } from "react-notification-system-redux";

import { getAuthHeaders } from '@infrastructure/auth'
import { Tab } from "semantic-ui-react";

import { connect } from "react-redux";

import PaymentTable from "./components/PaymentTable";
import './Payment.less';

@connect(
    null,
    {
        success,
        error,
    }
)

export default class Payment extends Component {
    constructor(props) {
        super(props);

        this.state = {
            paymentRequest: [],
            description: "",
            activeTab: 0            
        }
    }



    acceptRequest = (id, desc) => {
        axios.get(`/api/Payment/AcceptDenyRequest`,
            {
                params: {
                    requestId: id,
                    response: 1,
                    desc: desc
                }
            }
            , getAuthHeaders())
            .then((response) => {
                this.loadPaymentRequests(1);
                this.setState({ activeTab: 1 });
                this.props.success({
                    title: "Save successfully.",
                });
            })
            .catch((response) => {
                this.props.error({
                    title: "Please try again later.",
                });
            });
    };

    denyRequest = (id, desc) => {
        axios.get(`/api/Payment/AcceptDenyRequest`, {
            params: {
                requestId: id,
                response: 0,
                desc: desc
            }
        }, getAuthHeaders())
            .then((response) => {
                this.loadPaymentRequests(1);
                this.setState({ activeTab: 1 });
                this.props.success({
                    title: "Save successfully.",
                });
            })
            .catch((response) => {
                this.props.error({
                    title: "Please try again later.",
                });
            });
    };



    componentDidMount() {
        this.loadPaymentRequests(0);
    }

    loadPaymentRequests = (type) => {
        axios.get(`/api/Payment/GetFundRequests/${type}`, getAuthHeaders())
            .then((response) => {
                this.setState({ paymentRequest: response.data });
            })
            .catch((response) => {
            });
    }

    downloadCSV = (paymentRequests) => {
        const paymentData = paymentRequests;

        let selectedPayment = paymentData.filter(x => x.isChecked == true);

        if (selectedPayment.length === 0) {
            this.props.error({
                title: "Please select any record.",
            });
            return false;
        }

        axios.post(`/api/Payment/DownloadCSV`, {
            Payments: selectedPayment
        }, getAuthHeaders())
            .then((response) => {
                const fileBytes = this.base64ToArrayBuffer(response.data[0]);
                this.saveByteArrayAsCsv("PaymentRequest", fileBytes);
                this.loadPaymentRequests(0);
                this.props.success({
                    title: "Excel has been generated containing " + response.data[1] + " payment requests",
                });

            })
            .catch((response) => {
            });
    }

    batchProcess = (isAccepted, paymentRequests) => {
        const paymentData = paymentRequests;//this.state.paymentRequest;

        let selectedPayment = paymentData.filter(x => x.isChecked == true);

        if (selectedPayment.length === 0) {
            this.props.error({
                title: "Please select any record.",
            });
            return false;
        }

        axios.post(`/api/Payment/BatchProcess/${isAccepted}`, {
            Payments: selectedPayment
        }, getAuthHeaders())
            .then((response) => {
                this.loadPaymentRequests(1);
                this.props.success({
                    title: "Bacth process sucessfully completed for " + response.data[1] + " records.",
                });

            })
            .catch((response) => {
            });
    }

    base64ToArrayBuffer = (base64) => {
        const binaryString = window.atob(base64);
        const binaryLen = binaryString.length;
        const bytes = new Uint8Array(binaryLen);
        for (let i = 0; i < binaryLen; i++) {
            const ascii = binaryString.charCodeAt(i);
            bytes[i] = ascii;
        }
        return bytes;
    }


    saveByteArrayAsCsv = (reportName, byte) => {
        const blob = new Blob([byte], { type: "text/csv" });
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        let fileName = reportName;
        link.download = fileName + '.csv';
        link.click();
    };

    handleMasterChange = (value) => {

        if (value.checked) {
            // All Checked
            this.setState((prevState) => {
                let { paymentRequest } = prevState;
                paymentRequest = paymentRequest.map(request => ({
                    ...request, isChecked: true
                }));

                return { paymentRequest };
            });            
        } else if (!value.indeterminate) {
            this.setState((prevState) => {
                let { paymentRequest } = prevState;
                paymentRequest = paymentRequest.map(request => ({
                    ...request, isChecked: false
                }));
                return { paymentRequest };
            });
        }
    }

    handleChange = (id, paymentRequests) => {
        
        const paymentRequest = paymentRequests;//this.state.paymentRequest;
        const index = paymentRequest.findIndex(request => request.id === id);
        paymentRequest[index].isChecked = !paymentRequest[index].isChecked;
        this.setState({ paymentRequest: paymentRequest });
    }

    handleFormulaSelector = (e, data) => {

        switch (data.activeIndex) {
            case 0:
                this.loadPaymentRequests(0);
                this.setState({ activeTab: 0 });
                break;
            case 1:
                this.loadPaymentRequests(1);
                this.setState({ activeTab: 1 });
                break;
            case 2:
                this.loadPaymentRequests(2);
                this.setState({ activeTab: 2 });
                break;
        }
    };


    renderTab = () => {


        const formulaTable = (
            <PaymentTable
                paymentRequestList={this.state.paymentRequest}
                onUnCheckAll={this.handleUnCheckAll}
                onMasterSelect={this.handleMasterChange}
                onChange={this.handleChange}
                downloadCSV={this.downloadCSV}
                acceptRequest={this.acceptRequest}
                denyRequest={this.denyRequest}
                batchProcess={this.batchProcess}
                activeTab={this.state.activeTab}
            />
        );

        let panes = [];
        panes = [
            {
                menuItem: { key: "NewRequest", content: "New Request" },
                pane: {
                    key: "NewRequest",
                    content: <React.Fragment>{formulaTable}</React.Fragment>,
                },
            },
            {
                menuItem: { key: "InProcess", content: "In Process" },
                pane: {
                    key: "InProcess",
                    content: <React.Fragment>{formulaTable}</React.Fragment>,
                },
            },
            {
                menuItem: { key: "History", content: "History" },
                pane: {
                    key: "History",
                    content: <React.Fragment>{formulaTable}</React.Fragment>,
                },
            },
        ].filter((pane) => pane);

        return (
            <Tab
                panes={panes}
                renderActiveOnly={false}
                menu={{ secondary: true, pointing: true }}
                onTabChange={this.handleFormulaSelector}
                activeIndex={this.state.activeTab}
            />
        );
    };


    render() {
        return (
            <div className="iauto-payment">
                <div className="payments-pane tab-modal">
                    {this.renderTab()}
                </div>
            </div>
        );
    }
}
