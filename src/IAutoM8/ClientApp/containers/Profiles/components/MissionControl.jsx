/* eslint-disable max-len */
import React from "react";
import { connect } from "react-redux";
import { success, error, info } from 'react-notification-system-redux';
import { Segment, Button, Divider } from "semantic-ui-react";
import { reset } from "redux-form";

import { ModusButton } from "@components";

import { Tab } from "semantic-ui-react";

import {
    loadPerformanceData,
    loadCompanyPerformanceData,
    deleteTaskPrice,
    deleteCompanyUserTasks,
    deleteCompanyPerformanceTasks,
    updateTaskPrice,
    updateCompanyTaskPrice,
    companyWorkerEmail,
    getNewTaskData,
    loadCompanyUserData,
    loadCompanyUserDetails,
    selectUser
} from "@store/vendor";

import { VendorPriceUpdateModal, NewTaskFormModal, AddButtonModal } from "@components";

import CompanyPendingBidRequestsTable from
    "../../HomeList/components/VendorDashboard/PendingJobInvites/CompanyPendingBidRequestsTable";

import PerformanceTable from "./performance/PerformanceTable";

import UserTable from "./performance/UserTable";



//import CompanyUserAccordion from "./performance/CompanyUserAccordion";

class MissionControl extends React.Component {

    state = {
        // profileImageUri: "",
        // borderColor: "#000000",
        isActive: true,
        isArchive: false,
        isGig: true,
        isFormula: false,
        newTaskModalOpen: false,
        addButtonModalOpen: false

    };

    componentDidMount() {
        //this.props.loadPerformanceData();
        this.props.loadCompanyPerformanceData();
        this.props.loadCompanyUserData();
        this.props.loadCompanyUserDetails();
    }
    handleCompanyWorker = () => {

        console.log('table data')

    }

    handleDeletePerformanceTasks = (userId,formulaTaskId) => {
        //console.log('handleDeletePerformanceTasks',userId,formulaTaskId);
        this.props.deleteCompanyPerformanceTasks(userId,formulaTaskId)
            //this.props.loadPerformanceData();
            .then(() => {
                this.props.loadCompanyPerformanceData();
                this.props.loadCompanyUserDetails();
            });

    };
    handleUserTasksDelete = (userId,formulaTaskId) => {
            //console.log('handleUserTasksDelete',userId,formulaTaskId);
            this.props.deleteCompanyUserTasks(userId,formulaTaskId)
                   //this.props.loadPerformanceData();
                .then(() => {
                    this.props.loadCompanyPerformanceData();
                    this.props.loadCompanyUserDetails();
                });
        
        };
    toggleNewtaskModal = (opened) => {
        if (!opened) {
            this.props.reset('NewTaskFormModal');
        }

        this.setState({
            newTaskModalOpen: opened
        });
    }
    toggleAddButtonModal = (opened) => {
        this.setState({
            addButtonModalOpen: opened
        });
    }

    handleEdit = (formulaTaskId, price) => {
        this.toggleNewtaskModal(true)

        this.props.getNewTaskData(formulaTaskId)
    };

