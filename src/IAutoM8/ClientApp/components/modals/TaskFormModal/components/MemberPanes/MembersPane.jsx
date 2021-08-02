import React, { Component } from 'react'
import { connect } from 'react-redux'
import { Form, Checkbox } from 'semantic-ui-react'

import { DropdownInput } from '@components'

import { loadUsers } from '@store/users'

import AddCheckbox from '../AddCheckbox/AddCheckbox';

@connect(
    (state, props) => ({
        users: state.users.users,
        isNewTask: state.pendingTask.pendingTask.isNewTask
    }),
    { loadUsers }
)
export default class MembersPane extends Component {
    state = {
        requireTodo: false
    }

    componentDidMount() {
        this.props.loadUsers();
    }

    updateTodoStatus = () => {
        this.setState(({requireTodo}) => ({requireTodo: !requireTodo}));
    }

    render() {
        const {
            canEdit,
            changeField,
            hasVendor
        } = this.props;

        const userItems = this.props.users.map(t => ({
            key: t.id, value: t.id, text: t.fullName
        }));
        userItems.unshift({ key: 0, value: 0, text: 'Select user' });

        const reviewingUsers = this.props.users.filter(t => t.roles.includes("Owner")
            || t.roles.includes("Manager")).map(t => ({
            key: t.id, value: t.id, text: t.fullName
        }));
        reviewingUsers.unshift({ key: 0, value: 0, text: 'Select user' });

        const checkboxStyles = {
            padding: '5px 0 12px 0',
            fontWeight: 'bold',
            display: 'block'
        };

        return (
            <div className="task-details-modal__pane" >
                <Form as="section">
                    {<DropdownInput
                        required
                        fluid
                        multiple
                        name="assignedUserIds"
                        label="Users who have will work on this task"
                        options={userItems}
                    />}

                    <DropdownInput
                        required
                        fluid
                        multiple
                        name="reviewingUserIds"
                        label="Users who will review this task"
                        options={reviewingUsers}
                    />

                    {this.props.isNewTask &&
                    <Checkbox
                        style={checkboxStyles}
                        label="Add reviewer checklist"
                        onChange={this.updateTodoStatus}
                        checked={this.state.requireTodo}
                    />}

                    {
                        this.state.requireTodo &&
                        <AddCheckbox
                            reviewerTodo={this.state.requireTodo} 
                            isFormulaTask={false}
                        />
                    }
                </Form>
            </div>
        );
    }
}
