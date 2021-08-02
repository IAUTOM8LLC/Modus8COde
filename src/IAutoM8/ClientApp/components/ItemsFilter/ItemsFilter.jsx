import React, { Component } from 'react'
import { Dropdown } from 'semantic-ui-react'

import './ItemsFilter.less'

export default class ItemsFilter extends Component {

    handleChange = (e, { value }) => {
        this.props.setFilter(value);
    }

    componentDidMount() {
        const { setFilter, defaultValue = 'name' } = this.props;
        setFilter(defaultValue);
    }

    componentWillUnmount() {
        if (this.props.doNotReset) return;
        this.props.setFilter(null);
    }

    render() {
        const { by, filterValue = 'name', label = 'FILTER BY:', scrolling } = this.props;
        return (
            <div className="modus-items-filter">
                {label &&
                    <label>
                        <strong>{label}</strong>
                    </label>
                }
                <Dropdown
                    scrolling={scrolling}
                    options={by}
                    value={filterValue}
                    onChange={this.handleChange}
                />
            </div>
        );
    }
}
