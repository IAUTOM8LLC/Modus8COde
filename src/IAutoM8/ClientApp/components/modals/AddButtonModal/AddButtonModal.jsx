import React, { Component } from "react";
import { reduxForm } from "redux-form";
import { Modal, Form } from "semantic-ui-react";

import { required, email, minUsersNumber } from "@utils/validators";

import editModalHoc from "../../common/editModalHoc";

import {
    TextInput,
    DropdownInput,
    WhenPermitted,
    ModusButton,
} from "@components";

// const validate = (values, props) => {
//     const { teamName, teamSkills, id } = values;
//     const { teams } = { ...props };

//     const isNotDuplicated = () => {
//         const errorMessage = `Team ${teamName} already exists`;

//         const filteredTeams = teams.filter((t) => t.id !== id);
//         const lowercase = teamName.toLowerCase().trim();
//         const isTeamNameExists = filteredTeams.some(
//             (t) => t.teamName.toLowerCase().trim() === lowercase
//         );

//         if (!values.hasOwnProperty("id") && isTeamNameExists) {
//             return errorMessage;
//         }

//         if (isTeamNameExists) {
//             return errorMessage;
//         }

//         return undefined;
//     };

//     return {
//         teamName: required(teamName) || isNotDuplicated(),
//         teamSkills: minUsersNumber(1)(teamSkills),
//     };
// };

@editModalHoc
@reduxForm({
    form: "addButtonModal",
    enableReinitialize: true,
    // validate,
})
export default class AddButtonModal extends Component {
    render() {
        const {
            open,
            loading,
            onClose,
            handleSubmit,
            // teamsDll,
        } = this.props;

        return (
            <Modal
                as="form"
                open={open}
                className="task-details-modal frm-teams"
                size="small"
                onSubmit={handleSubmit}
                onClose={onClose}
            >
                <Modal.Header>
                    Add Users
                </Modal.Header>

                <Modal.Content>
                    <Form as="section">

                        <div>

                            <TextInput
                                fluid
                                name="email"
                                label="Email"
                                validate={[required, email]}
                            />
                            {/* <DropdownInput
                                    search
                                    fluid                                    
                                    name="id"
                                    label="Role"
                                    options={teamData}
                                    validate={[required, minUsersNumber(1)]}
                                /> */}
                        </div>
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                            className="button-flex-order1"
                            filled
                            content="Save"
                            type="submit"
                            loading={loading}
                        />
                     <ModusButton
                        className="button-flex-order2"
                        grey
                        type="button"
                        content="Close"
                        onClick={onClose}
                    />
                </Modal.Actions>
            </Modal>
        );
    }
}
