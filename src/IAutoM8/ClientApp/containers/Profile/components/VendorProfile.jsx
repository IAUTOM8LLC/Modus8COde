/* eslint-disable max-len */

import React from "react";
import moment from "moment";
import FileInput from "react-fine-uploader/file-input";
import FineUploaderTraditional from "fine-uploader-wrappers";
import { Form, Input, Grid, Popup } from "semantic-ui-react";
import axios from "axios";
import Avatar from "react-avatar";

import { getAuthHeaders } from "@infrastructure/auth";

import { fullName, number, required, email } from "@utils/validators";

import { TextInput, SimpleSegment } from "@components";

export default class VendorProfile extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            profileImageUri: "",
            borderColor: ""
        };

        this.initUploader();
    }

    componentDidMount() {
        axios
            .get("/api/resource/get-profile-image", getAuthHeaders())
            .then((response) => {
                if (response.data) {
                    this.setState({ profileImageUri: response.data });
                }
            });

        this.setState((state, props) => ({ borderColor: props.borderColor }));
    }

    componentDidUpdate(prevProps) {
        if (this.props.borderColor !== prevProps.borderColor) {
            this.setState((state, props) => ({ borderColor: props.borderColor }));
        }
    }

    initUploader = () => {
        this.uploader = new FineUploaderTraditional({
            options: {
                request: {
                    endpoint: "/api/resource/upload-profile-image",
                    customHeaders: {
                        Authorization: getAuthHeaders().headers.Authorization,
                    },
                },
                validation: {
                    allowedExtensions: [
                        "jpg",
                        "jpeg",
                        "png",
                        "gif",
                        "svg",
                        "bmp",
                    ],
                },
                callbacks: {
                    onComplete: (id, name, response) => {
                        if (response.success) {
                            this.setState({
                                profileImageUri: response.fileInfo.url,
                                borderColor: "#000000"
                            });
                            this.props.onSetProfileImage(name);
                        }
                    },
                },
            },
            cors: {
                //all requests are expected to be cross-domain requests
                expected: true,
            },
        });
    };

    render() {
        const acceptTypes = () => {
            return `.${this.uploader.options.validation.allowedExtensions.join(
                ",."
            )}`;
        };

        const useIntl =
            typeof window.Intl !== "undefined" &&
            typeof window.Intl.DateTimeFormat !== "undefined";

        const timezoneLabel = useIntl
            ? "Your timezone is:"
            : "Your timezone offset is:";

        return (
            <Form
                as={SimpleSegment}
                className="profile-container__vendor-section"
            >
                <h2>Profile Information</h2>
                <Grid>
                    <Grid.Row>
                        <Grid.Column width={10}>
                            <TextInput
                                fluid
                                validate={[fullName, required]}
                                name="fullName"
                                label="Full Name"
                            />
                            <TextInput
                                fluid
                                validate={[required]}
                                name="displayName"
                                label="Display Name"
                            />
                            <TextInput
                                fluid
                                readOnly
                                name="email"
                                label="Email Address"
                                validate={[required]}
                            />
                            <Form.Field>
                                <label>{timezoneLabel}</label>
                                <Input
                                    fluid
                                    readOnly
                                    value={
                                        useIntl
                                            ? `${
                                            Intl.DateTimeFormat().resolvedOptions()
                                                .timeZone
                                            } ${moment().format("Z")}`
                                            : moment().format("Z")
                                    }
                                />
                            </Form.Field>
                            <TextInput
                                fluid
                                name="payoneerEmail"
                                label="Payoneer Account Email"
                                validate={[required, email]}
                            />
                            <a href="https://blog.payoneer.com/outsourcing/registration-identity-verification/" target="_blank" rel="noopener noreferrer">Payoneer Account Instructions</a>
                        </Grid.Column>
                        <Grid.Column className="image-section" width={6}>
                            <div className="image-section-child">
                                <div className="image-box" style={{
                                    borderColor: this.state.borderColor,
                                    borderRadius: "25px",
                                    border: "1px solid #7d94a0",
                                    padding: "20px",
                                    width: "250px",
                                    height: "300px",
                                }}

                                >
                                    {this.state.profileImageUri && !this.props.loading && (
                                        <div className="circular--landscape"
                                            style={{
                                                display: "inline-block",
                                                position: "relative",
                                                width: "200px",
                                                height: "200px",
                                                overflow: "hidden",
                                                border: "solid",
                                                borderColor: "#7d94a0",
                                                borderRadius: "50%",
                                                borderWidth: "1px",
                                                backgroundColor: "darkgrey",
                                            }}>
                                            <img src={this.state.profileImageUri} />
                                        </div>
                                    )}
                                    
                                    {!this.state.profileImageUri && !this.props.loading && (
                                        <div
                                            className="circular--landscape"
                                            style={{
                                                display: "inline-block",
                                                position: "relative",
                                                borderWidth: "1px",
                                            }}
                                        >
                                            <Avatar
                                                name={this.props.userFullName}
                                                size="210"
                                                round={true}
                                            />
                                        </div>
                                    )}
                                    <div style={{ paddingTop: "10px" }}>
                                        <FileInput
                                            accept={acceptTypes()}
                                            uploader={this.uploader}
                                            className="ui button iauto purple wide"
                                        >
                                            Add Profile Picture
                                        </FileInput>
                                    </div>
                                </div>
                                <div className="daily-avail">
                                    <Popup
                                        content="Enter your daily availability in hours from 0 to 14"
                                        trigger={
                                            <TextInput
                                                style={{ border: "0px" }}
                                                fluid
                                                name="dailyAvailability"
                                                label="Daily Availability"
                                                validate={[required, number]}
                                                type="Number"
                                                maxLengthValue={14}                                                
                                            />
                                        }
                                    />
                                </div>
                            </div>
                        </Grid.Column>
                    </Grid.Row>
                </Grid>
            </Form>
        );
    }
}
