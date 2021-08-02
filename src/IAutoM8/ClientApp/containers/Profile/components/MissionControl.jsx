/* eslint-disable max-len */
import React from "react";
import { connect } from "react-redux";
import { reset } from "redux-form";

import { Tab } from "semantic-ui-react";

import {
    loadPerformanceData,
    deleteTaskPrice,
    updateTaskPrice,
} from "@store/vendor";

import { VendorPriceUpdateModal } from "@components";

import PerformanceTable from "./performance/PerformanceTable";

class MissionControl extends React.Component {
    componentDidMount() {
        this.props.loadPerformanceData();
    }

    handleDelete = (formulaTaskId) => {
        this.props.deleteTaskPrice(formulaTaskId)
        .then(()=>{
            this.props.loadPerformanceData();
        })
    };

    handleEdit = (formulaTaskId, price) => {
        this.vendorPriceUpdateModal
            .show({ initialValues: { price, formulaTaskId } })
            .then((response) => {
                this.props
                    .updateTaskPrice(response.formulaTaskId, response.price)
                    .then(() => {
                        this.props.reset("vendorPriceUpdateModal");
                        this.props.loadPerformanceData();
                    });
            })
            .catch(() => {
                this.props.reset("vendorPriceUpdateModal");
            });
    };

    renderTab() {
        const panes = [
            {
                menuItem: { key: "performance", content: "Performance" },
                pane: {
                    key: "performance",
                    content: (
                        <div className="table-performance">
                            <PerformanceTable
                                performanceData={this.props.performanceData}
                                onDelete={this.handleDelete}
                                onEdit={this.handleEdit}
                            />
                        </div>
                    ),
                },
            },
            //{
            //    menuItem: { key: "coupon", content: "Coupon" },
            //    pane: {
            //        key: "coupon",
            //        content: <div>Coupon</div>,
            //    },
            //},
            //{
            //    menuItem: { key: "wallet", content: "Wallet" },
            //    pane: {
            //        key: "wallet",
            //        content: <div>Wallet</div>,
            //    },
            //},
            //{
            //    menuItem: { key: "affiliate", content: "Affiliate" },
            //    pane: {
            //        key: "affiliate",
            //        content: <div>Affiliate</div>,
            //    },
            //},
        ].filter((pane) => pane);

        return (
            <Tab
                panes={panes}
                defaultActiveIndex={0}
                renderActiveOnly={false}
                menu={{ secondary: true, pointing: true }}
            />
        );
    }

    render() {
        return (
            <div className="profile-container__mission-section">
                <h2>Mission control</h2>
                {this.renderTab()}
                <div>
                    <VendorPriceUpdateModal
                        ref={(c) => {
                            this.vendorPriceUpdateModal = c;
                        }}
                        loading={this.props.loading}
                    />
                </div>
            </div>
        );
    }
}

export default connect(
    (state) => ({
        loading: state.vendor.loading,
        performanceData: state.vendor.performanceData,
    }),
    {
        reset,
        loadPerformanceData,
        deleteTaskPrice,
        updateTaskPrice,
    }
)(MissionControl);
