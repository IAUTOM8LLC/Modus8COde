import React from 'react'
import { Menu, Dropdown, Icon } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

import { WhenPermitted } from '@components'

import { UserAcronym } from '@components'

import './UserMenuItem.less'

export default function UserMenuItem({ userName, onLogoff,role }) {
    const Trigger = (
        <div className="iauto-user-menu__trigger">
            <UserAcronym fullname={userName} />
            <span className="iauto-user-menu__trigger-label">
                {userName}
            </span>
        </div>
    )

    const options = [
        role === 'Company' ?
        (<Dropdown.Item
            as={Link}
            icon={<Icon className="profile" />}
            className="profile"
            to="/profiles"
            key="accounts"
            content="Company Profile"
        />):
        (<Dropdown.Item
            as={Link}
            icon={<Icon className="profile" />}
            className="profile"
            to="/profile"
            key="account"
            content="Profile"
        />),
        <WhenPermitted key={"credits"} rule="creditsPage">
            <Dropdown.Item
                as={Link}
                icon={<Icon className="credits" />}
                className="credits"
                to="/credits"
                key="credits"
                content="Credits"
            />
        </WhenPermitted>,
        <WhenPermitted key={"balance"} rule="balancePage">
            <Dropdown.Item
                as={Link}
                icon={<Icon className="credits" />}
                className="credit"
                to="/balance"
                key="balance"
                content="Balance"
            />
        </WhenPermitted>,
        <Dropdown.Item
            key="logout"
            icon={<Icon className="logout" />}
            content="Sign Out"
            onClick={onLogoff}
        />
    ]

    return (
        <Menu.Item name="userMenu" >
            <Dropdown
                className="iauto-user-menu"
                trigger={Trigger}
                options={options}
                icon={null}
            />
        </Menu.Item>
    );
}