    handlePriceSubmit = (price) => {
        const formulaTaskId = this.props.newTaskModalData[0].formulaTaskId

        const data = [];
        Object.keys(price).forEach(item => {
            switch (item) {
                case 'companyPrice':
                    {
                        const obj = {
                            formulaTaskId: this.props.newTaskModalData[0].formulaTaskId,
                            vendorGuid: this.props.newTaskModalData[0].companyId,
                            price: price.companyPrice
                        }
                        data.push(obj)

                        return

                    }

                default:
                    //const index = this.props.newTaskModalData.findIndex(item => item.companyWorkerId == item )
                    {
                        const obj = {
                            formulaTaskId: this.props.newTaskModalData[0].formulaTaskId,
                            vendorGuid: item,
                            price: price[item]
                        }
                        data.push(obj)

                        return

                    }
            }
            // return  console.log('item', item)
        })

        this.props
            .updateCompanyTaskPrice(formulaTaskId, data)
            .then(() => {
                this.toggleNewtaskModal(false)
                //this.props.reset("NewTaskFormModal");
                //this.props.loadPerformanceData();
                this.props.loadCompanyPerformanceData();
                this.props.loadCompanyUserDetails();
            });
        // .catch(() => {
        //     this.props.reset("NewTaskFormModal");
        // });
    };
    handleEmailSubmit = (email) => {
        this.props.companyWorkerEmail(email)
            .then(() => this.props.success({ title: 'Email has been sent' }))
        this.toggleAddButtonModal(false);
        this.props.reset("addButtonModal");

    }
    handleNewTaskModalClose = () => {
        this.toggleNewtaskModal(false);
    }
    handleAddButtonModalClose = () => {
        this.toggleAddButtonModal(false);
        this.props.reset("addButtonModal");
    }
    onToggleGigOrFormula = () => {
        this.setState((prevState) => ({
            isActive: !prevState.isActive,
            isFormula: !prevState.isFormula,
        }));
    }
    renderTab() {
        const panes = [
            {
                menuItem: { key: "performance", content: "Performance" },
                pane: {
                    key: "performance",
                    content: (
                        <div className="table-performance">
                            <div style={{ marginBottom: "25px" }}>
                                {/* <Divider /> */}
                                <div className="iauto-vendor-task-header">
                                    {/* <div className="ui header">INVITES</div> */}
                                    <div style={{ paddingTop: "9px" }}>
                                        {/* <Header size="medium">INVITES</Header> */}
                                    </div>
                                    <div>
                                        <Button.Group>
                                            <ModusButton
                                                filled={this.state.isActive}
                                                grey={!this.state.isActive}
                                                onClick={this.onToggleGigOrFormula}
                                                content="ACTIVE"
                                            />
                                            <ModusButton
                                                filled={this.state.isFormula}
                                                grey={!this.state.isFormula}
                                                onClick={this.onToggleGigOrFormula}
                                                content="INVITES"
                                            />
                                        </Button.Group>
                                        {
                                            this.props.companyFormulaBids.length !== 0 &&
                                            <div className="iauto-notify--bid-message">{this.props.companyFormulaBids.length}</div>
                                        }
                                    </div>
                                </div>
                                {/* <Divider /> */}
                                {/* {this.state.isGig && (
                        <div style={{ overflow: "auto" }}>
                            <PendingJobInvitesTable
                                vendorJobInvites={vendorJobInvites}
                                onPendingInviteRefresh={this.handlePendingInviteRefresh}
                                onAccepInvite={this.handleVendorInviteResponse}
                                onDeclineInvite={this.handleVendorInviteResponse}
                            />
                        </div>
                    )}*/}
                                {this.state.isFormula && (
                                    <div style={{ overflow: "auto" }}>
                                        {/* <PendingBidRequestsTable
                                
                            /> */}
                                        <CompanyPendingBidRequestsTable
                                            companyFormulaBids={this.props.companyFormulaBids}
                                        //onDeclineFormulaBid ={this.handleFormulaBidDecline} 
                                        />
                                    </div>
                                )}
                                {this.state.isActive && (
                                    <PerformanceTable
                                        companyPerformanceData={this.props.companyPerformanceData}
                                        onDelete={this.handleDeletePerformanceTasks}
                                        onEdit={this.handleEdit}
                                    />
                                )}
                            </div>

                            {/* <PerformanceTable
                                performanceData={this.props.performanceData}
                                onDelete={this.handleDelete}
                                onEdit={this.handleEdit}
                            /> */}
                        </div>
                    ),
                },
            },
            {
                menuItem: { key: "Users", content: "Users" },
                pane: {
                    key: "Users",
                    content: (
                        <div className="table-performance">
                            <div className="iauto-vendor-task-header">
                                <div style={{ paddingTop: "9px" }}>
                                    {/* <Header size="medium">INVITES</Header> */}
                                </div>

                                <ModusButton
                                    wide
                                    filled
                                    floated="right"
                                    content="+Add"
                                    onClick={() => this.toggleAddButtonModal(true)}
                                // loading={submitting}
                                />
                            </div>
                            <UserTable
                                companyUserData={this.props.companyUserDetails}
                                performanceData={this.props.performanceData}
                                // onDelete={this.handleDelete}
                                onDelete={this.handleUserTasksDelete}
                                // onEdit={this.handleEdit}
                                handleCompanyWorker={this.handleCompanyWorker}
                                selectUser={(val) => this.props.selectUser(val)}
                                selectedUsers={this.props.vendor.selectedUsers}
                                toggleAddButtonModal={this.toggleAddButtonModal}
                                onEdit={this.handleEdit}
                            />
                            {/* <CompanyUserAccordion  
                                companyUserData={this.props.companyUserData}
                            /> */}
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
                    {this.props.newTaskModalData !== undefined && this.props.newTaskModalData &&
                        (<NewTaskFormModal
                            open={this.state.newTaskModalOpen}
                            onClose={this.handleNewTaskModalClose}
                            newTaskModalData={this.props.newTaskModalData}
                            onSubmit={this.handlePriceSubmit}
                            toggleAddButtonModal={this.toggleAddButtonModal}
                        // ref={(c) => {
                        //     this.newTaskFormModal = c;
                        // }}
                        // loading={this.props.loading}
                        />)}
                    <AddButtonModal
                        open={this.state.addButtonModalOpen}
                        onClose={this.handleAddButtonModalClose}
                        onSubmit={this.handleEmailSubmit}
                    // ref={(c) => {
                    //     this.newTaskFormModal = c;
                    // }}
                    // loading={this.props.loading}
                    />
                </div>
            </div>
        );
    }
}

export default connect(
    (state) => ({
        loading: state.vendor.loading,
        companyPerformanceData: state.vendor.companyPerformanceData,
        newTaskModalData: state.vendor.newTaskData,
        companyUserData: state.vendor.companyUserData,
        companyUserDetails: state.vendor.companyUserDetails,
        vendor: state.vendor

    }),
    {
        reset,
        success, error, info,
        //loadPerformanceData,
        loadCompanyPerformanceData,
        loadCompanyUserData,
        deleteTaskPrice,
        deleteCompanyUserTasks,
        deleteCompanyPerformanceTasks,
        updateTaskPrice,
        updateCompanyTaskPrice,
        companyWorkerEmail,
        getNewTaskData,
        loadCompanyUserDetails,
        selectUser,
    }
)(MissionControl);
