import React from 'react'
import { Form, Grid } from 'semantic-ui-react'

import { fullName } from '@utils/validators'

import { TextInput, NotificationSettingsListInput, SimpleSegment, DateTimeInput } from '@components'

export default function BusinessProfile() {
    return (
        <Grid columns={2} className="profile-container__business-section">
            <Grid.Column>
                <Form as={SimpleSegment} className="profile-container__business">
                    <h2>Business information</h2>
                    <TextInput
                        fluid
                        validate={[fullName]}
                        name="name"
                        label="Name"
                    />
                    <TextInput
                        fluid
                        name="address"
                        label="Address"
                    />
                    <TextInput
                        fluid
                        name="occupation"
                        label="Occupation"
                    />
                    <DateTimeInput
                        required
                        name="toDoSummaryTime"
                        label="To do summary time"
                        withoutDate={true}
                    />
                </Form>
            </Grid.Column>

            <Grid.Column>
                <Form as={SimpleSegment} className="profile-container__notifications">
                    <h2>Notification settings</h2>
                    <NotificationSettingsListInput
                        name="notificationSettings"
                    />
                </Form>
            </Grid.Column>

            <Grid.Column>
                <Form as={SimpleSegment} className="profile-container__business">
                    <h2>Affiliate information</h2>
                    <TextInput
                        fluid
                        readOnly
                        name="goldAffUrl"
                        label="Share your gold link"
                    />
                    <TextInput
                        fluid
                        readOnly
                        name="silverAffUrl"
                        label="Share your silver link"
                    />
                    <TextInput
                        fluid
                        readOnly
                        name="affLoginUrl"
                        label="Login page"
                    />
                    <TextInput
                        fluid
                        readOnly
                        name="affCode"
                        label="User name"
                    />
                    <TextInput
                        fluid
                        readOnly
                        name="affPass"
                        label="Password"
                    />
                </Form>
            </Grid.Column>
        </Grid>
    );
}
