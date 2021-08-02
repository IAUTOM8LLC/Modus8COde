import React from 'react'
import { connect } from 'react-redux'

import { Intercom } from '@components'

const appId = () => {
    const devKey = 'gmnkdflq';
    const prodKey = 'fckgw2sw';
    const isProd = window.location.hostname === "gomodus8.com"; // dirty hack 
    return isProd ? prodKey : devKey;
}

const mapIntercomUser = ({ auth }) => {
    const { user } = auth;
    const appID = appId();

    if (!user || !user.name || !user.fullName) {
        return { appID }
    }

    return {
        appID,
        user: {
            email: user.name,
            name: user.fullName
        }
    }
}

const IntercomManager = ({ appID, user }) =>
    <Intercom appID={appID} {...user} />

export default connect(mapIntercomUser)(IntercomManager)