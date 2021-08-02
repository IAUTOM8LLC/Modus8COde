import React, { Component } from 'react'
import cn from 'classnames'

import { Header, Dropdown } from 'semantic-ui-react'

import { SimpleSegment } from '@components'

export default class FormulasCategoryDropdown extends Component {
    state = {
        selectedCategories: [],
        visible: false
    };

    node = null;
    childNodes = [];
    componentWillReceiveProps(nextProps) {
        this.childNodes = [];
        const mapNodeToArray = (node) => {
            Array.from(node.childNodes)
                .map(childNode => mapNodeToArray(childNode));
            this.childNodes.push(node);
        }
        mapNodeToArray(this.node);
        this.setState({
            ...this.state,
            selectedCategories: nextProps.filterCategorieIds
        });
    }

    handleOnClick = () => {
        const nextVisible =
            this.state.selectedCategories.length === this.props.categories.length
                ? false : !this.state.visible;
        if (nextVisible) {
            // attach/remove event handlers
            document.addEventListener('click', this.handleOutsideClick, false);
        } else {
            document.removeEventListener('click', this.handleOutsideClick, false);
        }

        this.setState({
            ...this.state,
            visible: nextVisible
        })
    }
    
    handleOutsideClick = e => {
        // ignore clicks on the component itself
        if (this.childNodes.some(n => n === e.target)) {
            return;
        }
        this.handleOnClick();
    }

    handleOnSelect = id => {
        const newCategories = this.state.selectedCategories.concat([id]);
        this.props.formulaCategoryApply(newCategories);
        if (newCategories.length === this.props.categories.length)
            this.handleOnClick();
    }

    render() {
        const { selectedCategories, visible } = this.state;
        const { categories } = this.props;
        return (
            <div ref={node => this.node = node}>
                <SimpleSegment clearing floated="left"
                    className="iauto-formulas-header_dropdown" style={{ margin: '10px 0 0 10px' }}>
                    <div onClick={this.handleOnClick}>CATEGORIES <i className="icon dropdown" /></div>
                    <div className={cn(['iauto-formulas-header_menu', visible && 'visible'])}>
                        {categories.map(category =>
                            selectedCategories.some(id => id === category.id)
                                ? null
                                : (
                                    <div key={category.id} className="item"
                                        onClick={() => this.handleOnSelect(category.id)}>
                                        <span className="text">{category.name}</span>
                                    </div>))}
                    
                    </div>
                </SimpleSegment>
            </div>
        );
    }
}
