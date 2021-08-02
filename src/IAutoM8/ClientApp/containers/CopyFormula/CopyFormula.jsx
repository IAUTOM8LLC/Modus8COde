import axios from 'axios'
import React, { Component } from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux';
import { Modal, Dimmer, Loader } from 'semantic-ui-react'
import { success, error } from 'react-notification-system-redux'

import { getAuthHeaders } from '@infrastructure/auth'
import { selectFormulaFromUrl } from '@selectors/formula'

import { ModusButton } from '@components'

@connect(
    (state, props) => ({
        userPermissions: state.auth.permissions
    }),
    { pushState: push, success, error }
)
export default class CopyFormula extends Component {
    state = {
        loading: true,
        message: 'Check formula'
    }

    componentWillMount() {
        this.ensureUserCanCopyFormula();
    }

    ensureUserCanCopyFormula = () => {
        if (!this.props.userPermissions.copyFormulaFromShareLink) {
            this.props.error({
                title: 'You do not have permissions to copy formula'
            });
            this.props.pushState('/');
        } else {
            this.setState({
                loading: true,
                message: 'Check if formula can be shared'
            });
            axios.get(`/api/formula-sharing/${this.props.match.params.formulaId}/status`, getAuthHeaders())
                .then((response) => {
                    this.setState({ loading: false });
                    if (!response.data) {
                        this.props.error({
                            title: 'Cannot copy this formula'
                        });
                        this.props.pushState('/');
                    }
                })
                .catch(() => {
                    this.setState({ loading: false });
                    this.props.error({
                        title: 'Cannot copy this formula'
                    });
                    this.props.pushState('/');
                });

        }
    }

    copyFormula = () => {
        this.setState({
            loading: true,
            message: "Sharing formula"
        });
        axios.get(`/api/formula-sharing/${this.props.match.params.formulaId}`, getAuthHeaders())
            .then(() => {
                this.props.pushState('/formulas')
                this.setState({ loading: false });
            })
            .catch((response) => {
                this.props.error({
                    title: response.data.message
                        ? response.data.message
                        : response.data
                });
                this.props.pushState('/');
                this.setState({ loading: false });
            });
    }

    handleClose = () => {
        this.props.pushState('/');
    }

    render() {
        const { loading,
            message} = this.state;

        return (
            <div>
                <Dimmer page active={loading}>
                    <Loader>{message}</Loader>
                </Dimmer>

                <Modal
                    open={!loading}
                    size="small"
                    onClose={this.handleClose}
                >
                    <Modal.Header>You have been shared a formula!</Modal.Header>

                    <Modal.Content>
                        <p>Do you want to copy a shared formula to your account ?</p>
                    </Modal.Content>

                    <Modal.Actions className="modal-flex-actions">
                        <ModusButton
                            className="button-flex-order1"
                            filled
                            content="Yes"
                            onClick={this.copyFormula}
                        />
                        <ModusButton
                            className="button-flex-order2"
                            grey
                            content="No"
                            onClick={this.handleClose}
                        />
                    </Modal.Actions>
                </Modal>
            </div>
        );
    }
}
