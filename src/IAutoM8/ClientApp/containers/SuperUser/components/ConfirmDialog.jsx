import React, { Component } from "react";
import { push } from "react-router-redux";
import { Button, Header, Icon, Modal, Popup } from "semantic-ui-react";
import Axios from "axios";
import { getAuthHeaders } from "@infrastructure/auth";
import { connect } from "react-redux";
import { success, error } from "react-notification-system-redux";
import { loadPermissions, adminLoginAsUser } from "@store/auth";

@connect((state) => ({ ...state.auth }), {
    success,
    error,
    pushState: push,
    loadPermissions,
    adminLoginAsUser,
})
class ConfirmDialog extends Component {
    state = {
        modalOpen: false,
    };

    handleLoginAsUser = (email) => {
        const SignInDto = {
            Email: email,
        };
        this.props.adminLoginAsUser(SignInDto).then((response) => {
            this.props.loadPermissions();
            this.props.pushState("/projects/getTasksFromAllProjects/listView");
        });
    };

    // handleLoginAsUser = (email) => {
    //     const SignInDto = {
    //         Email: email,
    //     };
    //     //  const token = localStorage.getItem("JWT_TOKEN");
    //     localStorage.setItem("JWT_TOKEN", "");
    //     Axios.post("/api/SuperAdmin/user-signin", SignInDto, getAuthHeaders())
    //         .then((response) => {
    //             localStorage.setItem("JWT_TOKEN", response.data.access_token);
    //             this.props.setUser(email);
    //             this.props.resetLoginState();
    //             this.props.loadPermissions();
    //             this.props.pushState(
    //                 "/projects/getTasksFromAllProjects/listView"
    //             );
    //             // window.location.reload(false);
    //         })
    //         .catch((error) => {
    //             console.log("ppqErr", error);
    //             this.props.error({
    //                 title: error,
    //             });
    //         });
    //     // this.handleClose();
    // };

    handleConfirmationMailResend = (email) => {
        const ChangeEmail = {
            eMail: email,
            newEmail: "",
        };
        Axios.put("/api/SuperAdmin/resend-email", ChangeEmail, getAuthHeaders())
            .then(() => {
                this.props.success({
                    title: "Confirmation mail has been sent successfully",
                });
            })
            .catch((error) => {
                this.props.error({
                    title: error.data.message,
                });
            });
        this.handleClose();
    };

    handleOpen = () => this.setState({ modalOpen: true });

    handleClose = () => this.setState({ modalOpen: false });

    render() {
        const { title, message, userEmail, action, mailConfirmed } = this.props;
        return (
            <Modal
                trigger={
                    // <Button onClick={this.handleOpen}>Show Modal</Button>
                    action === "LogInAsUser" ? (
                        <Popup
                            content="Login as user"
                            trigger={
                                <Button
                                    className="iauto"
                                    circular
                                    icon="sign in"
                                    onClick={this.handleOpen}
                                ></Button>
                            }
                        />
                    ) : mailConfirmed === true ? (
                        <Popup
                            content="Email is already Confirmed"
                            trigger={
                                <Button
                                    // disabled="disabled"
                                    className="iauto"
                                    // icon="check"
                                    style={{
                                        width: "100px",
                                        paddingTop: "10px",
                                        paddingBottom: "4px",
                                        paddingLeft: "42px",
                                    }}
                                >
                                    <i
                                        aria-hidden="true"
                                        className="check icon"
                                        style={{ fontSize: "2em" }}
                                    ></i>
                                </Button>
                            }
                        />
                    ) : (
                        <Popup
                            content="Resend confirmation email"
                            trigger={
                                <Button
                                    className="iauto"
                                    onClick={this.handleOpen}
                                >
                                    Resend
                                </Button>
                            }
                        />
                    )
                }
                open={this.state.modalOpen}
                onClose={this.handleClose}
                basic
                size="small"
            >
                <Header icon="user" content={title} />
                <Modal.Content>{message}</Modal.Content>
                <Modal.Actions>
                    <Button color="red" onClick={this.handleClose} inverted>
                        <Icon name="remove" /> No
                    </Button>
                    <Button
                        color="green"
                        onClick={() =>
                            action === "LogInAsUser"
                                ? this.handleLoginAsUser(userEmail)
                                : action === "ResendEmail"
                                ? this.handleConfirmationMailResend(userEmail)
                                : this.handleClose
                        }
                        inverted
                    >
                        <Icon name="checkmark" /> Yes
                    </Button>
                </Modal.Actions>
            </Modal>
        );
    }
}

export default ConfirmDialog;
