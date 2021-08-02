import React from 'react'
import { Form, Input, Grid } from 'semantic-ui-react'
import moment from 'moment'
import Avatar from "react-avatar";

import FileInput from "react-fine-uploader/file-input";

import { fullName } from '@utils/validators'

import { TextInput, SimpleSegment } from '@components'

export default function UserProfile({
    loading,
    userFullName,
    profileImageUri, 
    lineColor, 
    acceptTypes, 
    uploader,
}) {
    const useIntl = typeof window.Intl !== 'undefined' &&
        typeof window.Intl.DateTimeFormat !== 'undefined';
    const timezoneLabel = useIntl ? "Your timezone is:" : "Your timezone offset is:";
    return (
        <Form as={SimpleSegment} className="profile-container__user">
            <Grid columns={2} className="profile-container__business-section">
                <Grid.Column>
                    <h2>User information</h2>
                    <TextInput
                        fluid
                        required
                        validate={[fullName]}
                        name="fullName"
                        label="Full name"
                    />
                    <TextInput fluid readOnly name="email" label="Email" />
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
                </Grid.Column>
                <Grid.Column style={{ paddingLeft: "200px" }}>
                    <div
                        className="image-box"
                        style={{
                            borderColor: lineColor,
                            borderRadius: "25px",
                            border: "1px solid #7d94a0",
                            padding: "20px",
                            width: "250px",
                            height: "300px",
                        }}
                    >
                        {profileImageUri && !loading && (
                            <div className="circular--landscape">
                                <img src={profileImageUri} />
                            </div>
                        )}
                        {!profileImageUri && !loading && (
                            <div
                                className="circular--landscape"
                                style={{
                                    display: "inline-block",
                                    position: "relative",
                                    borderWidth: "1px",
                                }}
                            >
                                <Avatar
                                    name={userFullName}
                                    size="210"
                                    round={true}
                                />
                            </div>
                        )}
                        <div style={{ paddingTop: "10px" }}>
                            <FileInput
                                accept={acceptTypes}
                                uploader={uploader}
                                className="ui button iauto purple wide"
                            >
                                Add Profile Picture
                            </FileInput>
                        </div>
                    </div>
                </Grid.Column>
            </Grid>
        </Form>
    );
}
