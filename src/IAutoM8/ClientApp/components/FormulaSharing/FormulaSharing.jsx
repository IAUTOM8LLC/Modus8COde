import React, { Component } from 'react'
import { connect } from 'react-redux'
import { Checkbox, Header, Icon, Form } from 'semantic-ui-react'
import { error } from 'react-notification-system-redux'

import { updateShareStatus } from '@store/formula'

import { SimpleSegment } from '@components'

import SharePublicLink from './components/SharePublicLink'

import './FormulaSharing.less'

@connect(
    (state, ownProps) => ({
        ...state.formula
    }),
    { updateShareStatus, error }
)
export default class FormulaSharing extends Component {

    handleSharingStatusChange = (event, { checked: share }) => {
        const { id = 0, formulaShareStatus = {} } = this.props.formula;
        if (id === 0) return;

        formulaShareStatus.shareType = share ? 1 : 0;
        this.props.updateShareStatus(id, formulaShareStatus)
            .catch(() => {
                if (!share) return;
                this.props.error({
                    title: 'You cannot re-share formula'
                });
            });
    }

    handleReshareStatusChange = (event, { checked: allow }) => {
        const { id = 0, formulaShareStatus = {} } = this.props.formula;
        if (id === 0) return;

        formulaShareStatus.isResharingAllowed = allow;
        this.props.updateShareStatus(id, formulaShareStatus);
    }

    renderSection() {
        const { id, formulaShareStatus } = this.props.formula;

        if (formulaShareStatus.shareType === 1)
            return <SharePublicLink
                formulaId={id}
                shareStatus={formulaShareStatus}
                onResharingChange={this.handleReshareStatusChange}
            />

        return (
            <Header icon textAlign="center" className="formula-sharing__disabled">
                <Icon className="sharing-disabled" />
                Formula sharing is disabled
            </Header>
        );
    }

    render() {
        const { formulaShareStatus, isLocked } = this.props.formula;

        const shareCkeckboxLabel = `${formulaShareStatus.shareType !== 0 ? 'Disable' : 'Enable'} 
            formula sharing`;

        return (
            <div className="formula-sharing">
                <SimpleSegment>
                    <Form.Field>
                        <Checkbox
                            toggle
                            className="formula-sharing__switch"
                            onChange={this.handleSharingStatusChange}
                            checked={formulaShareStatus.shareType !== 0}
                            label={shareCkeckboxLabel}
                            disabled={isLocked}
                        />
                    </Form.Field>
                    {this.renderSection()}
                </SimpleSegment>
            </div>
        );
    }
}
