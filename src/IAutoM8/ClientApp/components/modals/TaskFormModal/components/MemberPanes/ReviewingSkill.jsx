import React from 'react'
import { Checkbox } from 'semantic-ui-react'

import { DropdownInput, TextInput, RichEditor} from '@components'

import AddCheckbox from '../AddCheckbox/AddCheckbox'

export default class ReviewingSkill extends React.Component {
    state = {
        requireReview: false,
        requireTodo: false
    };

    componentDidMount() {
        this.updateState();
    }

    componentDidUpdate(prevProps) {
        if (prevProps.reviewingSkillId !== this.props.reviewingSkillId) {
            this.updateState();
        }
    }

    updateState() {
        const { reviewingSkillId, todosExist } = this.props;
        this.setState(() => ({
            requireReview: reviewingSkillId !== null &&
                typeof reviewingSkillId !== 'undefined',
            requireTodo: todosExist
        }));
    }

    updateReviewVisibilityStatus = (event, data) => {
        this.setState({ requireReview: data.checked });
        if (!data.checked) {
            this.props.changeField("reviewingSkillId", null);
        }
    }

    updateTodoStatus = (event, data) => {
        this.setState({ requireTodo: data.checked });
    }


    handleAddItem(field, e, { value }) {
        this.props
            .addSkill({ name: value })
            .then(({ value }) =>
                this.props.changeField(field, value.data.id));
    }

    render() {
        const { canEdit, onChangeReviewingSkill } = this.props;
        const checkboxStyles = {
            padding: '5px 0 12px 0',
            fontWeight: 'bold',
            display: 'block'
        };

        return [
            <Checkbox
                key="1"
                disabled={!canEdit}
                checked={this.state.requireReview}
                label="Require someone to review"
                onChange={this.updateReviewVisibilityStatus}
                style={checkboxStyles}
            />,
            this.state.requireReview &&
            <DropdownInput
                key="2"
                disabled={!canEdit}
                label="Select reviewing skill"
                onChange={onChangeReviewingSkill}
                name="reviewingSkillId"
                {...this.props.dropdownProps}
            />,
            <Checkbox
                key="3"
                label="Add reviewer checklist"
                onChange={this.updateTodoStatus}
                style={checkboxStyles}
                checked={this.state.requireTodo}
            />,
            (this.state.requireTodo &&
                <div>
                    <AddCheckbox
                        key={4}
                        reviewerTodo={this.state.requireTodo}
                        isFormulaTask={true}
                    />
                    <div style={{ marginTop: "17px" }}>

                    <RichEditor
                        label="Reviewer Training"
                        name="reviewerTraining"
                        placeholder="Reviewer Training"
                    />
                    </div>
                </div>
            )
        ]
    }
}
