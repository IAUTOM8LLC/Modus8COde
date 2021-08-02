import React from 'react'
import { reduxForm } from 'redux-form'
import { Form, Grid } from 'semantic-ui-react'

import { email, fullName } from '@utils/validators'

import { DropdownInput, TextInput, ModusButton } from '@components'

let AddUserForm = ({ handleSubmit, submitting }) => {
    const roleOptions = [
        { value: 'Worker', text: 'Worker' },
        { value: 'Manager', text: 'Manager' }
    ];

    return (
        <Form onSubmit={handleSubmit} className="modus-users__new-user">
            <Grid columns={4}>
                <Grid.Row stretched>
                    <Grid.Column>
                        <TextInput
                            required
                            validate={[fullName]}
                            name="fullName"
                            label="Full Name"
                        />
                    </Grid.Column>

                    <Grid.Column >
                        <TextInput
                            required
                            validate={[email]}
                            name="email"
                            label="Email Address"
                        />
                    </Grid.Column>

                    <Grid.Column>
                        <DropdownInput
                            name="role"
                            label="Role"
                            options={roleOptions}
                        />
                    </Grid.Column>
                    <Grid.Column stretched={false} textAlign="right" verticalAlign="bottom">
                        <ModusButton
                            filled
                            className="modus-users__new-user-button"
                            icon="iauto--add-white"
                            type="submit"
                            loading={submitting}
                            content="Add user"
                        />
                    </Grid.Column>
                </Grid.Row>
            </Grid>
        </Form>
    );
}

AddUserForm = reduxForm({
    form: 'addAssignedUser'
})(AddUserForm)

export default AddUserForm;
