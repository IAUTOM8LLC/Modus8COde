import React from 'react'
import { connect } from 'react-redux'

@connect((state, ownProps) => ({
    hasPermission: state.auth.permissions[ownProps.rule]
}))
export default class WhenPermitted extends React.Component {
    componentWillMount() {
        if (typeof this.props.hasPermission === 'undefined')
            throw `Permission (${this.props.rule}) doesn't exist; At ${this.props.children.type.name}`;
    }

    render() {
        const { children, hasPermission } = this.props;

        if (typeof children === 'function')
            return children(hasPermission)

        return hasPermission && children;
    }
}