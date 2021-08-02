import React from 'react'
import { Link } from 'react-router-dom'
import { Menu, Icon, Sidebar } from 'semantic-ui-react'

import { WhenPermitted } from '@components'

const pathMatches = (path = '', to = '', items = []) => {


    if (to.length > 1 && path.length > 1) {

        if (to === '/projects') {
            return !path.includes('getTasksFromAllProjects')
                && path.startsWith('/projects');
        }
        else {
            return path.substring(1).includes(to.substring(1));
        }
    } else {
        if (to === '/' && path.includes("admin")) {
            return true;
        }
        else if (to === '/') {
            return path.includes('getTasksFromAllProjects');
        }
        else {
            return path === to;
        }
    }
};

const renderMenu = (items, currentPath) => items.map(item => {
    const menuItem = (
        <Menu.Item
            key={item.name}
            as={Link}
            to={item.to}
            active={pathMatches(currentPath, item.to, items)}
        >
            <Icon className={item.className} />
            {item.name}
            {item.menu && renderMenu(item.menu)}
        </Menu.Item>
    );

    return item.permission ?
        <WhenPermitted key={item.name} rule={item.permission}>{menuItem}</WhenPermitted>
        : menuItem;
});

export default function SidebarMenu({ path, visible, className }) {
    const items = [
        { name: 'Home', to: '/', className: 'home' },
        { name: 'Formulas', to: '/formulas', className: 'formulas', permission: 'formulasPage' },
        { name: 'Projects', to: '/projects', className: 'projects', permission: 'projectsPage' },
        { name: 'Notifications', to: '/notifications', className: 'notifications' },
        { name: 'Users', to: '/users', className: 'users', permission: 'usersPage' },
        { name: 'Skills', to: '/skills', className: 'skills', permission: 'skillsPage' },
        { name: 'Teams', to: '/teams', className: 'teams', permission: 'teamsPage' },
        { name: 'Clients', to: '/clients', className: 'clients', permission: 'clientsPage' },
        { name: 'Balance', to: '/balance', className: 'credits', permission: 'balancePage' },
        { name: 'Credits', to: '/credits', className: 'credits', permission: 'creditsPage' },
        //{ name: 'All-Users', to: '/adminUsers', className: 'users', permission: 'adminUsersPage' },
        { name: 'Payment', to: '/payment', className: 'credits', permission: 'payment' }
    ].filter(item => item);

    return (
        <Sidebar
            borderless
            as={Menu}
            vertical
            fixed="left"
            animation="push"
            visible={visible}
            className={className}
        >
            <Menu.Item header />
            {renderMenu(items, path)}
            <a className="item" target="_blank" rel="noopener noreferrer"
                href="https://support.i-autom8.com/">
                <i aria-hidden="true" className="icon sign language"
                    style={{ backgroundPosition: "-84px -156px",
                        height: "19px",
                        margin:"-4px 25px 0 -2px"}}></i>Support</a>
        </Sidebar>
    );
}
