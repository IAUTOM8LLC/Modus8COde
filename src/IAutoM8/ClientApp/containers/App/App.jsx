import React, { Component } from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import { Segment, Transition } from 'semantic-ui-react' // eslint-disable-line no-unused-vars
import cn from 'classnames'
import { Menu} from 'semantic-ui-react'

import withAuth from '@infrastructure/HOC/withAuth'
import { logoff } from '@store/auth'

import { SidebarMenu, Navbar } from '@components'

import '../../semantic'
import '../../styles/global.less'

import './App.less'
import Profiles from '../Profiles/Profiles'

@connect(
    state => ({
        user: state.auth.user,
        roles: state.auth.user.roles,
    }),
    { logoff, pushState: push }
)
export default class App extends Component {
    state = {
        sidebarVisible: true
    }

    logoffClick = () => {
        this.props.logoff();
        this.props.pushState('/login');
    }

    render() {
        const {
            user,
            location,
            roles
        } = this.props;
        const role = roles.toString()
        console.log('role', role);

        const { sidebarVisible } = this.state;

        const mainClass = cn([
            'iauto-root__main',
            sidebarVisible && 'iauto-root__main--with-sidebar'
        ]);
        // {role !== "Company" && (<SidebarMenu
        //     className="iauto-root__sidebar"
        //     path={location.pathname}
        //     visible={sidebarVisible}
        // />)}
        return (
            <div className="iauto-root">
                    {role !== "Company" && (<SidebarMenu
                        className="iauto-root__sidebar"
                        path={location.pathname}
                        visible={sidebarVisible}
                    />)}
                
                <div>
                    
                    <Navbar
                    className="iauto-root__navbar"
                    user={user}
                    path={location.pathname}
                    onLogoff={this.logoffClick}
                />
                {role == "Company" && (<Menu.Item header />
                )}
                    </div>
                <div className={ role !== "Company" ? mainClass: ""}>
                <Segment basic padded className="iauto-root__container">
                            {this.props.children}
                        </Segment>
                    </div>
            </div>       
            
            
        );
    }
}

export const AppWithAuth = withAuth(App);
