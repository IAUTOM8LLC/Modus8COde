import React, { Component } from 'react'
import CopyToClipboard from 'react-copy-to-clipboard'
import { connect } from 'react-redux'
import { success } from 'react-notification-system-redux'

import { ModusButton } from '@components'

@connect(
    null,
    { success }
)
export default class FormulaShareLink extends Component {
    static defaultProps = {
        formulaId: 0
    }

    getLink() {
        return `${document.location.origin}/formulas/share/${this.props.formulaId}`
    }

    handleCopy = () => {
        this.props.success({
            title: 'Link copied to the clipboard'
        });
    }

    renderButton() {
        if (this.props.children)
            return this.props.children;

        return (
            <ModusButton
                filled
                content="Copy share link"
                icon="share"
            />
        );
    }

    render() {
        return (
            <CopyToClipboard text={this.getLink()} onCopy={this.handleCopy}>
                {this.renderButton()}
            </CopyToClipboard>
        );
    }
}
