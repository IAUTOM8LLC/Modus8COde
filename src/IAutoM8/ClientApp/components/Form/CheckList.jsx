import React, { Component } from 'react'
import { List, Checkbox } from 'semantic-ui-react'
import { v4 as uuidv4 } from 'uuid'

export default class CheckList extends Component {

    handleMasterChange = (values) => {
        if (values.checked) {
            this.props.onCheckAll();
        } else if (!values.indeterminate) {
            this.props.onUncheckAll();
        }
    }

    render() {
        const {
            values,
            options,
            header,
            onChange
        } = this.props;

        const allChecked = values.length === options.length;
        const noneChecked = values.length === 0;

        return (
            <List>
                <List.Content>
                    {/* <List.Item>
                        <Checkbox
                            onChange={(e, d) => this.handleMasterChange(d)}
                            name="master"
                            checked={allChecked}
                            label={header}
                            indeterminate={!allChecked && !noneChecked}
                        />
                    </List.Item> */}

                    <List.Item>
                        <List.List>
                            <List.Item>
                                <Checkbox
                                    onChange={(e, d) => this.handleMasterChange(d)}
                                    name="master"
                                    checked={allChecked}
                                    label={header}
                                    indeterminate={!allChecked && !noneChecked}
                                />
                            </List.Item>
                            {
                                options.map((option) =>
                                    <List.Item key={uuidv4()}>
                                        <Checkbox
                                            label={option.text}
                                            onChange={(e, d) => onChange(option.value, d)}
                                            checked={values.indexOf(option.value) !== -1}
                                        />
                                    </List.Item>
                                )
                            }
                        </List.List>
                    </List.Item>
                </List.Content>
            </List>
        );
    }
}
