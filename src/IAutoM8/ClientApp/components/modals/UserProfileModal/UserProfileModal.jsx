import React from "react";
import { Form, Modal } from "semantic-ui-react";
import Avatar from "react-avatar";

import { ModusButton } from "@components";

import "./UserProfileModal.less";

const UserProfileModal = ({
    open,
    onClose,
    userProfile: { fullName, displayName, dailyAvailability, profileImage },
}) => {
    const availability = dailyAvailability
        ? dailyAvailability === 1
            ? `${dailyAvailability} hour`
            : `${dailyAvailability} hours`
        : "";

    return (
        <Modal
            open={open}
            className="task-details-modal profile-modal"
            size="small"
            onClose={onClose}
        >
            <Modal.Header>
                <div style={{ display: "inline-block" }}>
                    Profile Information
                </div>
            </Modal.Header>

            <Modal.Content style={{ paddingBottom: "25px" }}>
                <Form as="section">
                    <div style={{ width: "45%", display: "inline-block" }}>
                        <div className="field">
                            <label>Full Name</label>
                            <div className="ui fluid input">
                                <input
                                    type="text"
                                    name="fullName"
                                    value={fullName || ""}
                                    readOnly
                                />
                            </div>
                        </div>
                        <div className="field">
                            <label>Display Name</label>
                            <div className="ui fluid input">
                                <input
                                    type="text"
                                    name="displayName"
                                    value={displayName || ""}
                                    readOnly
                                />
                            </div>
                        </div>
                        <div className="field">
                            <label>Daily Availability</label>
                            <div className="ui fluid input">
                                <input
                                    style={{border:"0px"}}      
                                    type="text"
                                    name="dailyAvailability"
                                    value={availability}
                                    readOnly
                                />
                            </div>
                        </div>
                    </div>
                    <div
                        style={{
                            width: "45%",
                            display: "inline-block",
                            float: "right",
                            paddingLeft: "20px",
                        }}
                    >
                        <div
                            className="image-box"
                            style={{
                                borderRadius: "25px",
                                border: "1px solid #7d94a0",
                                padding: "20px",
                                width: "250px",
                                height: "250px",
                            }}
                        >
                            {profileImage && (
                                <div
                                    className="circular--landscape"
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
                                    }}
                                >                                    
                                    <img
                                        src={profileImage}
                                        style={{ height: "100%" }}
                                    />
                                </div>
                            )}
                            {!profileImage && (
                                <div
                                    className="circular--landscape"
                                    style={{
                                        display: "inline-block",
                                        position: "relative",
                                    }}
                                >
                                    <Avatar
                                        name={fullName}
                                        size="210"
                                        round={true}
                                    />
                                </div>
                            )}
                        </div>
                    </div>
                </Form>
            </Modal.Content>

            <Modal.Actions>
                <ModusButton
                    className="button-flex-order2"
                    grey
                    type="button"
                    content="Close"
                    onClick={onClose}
                />
            </Modal.Actions>
        </Modal>
    );
};

export default UserProfileModal;
