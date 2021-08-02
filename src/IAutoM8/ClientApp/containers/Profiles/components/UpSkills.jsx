/* eslint-disable max-len */
import React from "react";
import { connect } from "react-redux";
import { reset } from "redux-form";
import axios from "axios";
import { error } from "react-notification-system-redux";

import { getAuthHeaders } from "@infrastructure/auth";

import { Header, Label, Segment, Grid } from "semantic-ui-react";

import { loadVendorUpSkills, formulaTaskCertificationResponse } from "@store/vendor";

import { ModusButton, VendorPopupModal } from "@components";

class UpSkills extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            vendorPopupOpen: false,
            initialValues: {
                isProjectTaskNotification: false,
                ...{}
            },
            answer: 2
        };

        this.openVendorPopup = this.toggleVendorPopup.bind(this, true);
        this.closeVendorPopup = this.toggleVendorPopup.bind(this, false);
    }

    componentDidMount() {
        this.props.loadVendorUpSkills();
    }

    toggleVendorPopup = (opened) => {
        if (opened) {
            this.setState({
                vendorPopupOpen: true,
            });
        } else {
            this.setState({
                vendorPopupOpen: false,
            });
        }
    };

    handleClick = (formulaTaskId) => {
        axios.get(`/api/vendor/certify/formulatask/${formulaTaskId}`, getAuthHeaders())
            .then((response) => {
                const initialValues = {
                    isProjectTaskNotification: false,
                    ...response.data
                };

                this.setState({ initialValues: initialValues, vendorPopupOpen: true });
            });
    };

    onAcceptVendorNotification = () => {
        this.setState({ answer: 3 });
    };

    onDeclineVendorNotification = () => {
        this.setState({ answer: 0 });
    };

    handleVendorNotificationSubmit = (notification) => {
        if (this.state.answer === 3 && !notification.price || notification.price < 0) {
            this.props.error({ title: "Price is required field and can't be negative" });
            return;
        }

        notification.answer = this.state.answer;
        this.props.formulaTaskCertificationResponse(notification)
            .then(() => {
                this.closeVendorPopup();
                this.props.reset('vendorNotificationFormModal');
                this.props.loadVendorUpSkills();
            })
            .catch((error) => this.props.error({ title: error.data.message }));
    };

    render() {
        const { vendorUpSkills } = this.props;

        return (
            <React.Fragment>
                <div className="profile-container__upskill-section">
                    {vendorUpSkills && vendorUpSkills.length > 0 && (
                        <Segment>
                            <Header as="h2" textAlign="center">
                                WANT TO MAKE MORE MORE MONEY?
                                <Header.Subheader>
                                    Get certified in more skills and formulas
                                </Header.Subheader>
                            </Header>
                            <div className="content">
                                <div className="sub-content">
                                    <Header as="h4">Related Skills</Header>
                                    <Grid columns={4} divided>
                                        {vendorUpSkills &&
                                            vendorUpSkills.map(
                                                (item, index) => (
                                                    <Grid.Row key={index}>
                                                        <Grid.Column width={4}>
                                                            <Label size="large">
                                                                {item.team ||
                                                                    "NA"}
                                                            </Label>
                                                        </Grid.Column>
                                                        <Grid.Column width={4}>
                                                            <Label size="large">
                                                                {item.skill ||
                                                                    "NA"}
                                                            </Label>
                                                        </Grid.Column>
                                                        <Grid.Column width={6}>
                                                            <Label size="large">
                                                                {item.formula ||
                                                                    "NA"}
                                                            </Label>
                                                        </Grid.Column>
                                                        <Grid.Column width={2}>
                                                            <ModusButton
                                                                filled
                                                                type="button"
                                                                content="Get Certified"
                                                                className="cert-button"
                                                                onClick={() =>
                                                                    this.handleClick(
                                                                        item.formulaTaskId
                                                                    )
                                                                }
                                                            />
                                                        </Grid.Column>
                                                    </Grid.Row>
                                                )
                                            )}
                                    </Grid>
                                </div>
                                {/* <div className="sub-content">
                            <Header as="h4">New to the Vault</Header>
                            <List style={{ paddingLeft: "25px", paddingBottom: "15px" }}>
                                {newToVault && newToVault.map((item, index) => (
                                    <List.Item key={index}>
                                        <Label size="large">{item.teamName}</Label>
                                        <Label size="large">{item.skillName}</Label>
                                        <Label size="large">{item.formulaName}</Label>
                                        <ModusButton
                                            filled
                                            type="button"
                                            content="See Formula"
                                            className="formula-button"
                                        />
                                    </List.Item>
                                ))}
                            </List>
                        </div> */}
                            </div>
                        </Segment>
                    )}
                </div>

                <VendorPopupModal
                    onAccept={this.onAcceptVendorNotification}
                    onDecline={this.onDeclineVendorNotification}
                    open={this.state.vendorPopupOpen}
                    onClose={this.closeVendorPopup}
                    onSubmit={this.handleVendorNotificationSubmit}
                    loading={this.props.loading}
                    initialValues={this.state.initialValues}
                />
            </React.Fragment>
        );
    }
}

export default connect(
    (state) => ({
        loading: state.vendor.loading,
        vendorUpSkills: state.vendor.vendorUpSkills,
    }),
    {
        error, reset,
        loadVendorUpSkills,
        formulaTaskCertificationResponse,
    }
)(UpSkills);
