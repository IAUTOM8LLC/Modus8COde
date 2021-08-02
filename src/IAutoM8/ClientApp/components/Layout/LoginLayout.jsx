import React from 'react'
import { Grid, Image, Segment, Header } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

export default function LoginLayout({ header, linkTo, linkLabel, linkHeader,
    mainForm, leftForm, rigthForm}) {
    return (
        <div className="login-form">
            <Grid
                textAlign="center"
                verticalAlign="middle"
            >
                <div className="equal height row">
                {leftForm}
                <Grid.Column className="login-form__body">
                    <Image
                        centered
                        verticalAlign="middle"
                        src="/images/logo-big.png"
                    />

                    <Segment basic className="login-form__body-wrapper">
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
                {rigthForm}
                </div>
            </Grid>
        </div>
    );
}
