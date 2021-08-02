import React from 'react'
import { Grid, Image, Segment, Header } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

import './SignupLayout.less'


export default function SignupLayout({ header, linkTo, linkLabel, linkHeader,
    mainForm, boxClick, soloColor, grpColor, grpTextColor, soloTextColor, 
    vendor,cVendor })
    
    {
        //const vendorCheck = <div className="step2"><b>STEP 2: Create Your Vendor Account Now</b></div>
    return (
        <div className="login-form">
            <Grid
                textAlign="center"
                verticalAlign="middle"
            >
                <div className="equal height row">
                    <Grid.Column className="login-form__body setMaxWidth">
                        <div style={{
                            backgroundColor: "#5d2684", color: "white"
                            , textAlign: "right", padding: "8px"
                        }}><a href="/Login" target="_blank" style={{
                            color: "white", fontWeight: "bold",
                            fontSize: "18px"
                        }}>Login</a>
                        </div>
                        <div>
                            <img src="/images/logo-big.png"
                                className="ui centered middle aligned image"></img>
                        </div>
                        <div className="headingCss"
                            style={{ fontWeight: "bold", display: "none" }}>
                            Welcome to the Modus Vendor Signup</div>
                        <div className="congratsCss" style={{ padding: "15px", display: "none" }}>
                            Congratulations! You have been invited to join the exclusive MODUS8 community.
                            While you are registering your account,
                            are working hard behind the scenes to help you get more done and have the
                            freedom you have always wanted in your business.
                         </div>
                        <div className="watchVideo" style={{ display: "none" }}>
                            Watch Video to get started</div>

                        <iframe src="https://www.youtube.com/embed/24Xg50lHJEo"
                            allow="autoplay; encrypted-media"
                            title="video" height="400px" width="800px"
                        />
                        {/* vendor check AT 25-jan-2021 */}
                        {vendor && <div className="steps"><b>STEP 1: Select Your Account Type</b></div>}
                        {vendor && <div className="steps_block">
                            <div className="signUpButton step_left" style={{
                                float: "left",
                                backgroundColor: soloColor, 
                                color: soloTextColor

                            }}
                                onClick={() => boxClick(1)}
                            >
                                <i className="male icon"></i><br />
                                INDIVIDUAL<br />
                                It&apos;s just me!</div>


                            <span style={{ display: "none" }}>OR</span>

                            <div className="signUpButton step_right" style={{
                                float: "right",
                                backgroundColor: grpColor,
                                color: grpTextColor,
                                marginRight: "40px",
                                display: "none"
                            }} onClick={() => boxClick(2)}>
                                <i className="users icon"></i><br />
                                GROUP<br />
                                We have multiple team members under one brand
                            </div>
                        </div>}
                        {/* vendor check AT 25-jan-2021 */}
                        {vendor || cVendor ? <div className="step2">
                            <b>STEP 2: Create Your Vendor Account Now</b></div>: 
                        <div className="step2"><b>Create your Business Account</b></div>
                        }
                        

                        <Segment basic className="login-form__body-wrapper setWidth">
                            {
                                header &&
                                <Header textAlign="left">{header}</Header>
                            }

                            {mainForm}

                            <Segment basic className="login-form__links">
                                {linkHeader}
                                <br />
                                <a href={linkTo}>{linkLabel}</a>

                            </Segment>
                        </Segment>
                    </Grid.Column>
                </div>
            </Grid>
        </div>
    );
}
