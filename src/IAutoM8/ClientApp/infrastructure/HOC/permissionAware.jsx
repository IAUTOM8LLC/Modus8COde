import React from 'react'
import { connect } from 'react-redux'

/**
 * @param {Function|String} selectPermissionFromState Permission value selector. 
 * Can be a function selector of string permission name. 
 */
export default function permissionAware(selectPermissionFromState) {

    return (WrappedComponent) => {

        function mapStateToProps(state) {
            switch (typeof selectPermissionFromState) {
                case 'function':
                    return {
                        hasPermission: selectPermissionFromState(state)
                    }

                case 'string':
                    return {
                        hasPermission: state.auth.permissions[selectPermissionFromState]
                    }

                default: throw 'Permission selector must be either function or string.'
            }
        }

        @connect(mapStateToProps)
        class PermissionAwareComponent extends WrappedComponent {

            onlyRenderWhenPermitted() {
                return typeof super.renderDenied === 'undefined'
                    && typeof super.renderPermitted === 'undefined';
            }

            renderDenied() {
                return super.renderDenied && super.renderDenied();
            }

            renderPermitted() {
                return super.renderPermitted && super.renderPermitted();
            }

            renderPermissioned() {
                return this.props.hasPermission ? this.renderPermitted() : this.renderDenied();
            }

            render() {
                if (this.onlyRenderWhenPermitted()) {
                    return this.props.hasPermission && <WrappedComponent {...this.props} />
                }

                return <WrappedComponent
                    permissioned={this.renderPermissioned()}
                    {...this.props}
                />
            }
        }

        return PermissionAwareComponent;
    }
}