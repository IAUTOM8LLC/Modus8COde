import React, { Component } from "react";
import { reduxForm, FormSection, Form } from "redux-form";
import { connect } from "react-redux";
import { Segment, Button, Divider } from "semantic-ui-react";
import { success, error } from "react-notification-system-redux";
import moment from "moment";



import FineUploaderTraditional from "fine-uploader-wrappers";

import { updateProfile as changeProfile } from "@store/auth";

import {
    loadProfile,
    updateProfile,
    setProfileImage
} from "@store/profile";

import permissionAware from "@infrastructure/HOC/permissionAware";

import { ModusButton } from "@components";

import axios from "axios";

import { getAuthHeaders } from "@infrastructure/auth";

import {
    formulaTaskNotificationResponse,
    projectTaskNotificationResponse,
    loadVednorSnapshotDetail,
    loadVednorJobInvites,
    loadCompanyFormulaBids,
    syncVendorData,
    removeJobInvite,
    removeFormulaBid,
} from "@store/vendor";

import UserProfile from "./components/UserProfile";

import BusinessProfile from "./components/BusinessProfile";
import MissionControl from "./components/MissionControl";
import UpSkills from "./components/UpSkills";
import VendorProfile from "./components/VendorProfile";
import "./Profile.less";


@connect(
    (state) => ({
        ...state.profile,
        initialValues: state.profile.profile,
        viewMissionControl: state.auth.permissions.viewMissionControl,
        viewUpSkills: state.auth.permissions.viewUpSkills,
        companyFormulaBids: state.vendor.companyFormulaBids,
    }),
    {
        success,
        notifyError: error,
        loadProfile,
        updateProfile,
        changeProfile,
        setProfileImage,
        loadCompanyFormulaBids
    }
)
@reduxForm({
    form: "profileForm",
    enableReinitialize: true,
})
@permissionAware((state) => state.auth.permissions.viewOwnerBusinessDetails)
export default class Profiles extends Component {

    constructor(props) {
        super(props);

        this.state = {
            profileImageUri: "",
            borderColor: "#000000",
        };

        this.initUploader();
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
                            this.onSetProfileImage(name);
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

    //state = { borderColor: "#000000", profileImageUri: "" };

    // componentDidMount() {
    //     this.props.syncVendorData().then(() => {
    //         this.props.loadVendorTasksInStatus();
    //         this.props.loadVednorSnapshotDetail();
    //         this.props.loadVednorJobInvites();

    //     });
    // }

    componentDidMount() {
        this.props.loadProfile();
        this.props.loadCompanyFormulaBids();

        axios
            .get("/api/resource/get-profile-image", getAuthHeaders())
            .then((response) => {
                if (response.data) {
                    this.setState({ profileImageUri: response.data });
                }
            });

        this.setState((state, props) => ({ borderColor: props.borderColor }));
    }

    handleSubmit = (profile) => {

        const { profileImage } = profile.userProfile;
        const date = moment();
        if (profile.businessProfile) {
            let time = null;
            if (
                moment(
                    profile.businessProfile.toDoSummaryTime,
                    [
                        "YYYY-MM-DDTHH:mm:ssZ",
                        "YYYY-MM-DDTHH:mm:ss",
                        "YYYY-MM-DDTHH:mm:ss.sssssss",
                        "YYYY-MM-DDTHH:mm:ss.sssssssZ",
                    ],
                    true
                ).isValid()
            ) {
                time = moment(profile.businessProfile.toDoSummaryTime);
            } else {
                time = moment(
                    profile.businessProfile.toDoSummaryTime,
                    "hh:mm A"
                );
            }
            date.set({
                hour: time.get("hour"),
                minute: time.get("minute"),
            });
        }

        if (!profileImage || profileImage === null || profileImage === "") {
            this.props.notifyError({ title: "Profile image is required" });
            this.setState({ borderColor: "#e6424b" });
            return;
        }

        return this.props
            .updateProfile({
                ...profile,
                businessProfile: profile.businessProfile
                    ? {
                        ...profile.businessProfile,
                        toDoSummaryTime: date.format("MMM D, YYYY h:mm A"),
                    }
                    : undefined,
            })
            .then(({ value }) => {
                this.props.changeProfile(value);
                this.props.success({
                    title: "Profile have been updated successfully",
                });
            })
            .catch(() =>
                this.props.notifyError({
                    title: "User already exists",
                })
            );
    };

    onSetProfileImage = (value) => {
        this.props.setProfileImage(value);
    };

    renderPermitted() {
        return (
            this.props.profile.businessProfile && (
                <FormSection name="businessProfile">
                    <BusinessProfile />
                </FormSection>
            )
        );
    }

    render() {

        const acceptTypes = () => {
            return `.${this.uploader.options.validation.allowedExtensions.join(
                ",."
            )}`;
        };

        const {
            loading,
            submitting,
            permissioned,
            viewMissionControl,
            viewUpSkills,
            companyFormulaBids,
            profile: {
                userProfile: { fullName }
            }
        } = this.props;
        // if (companyFormulaBids !== undefined) {
        //     const unreadBidMessageCount = companyFormulaBids.length;
        // }
        return (
            <React.Fragment>
                <Form
                    className="profile-container"
                    onSubmit={this.props.handleSubmit(this.handleSubmit)}
                >
                    <FormSection name="userProfile">
                        {
                            this.props.initialValues.isVendor
                                ? <VendorProfile
                                    loading={loading}
                                    userFullName={fullName || 'Modus'}
                                    borderColor={this.state.borderColor}
                                    onSetProfileImage={this.onSetProfileImage}
                                />
                                : <UserProfile
                                    loading={loading}
                                    userFullName={fullName || 'Modus'}
                                    profileImageUri={this.state.profileImageUri}
                                    lineColor={this.state.borderColor}
                                    acceptTypes={acceptTypes}
                                    uploader={this.uploader}
                                />
                        }
                    </FormSection>

                    {permissioned}

                    {<Segment basic clearing>
                        <ModusButton
                            wide
                            filled
                            floated="right"
                            content="Save"
                            type="submit"
                            loading={submitting}
                        />
                    </Segment>}
                </Form>

                {viewMissionControl && companyFormulaBids !== undefined && (
                    <MissionControl
                        //unreadBidMessageCount ={unreadBidMessageCount}
                        companyFormulaBids={companyFormulaBids}
                    />
                )}
                {viewUpSkills && <UpSkills />}
            </React.Fragment>
        );
    }
}
