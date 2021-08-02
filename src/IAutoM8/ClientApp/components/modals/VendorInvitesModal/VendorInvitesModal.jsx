import React from "react";
import { connect } from "react-redux";
import cn from "classnames";
import { Checkbox, Grid, Header, List, Rating, Modal } from "semantic-ui-react";

import { ModusButton, TimeAgo, TimeEstimate, UserAcronym } from "@components";

import { setFormulaOutsourceAssigne } from "@store/pendingTask";

class VendorInvitesModal extends React.Component {
    renderVendor(vendor, index) {
        const text =
            vendor.status === "None"
                ? "Sent on "
                : vendor.status === "Accepted"
                ? "Confirmed on "
                : vendor.status === "Declined"
                ? "Declined on "
                : "";
        return (
            <div className="vendor-detail" key={index}>
                <div className="vendor-detail__acronym">
                    <UserAcronym fullname={vendor.fullName || ""} />
                </div>
                <div className="vendor-detail__detail">
                    <div
                        className={cn([
                            "vendor-detail__name-project",
                            vendor.avgRating === null &&
                                vendor.avgResponding === null &&
                                "vendor-detail__name-project__empty",
                            vendor.avgRating === null &&
                                vendor.avgResponding !== null &&
                                "vendor-detail__name-project__partial",
                        ])}
                    >
                        {vendor.fullName}
                    </div>
                    {vendor.avgRating !== null && (
                        <Rating
                            disabled
                            maxRating={5}
                            rating={vendor.avgRating}
                            className="vendor-detail__rating"
                        />
                    )}
                    <div className="vendor-detail__avg-time">
                        {vendor.avgResponding !== null &&
                            "Avg. Time To Accept Your Invite "}
                        {vendor.avgResponding !== null && (
                            <TimeEstimate minutes={vendor.avgResponding} />
                        )}
                    </div>
                    <div className="vendor-detail__avg-time">
                        {vendor.avgWorking !== null &&
                            "Avg. Time To Complete Your Task "}
                        {vendor.avgWorking !== null && (
                            <TimeEstimate minutes={vendor.avgWorking} />
                        )}
                    </div>
                    <div className="vendor-detail__avg-time">
                        {vendor.avgMessaging !== null &&
                            "Avg. Time To Respond Your Message "}
                        {vendor.avgMessaging !== null && (
                            <TimeEstimate minutes={vendor.avgMessaging} />
                        )}
                    </div>
                </div>
                <div className="vendor-detail__request-info">
                    {text !== "" && vendor.date && (
                        <div className="vendor-detail__time">
                            {text}
                            <TimeAgo date={vendor.date} />
                        </div>
                    )}

                    {vendor.price !== null && vendor.price !== 0.0 && (
                        <div className="vendor-detail__price">
                            ${vendor.price}
                        </div>
                    )}
                    {(vendor.status === null ||
                        vendor.status === "DeclinedByOwner") && (
                        <ModusButton
                            as="a"
                            className="vendor-detail__btn"
                            content="assign"
                            onClick={() =>
                                this.props.onSetFormulaOutsourceAssigne(
                                    vendor.id,
                                    true
                                )
                            }
                        />
                    )}
                    {vendor.status === "None" && (
                        <ModusButton
                            as="div"
                            className="vendor-detail__btn decline"
                            content={<i />}
                            onClick={() =>
                                this.props.onSetFormulaOutsourceAssigne(
                                    vendor.id,
                                    false
                                )
                            }
                            popup="Waiting for confirmation. Do you want to cancel?"
                        />
                    )}
                    {vendor.status === "Accepted" && (
                        <ModusButton
                            as="div"
                            className="vendor-detail__btn accept"
                            content={<i />}
                        />
                    )}
                    {vendor.status === "Declined" && (
                        <ModusButton
                            as="div"
                            disabled
                            className="vendor-detail__btn"
                            content="denied"
                        />
                    )}
                </div>
            </div>
        );
    }

    render() {
        const {
            outsourcers,
            open,
            onClose,
            onSendInvites,
            loading,
            teamName,
            isGlobalTeam,
            skillName,
            isGlobalSkill,
            formulaName,
        } = this.props;

        return (
            <Modal 
                open={open} 
                onClose={onClose}
                className="task-details-modal tab-modal invite-vendors"
            >
                <Modal.Header>Invite Outsourcers</Modal.Header>
                <Modal.Content>
                    <Grid>
                        <Grid.Row columns={2}>
                            <Grid.Column width={4}>
                                <div style={{ fontSize: "xx-large" }}>
                                    <List>
                                        <List.Item>
                                            <Checkbox
                                                label={teamName}
                                                readOnly={true}
                                                defaultChecked={!isGlobalTeam}
                                                size="large"
                                            />
                                        </List.Item>
                                        <List.Item>
                                            <Checkbox
                                                label={skillName}
                                                readOnly={true}
                                                defaultChecked={!isGlobalSkill}
                                                size="large"
                                            />
                                        </List.Item>
                                        <List.Item>
                                            <Header
                                                as="h4"
                                                style={{
                                                    color: "#c3922e",
                                                    paddingTop: "35px"
                                                }}
                                            >
                                                <i
                                                    aria-hidden="true"
                                                    style={{
                                                        background:
                                                            "url(/images/icons-sprite.svg) no-repeat",
                                                        backgroundSize:
                                                            "112px 494px",
                                                        backgroundPosition:
                                                            "-87px -251px",
                                                        height: "18px",
                                                        width: "25px",
                                                        float: "left",
                                                    }}
                                                />
                                                <div style={{ padding: "3px 0 0 5px"}}>{formulaName}</div>
                                            </Header>
                                        </List.Item>
                                    </List>
                                </div>
                            </Grid.Column>
                            <Grid.Column width={12}>
                                <div
                                    style={{
                                        height: "450px",
                                        overflowY: "auto",
                                    }}
                                >
                                    {outsourcers &&
                                        outsourcers.length > 0 &&
                                        outsourcers.map((vendor, index) =>
                                            this.renderVendor(vendor, index)
                                        )}
                                </div>
                            </Grid.Column>
                        </Grid.Row>
                    </Grid>
                    <Modal.Description></Modal.Description>
                </Modal.Content>

                <Modal.Actions>
                    <ModusButton
                        className="button-flex-order2"
                        grey
                        type="button"
                        content="Cancel"
                        onClick={onClose}
                    />
                    <ModusButton
                        className="button-flex-order1"
                        filled
                        content="SEND"
                        type="button"
                        onClick={onSendInvites}
                        loading={loading}
                    />
                </Modal.Actions>
            </Modal>
        );
    }
}

export default connect(null, {
    setFormulaOutsourceAssigne,
})(VendorInvitesModal);
