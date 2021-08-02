import React from "react";
import { Field } from "redux-form";
import {
    Button,
    Header,
    Image,
    Rating,
    Radio
} from "semantic-ui-react";
import { orderBy } from 'lodash';

import { formatMoney } from "@utils/formatMoney";
import { required } from "@utils/validators";

import {
    DropdownInput,
    ModusButton,
    UserAcronym
} from "@components";

export default class AssignSkillUsers extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            value: '',
            outsourceUsers: [],
            inHouseUserOptions: [],
            outsourcedUsersOptions: [],
            reviewingUserOptions: [],
            isInHouseUsersVisible: true,
        };
    }

    componentDidMount() {
        const { 
            field,
            inHouseOptions,
            outsourcedUsers,
            reviewingUsers,
        } = this.props;

        // mapped, as getting values from the api
        let users = [];
        if (outsourcedUsers && outsourcedUsers.length > 0) {
            outsourcedUsers.map((user) => {
                users.push({
                    id: user.id,
                    fullName: user.fullName,
                    profileImage: user.profileImage,
                    price: user.price ? user.price : 0,
                    rating: user.avgRating ? user.avgRating : 0,
                    availability: user.avgWorking ? user.avgWorking : 0,
                    taskCompleted: user.taskCompleted ? user.taskCompleted : 0,
                });
            });
        }

        const isInHouseUsersVisible = inHouseOptions.length !== 0 ? true : false;

        // default sorting is based on rating
        users = orderBy(users, ['rating'], ['desc']);
        const inHouseUserOptions = this.getUserOptions(inHouseOptions);
        const reviewingUserOptions = this.getUserOptions(reviewingUsers);
        const outsourcedUsersOptions = this.getOutSourceOptions(users);

        this.setState({
            value: `${field}.topRated`,
            outsourceUsers: users,
            inHouseUserOptions:  inHouseUserOptions,
            reviewingUserOptions: reviewingUserOptions,
            outsourcedUsersOptions: outsourcedUsersOptions,
            isInHouseUsersVisible,
        });
    }

    getUserOptions = (users) => {
        return users.map((user) => ({
            key: user.id,
            value: user.id,
            content: (
                <Header as="h5" style={{ display: "flex" }}>
                    {user.profileImage && (
                        <Image
                            style={{ width: "35px", height: "35px" }}
                            avatar
                            src={user.profileImage}
                        />
                    )}
                    {!user.profileImage && (
                        <UserAcronym fullname={user.fullName} />
                    )}
                    <label style={{ paddingTop: "8px", paddingLeft: "10px" }}>{user.fullName}</label>
                </Header>
            ),
            text: `${user.fullName}`
        }))
    };

    getOutSourceOptions = (users) => {
        return users.map((user) => ({
            key: user.id,
            value: user.id,
            content: (
                <Header as="h5">
                    <div
                        style={{
                            display: "flex",
                            justifyContent: "space-between",
                        }}
                    >
                        {user.profileImage && (
                            <div style={{ width: "50%" }}>
                                <Image
                                    style={{ width: "35px", height: "35px" }}
                                    avatar
                                    src={user.profileImage}
                                />
                                {user.fullName}
                                {user.taskCompleted <= 5 && <sup>New User</sup>}
                            </div>
                        )}
                        {!user.profileImage && (
                            <div style={{ width: "50%", display: "flex" }}>
                                <UserAcronym fullname={user.fullName} />
                                <label style={{ paddingTop: "8px", paddingLeft: "10px" }}>
                                    {user.fullName}
                                    {user.taskCompleted <= 5 && <sup>New User</sup>}
                                </label>
                            </div>
                        )}
                        <div style={{ width: "30%" }}>
                            <Rating
                                icon="star"
                                disabled
                                maxRating={5}
                                rating={user.rating}
                            />
                        </div>
                        <div style={{ width: "20%", textAlign: "right" }}>
                            <label>
                                $ {formatMoney(user.price, 2, ".", " ")}
                            </label>
                        </div>
                    </div>
                </Header>
            ),
            text: `${user.fullName} $${formatMoney(user.price, 2, ".", " ")}`,
        }));
    };

    handleFilterChange = (e, { value }, index) => {
        const { outsourceUsers, } = this.state;

        const keys = value.split(".");

        let sortedOutsourcedUsers = [...outsourceUsers];

        if (keys[1] === 'topRated') {
            sortedOutsourcedUsers = orderBy(outsourceUsers, ['rating'], ['desc']);
        }

        if (keys[1] === 'firstAvailable') {
            sortedOutsourcedUsers = orderBy(outsourceUsers, ['availability'], ['asc']);
        }

        if (keys[1] === 'cheapest') {
            sortedOutsourcedUsers = orderBy(outsourceUsers, ['price'], ['asc']);
        }

        this.setState({ 
            value: value,
            outsourcedUsersOptions: this.getOutSourceOptions(
                sortedOutsourcedUsers
            ),
        });

        this.props.addSelectedVendors(index, keys[1]);
    };

    renderField = ({ assignMapName, options }) => {
        return (
            <DropdownInput
                fluid
                multiple
                search
                selection
                options={options}
                name={assignMapName}
                //label="In-House Users"
                placeholder="Select In-House Users"
                //validate={[required, minUsersNumber(1)]}
                validate={[required]}
            />
        );
    };

    renderFieldWithoutValidation = ({ assignMapName, options }) => {
        return (
            <DropdownInput
                fluid
                multiple
                search
                selection
                options={options}
                name={assignMapName}
                placeholder="Select In-House Users"
            />
        );
    };

    renderReviewerField = ({ assignMapName, options }) => {
        return (
            <DropdownInput
                fluid
                multiple
                search
                selection
                options={options}
                name={assignMapName}
                //label="In-House Users"
                placeholder="Select Reviewer"
                validate={[required]}
            />
        );
    };

    renderReviewerFieldWithoutValidation = ({ assignMapName, options }) => {
        return (
            <DropdownInput
                fluid
                multiple
                search
                selection
                options={options}
                name={assignMapName}
                placeholder="Select Reviewer"
            />
        );
    };

    renderOutsourcerField = ({ assignMapName, options }) => {
        return (
            <DropdownInput
                fluid
                multiple
                search
                selection
                options={options}
                name={assignMapName}
                //label="Outsourced Users"
                placeholder="Select Outsource Users"
                validate={[required]}
            />
        );
    };

    renderOutsourcerFieldWithoutValidation = ({ assignMapName, options }) => {
        return (
            <DropdownInput
                fluid
                multiple
                search
                selection
                options={options}
                name={assignMapName}
                placeholder="Select Outsource Users"
            />
        );
    };

    onToggle = (event) => {
        event.preventDefault();
        this.setState((state) => ({ 
            isInHouseUsersVisible: !state.isInHouseUsersVisible 
        }));
    };

    render() {
        const { 
            field,
            taskMapItem,
            inHouseOptions,
            outsourcedUsers,
            index,
        } = this.props;

        // const disabledStyle = taskMapItem.isDisabled
        //     ? { display: "none" }
        //     : { marginTop: "15px" };

        return (
            <div style={{ marginTop: "15px" }}>
                <div style={{ textAlign: "center" }}>
                    <Button.Group>
                        <ModusButton
                            filled={this.state.isInHouseUsersVisible}
                            grey={!this.state.isInHouseUsersVisible}
                            onClick={(e) => this.onToggle(e)}
                            content="In-House"
                            disabled={inHouseOptions.length === 0}
                        />
                        <ModusButton
                            filled={!this.state.isInHouseUsersVisible}
                            grey={this.state.isInHouseUsersVisible}
                            onClick={(e) => this.onToggle(e)}
                            content="Outsource"
                            disabled={outsourcedUsers.length === 0}
                        />
                    </Button.Group>
                </div>
                <div style={{ marginTop: "15px" }}>
                    {this.state.isInHouseUsersVisible && (
                        <div>
                            <Field
                                name={`${field}.skillId`}
                                component={taskMapItem
                                    ? this.renderField
                                    : this.renderFieldWithoutValidation}
                                assignMapName={`${field}.userIds`}
                                options={this.state.inHouseUserOptions}
                            />
                            {taskMapItem.canReviewerHasSkill && (
                                <Field
                                    name={`${field}.reviewingSkillId`}
                                    component={taskMapItem
                                        ? this.renderReviewerField
                                        : this.renderReviewerFieldWithoutValidation}
                                    assignMapName={`${field}.reviewingUserIds`}
                                    options={this.state.reviewingUserOptions}
                                />
                            )}
                        </div>
                    )}
                    {!this.state.isInHouseUsersVisible && (
                        <React.Fragment>
                            {outsourcedUsers && outsourcedUsers.length > 0 && (
                                <div style={{ width: "100%" }}>
                                    <div
                                        style={{
                                            display: "flex",
                                            justifyContent: "space-between",
                                            textAlign: "center",
                                        }}
                                    >
                                        <div style={{ width: "100%" }}>
                                            <Radio
                                                label="Top Rated"
                                                name={`${field}.topRated`}
                                                value={`${field}.topRated`}
                                                checked={
                                                    this.state.value ===
                                                    `${field}.topRated`
                                                }
                                                onChange={(e, o) =>
                                                    this.handleFilterChange(
                                                        e,
                                                        o,
                                                        index
                                                    )
                                                }
                                            />
                                        </div>
                                        <div style={{ width: "100%" }}>
                                            <Radio
                                                label="First Available"
                                                name={`${field}.firstAvailable`}
                                                value={`${field}.firstAvailable`}
                                                checked={
                                                    this.state.value ===
                                                    `${field}.firstAvailable`
                                                }
                                                onChange={(e, o) =>
                                                    this.handleFilterChange(
                                                        e,
                                                        o,
                                                        index
                                                    )
                                                }
                                            />
                                        </div>
                                        <div style={{ width: "100%" }}>
                                            <Radio
                                                label="Cheapest"
                                                name={`${field}.cheapest`}
                                                value={`${field}.cheapest`}
                                                checked={
                                                    this.state.value ===
                                                    `${field}.cheapest`
                                                }
                                                onChange={(e, o) =>
                                                    this.handleFilterChange(
                                                        e,
                                                        o,
                                                        index
                                                    )
                                                }
                                            />
                                        </div>
                                    </div>
                                </div>
                            )}
                            <div style={{ marginTop: "15px" }}>
                                {taskMapItem.canBeOutsourced && (
                                    <React.Fragment>
                                        <div>
                                            <Field
                                                name={`${field}.formulaTaskId`}
                                                component={taskMapItem
                                                    ? this.renderOutsourcerField
                                                    : this.renderOutsourcerFieldWithoutValidation
                                                }
                                                assignMapName={`${field}.outsourceUserIds`}
                                                options={
                                                    this.state
                                                        .outsourcedUsersOptions
                                                }
                                            />
                                            <Field
                                                name={`${field}.reviewingSkillId`}
                                                component={taskMapItem
                                                    ? this.renderReviewerField
                                                    : this.renderReviewerFieldWithoutValidation
                                                }
                                                assignMapName={`${field}.reviewingUserIds`}
                                                options={
                                                    this.state
                                                        .reviewingUserOptions
                                                }
                                            />
                                        </div>
                                    </React.Fragment>
                                )}
                            </div>
                        </React.Fragment>
                    )}
                </div>
            </div>
        );
    }
}
