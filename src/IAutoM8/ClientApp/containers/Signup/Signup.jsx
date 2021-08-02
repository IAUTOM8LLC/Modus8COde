import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reset } from 'redux-form'
import { push } from 'react-router-redux'
import { success, error as nsError } from 'react-notification-system-redux'

import { register, registerVendor, saveInfusionUrl, registerCVendor } from '@store/auth'

import { SignupLayout } from '@components'

import SignupForm from './components/SignupForm'
import ChangeAccountType from './components/ChangeAccountType'

import './Signup.less';

@connect(
    state => ({ ...state.auth }),
    {
        register, pushState: push, reset, success, nsError,
        registerVendor, registerCVendor,
        saveInfusionUrl
    }
)
export default class Signup extends Component {

    constructor(props) {
        super(props);
        // Don't call this.setState() here!
        this.state = {
            fullName: "",
            email: "",
            orderId: 0,
            soloColor: "#5d2684",
            soloTextColor: "white",
            grpTextColor: "",
            grpColor: "",
            showEmailDiv: false,
            companyWorkerOwnerId: ''//Added for Passing CompanyWorkerOwnerId
        };
    }
    boxClick = (value) => {
        if (value == 1) {
            this.setState({
                soloColor: "#5d2684",
                soloTextColor: "white",
                grpTextColor: "",
                grpColor: "",
                showEmailDiv: false

            });
        }
        else {
            this.setState({
                soloColor: "",
                grpTextColor: "white",
                soloTextColor: "",
                grpColor: "#5d2684",
                showEmailDiv: true,
            });


        }

    }
    componentDidMount() {
        if (window.location.search !== null && window.location.search !== '') {
            const params = this.getParams();

            if (params.inf_field_FirstName === undefined && !this.props.cVendor) {
                this.props.pushState('/login');
            }
            else if (this.props.cVendor) {
                console.log("State", params)
                this.setState({
                    email: params.email,
                    companyname: params.companyname,
                    companyWorkerOwnerId: params.Id//Added for Passing CompanyWorkerOwnerId
                });

            }
            else {
                this.props.saveInfusionUrl({
                    email: params.inf_field_Email,
                    orderId: params.orderId * 1,
                    url: window.location.href
                });
                this.setState({
                    fullName: params.inf_field_FirstName + " " + params.inf_field_LastName,
                    email: params.inf_field_Email,
                    orderId: params.orderId * 1
                });
            }

        }

        else if (!this.props.vendor) {
            this.props.pushState('/login');
        }
    }


    componentWillMount() {
        if (this.props.isLoggedIn) {
            this.props.pushState('/');
        }
    }

    componentWillReceiveProps(nextProps) {
        if (!this.props.signedUp && nextProps.signedUp) {
            this.props.success({
                title: 'You are registered!',
                message: 'Please, check your email and follow confirmation link'
            });
            this.props.pushState('/login');
        }

        if (!this.props.error && nextProps.error) {
            this.props.nsError({
                title: 'Sign up failed!',
                message: nextProps.error.message
            });
        }
        if (!nextProps.vendor) {
            const params = this.getParams();
            if (params.inf_field_FirstName === undefined) {
                this.props.pushState('/login');
            }
        }
    }

    handleSubmit = (state) => {
        const { isLoading, vendor, cVendor,
            register, registerVendor, registerCVendor } = this.props;
        if (isLoading)
            return null;

        if (vendor) {
            return registerVendor(state);
        }
        else if (cVendor) {
            return registerCVendor(state);
        }
        else {
            return register(state)
        }

        // return vendor ? registerVendor(state) : register(state);
    }

    getParams = () => {
        const query = decodeURIComponent(decodeURIComponent(window.location.search));
        return JSON.parse('{"' + query
            .substring(1)
            .replace(/&/g, "\",\"")
            .replace(/=/g, "\":\"") + '"}');
    }

    render() {
        const { vendor, cVendor } = this.props;
        const text = !vendor ? (<div> Congratulations! You&apos;ve been invited
        to join the exclusive MODUS8 community. Whether its SEO,
        graphic design, writing, or some other skill you bring to the
        table, MODUS8 will connect you with business owners who are
        excited to hire your services right now.
            <br />
            <br />
            Register your account today and begin accepting Invitations to
            Bid on tasks you&apos;re qualified to do.
        </div>) : (<div> Congratulations! You&apos;ve been invited to join the
        exclusive MODUS8 community. While you&apos;re registering your account,
        we&apos;re working hard behind the scenes to help you get more
        done and have the freedom you&apos;ve always wanted in your business.
            <br />
            <br />
            Whether you have a team or you plan on outsourcing all your work, MODUS8
            will help know what needs to be done all while getting it done! Complete
            your registration now and get ready to enjoy your
            newfound excitement and freedom in your business and life!
        </div>);
        return (
            <SignupLayout
                vendor={vendor} //vendor prop pass to signuplayout  At 25-jan-2021
                cVendor={cVendor}
                boxClick={this.boxClick}
                soloColor={this.state.soloColor}
                soloTextColor={this.state.soloTextColor}
                grpTextColor={this.state.grpTextColor}
                grpColor={this.state.grpColor}
                // header={vendor ? "Create your vendor account" : "Create your business account"}
                linkHeader="Have an account ?"
                linkLabel="Log In"
                linkTo="/login"
                mainForm={<SignupForm
                    cVendor={cVendor}
                    showEmailDiv={this.state.showEmailDiv}
                    onSubmit={this.handleSubmit}
                    initialValues={{ ...this.state }} />
                }
                leftForm={vendor &&
                    <ChangeAccountType
                        header="Create your business account"
                        linkLabel="Start with bussiness account"
                        text={text}
                        linkTo="/signup"
                    />}
                rigthForm={!vendor &&
                    <ChangeAccountType
                        header="Create your vendor account"
                        linkLabel="Start with vendor account"
                        text={text}
                        linkTo="/vendor-signup"
                    />}
            />
        );
    }
}
