import React, { Component } from "react";
import { reduxForm } from "redux-form";
import { Modal, Form } from "semantic-ui-react";

import { required, alphaNumeric, minUsersNumber } from "@utils/validators";

import {
    TextInput,
    DropdownInput,
    WhenPermitted,
    ModusButton,
} from "@components";

import editModalHoc from "../../common/editModalHoc";

const validate = (values, props) => {
    const { name, userSkills, id } = values;
    const isNotDuplicated = () => {
        const lowerCase = name.toLowerCase().trim();
        if (
            (!id &&
                props.skills.some(
                    (s) => s.name.toLowerCase().trim() === lowerCase
                )) ||
            (id &&
                props.skills.some(
                    (s) =>
                        s.name.toLowerCase().trim() === lowerCase && s.id !== id
                ))
        )
            return `${name} is existed`;
        return undefined;
    };
    return {
        name: required(name) || isNotDuplicated(),
        userSkills: minUsersNumber(1)(userSkills),
    };
};

@editModalHoc
@reduxForm({
    form: "skillFormModal",
    enableReinitialize: true,
    validate,
})
export default class SkillFormModal extends Component {
    render() {
        const {
            open,
            loading,
            onClose,
            handleSubmit,
            users,
            isSkillContainer,
            isAdmin,
            //initialValues,
        } = this.props;
        const otherThanAdmin = this.props.initialValues && this.props.initialValues.isGlobal // 22-12-2020 AT_Bugs
        const options = users.map((u) => ({
            key: u.id,
            value: u.id,
            text: `${u.fullName} (${u.userName})`,
        }));

        const userRole =
            users.length === 0
                ? "empty"
                : users[0].roles.length === 0
                ? "empty"
                : users[0].roles[0];

        // Skill name shouldn't to be editable, during the skill edit
        // during edit, "this.props.initialValues" has some data
        const disabled = this.props.initialValues ? true : false;

        return (
            <Modal
                as="form"
                open={open}
                onSubmit={handleSubmit}
                className="task-details-modal"
                size="small"
                onClose={onClose}
            >
                <Modal.Header>
                    {isSkillContainer
                        ? "Skill details"
                        : isAdmin
                        ? "ARE YOUR SURE YOU WANT TO ADD A SKILL?"
                        : "ARE YOUR SURE YOU WANT TO ADD A CUSTOM SKILL?"}
                </Modal.Header>

                <Modal.Content>
                    {!isSkillContainer && (
                        <Modal.Description>
                            <span>
                                Adding a skill will only be visible in your
                                account and not available for outsourcing. If
                                you would like an outsourcer to become certified
                                in this task, please ask support.
                            </span>
                        </Modal.Description>
                    )}
                    <Form as="section">
                        <TextInput
                            fluid
                            name="name"
                            label="Skill Name"
                            placeholder={
                                isSkillContainer ? "" : "Add name of the skill"
                            }
                            disabled={userRole === "Admin" ? "" :otherThanAdmin === false?"":disabled}
                            //disabled={userRole === "Admin"  ? "" : disabled}
                            validate={[required, alphaNumeric]}
                        />
                        {userRole !== "Admin" && (
                            <DropdownInput
                                fluid
                                multiple
                                name="userSkills"
                                label={
                                    isSkillContainer
                                        ? "Skill Users"
                                        : "Assign Skills"
                                }
                                options={options}
                                placeholder={
                                    isSkillContainer ? "" : "Select worker"
                                }
                                validate={[required, minUsersNumber(1)]}
                            />
                        )}
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <WhenPermitted rule="editSkill">
                        <ModusButton
                            className="button-flex-order1"
                            filled
                            content={isSkillContainer ? "Save" : "Yes I'm Sure"}
                            type="submit"
                            loading={loading}
                        />
                    </WhenPermitted>
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
