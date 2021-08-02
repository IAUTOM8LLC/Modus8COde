import React, { Component } from 'react'
import { connect } from 'react-redux'
import { Input, Feed } from 'semantic-ui-react'

import { addComment, deleteComment } from '@store/comment'
import { loadUserTasksInStatus } from "@store/projectTasks";

import { TimeAgo, ModusButton, UserAcronym, Inline } from '@components'

import './CommentPane.less'

@connect(
    (state) => ({
        ...state.comment,
        userName: state.auth.user.fullName
    }),
    {
        addComment,
        deleteComment,
        loadUserTasksInStatus
    }
)
export default class CommentPane extends Component {
    state = {
        comment: ''
    }

    handleChange = (event) => {
        this.setState({ comment: event.target.value });
    }

    handleAdd = () => {
        const { comment = '' } = this.state;

        if (comment.trim() === '') {
            return;
        }

        this.props.addComment({ taskId: this.props.taskId, text: comment })
            .then(() => this.props.loadUserTasksInStatus());

        this.setState({ comment: '' });
    }

    handleKeyPress = (e) => {
        if (e.charCode === 13 /* enter */) {
            e.preventDefault();
            this.handleAdd();
        }
    }

    render() {

        return (
            <div className="task-details-modal__pane">
                <div className="comments-pane">

                    <div className="comments-pane__add-section">
                        <Inline>
                            <UserAcronym
                                fullname={this.props.userName}
                                popup={this.props.userName}
                            />
                            <Input
                                placeholder="Write a comment..."
                                value={this.state.comment}
                                onChange={this.handleChange}
                                onKeyPress={this.handleKeyPress}
                            />
                            <ModusButton
                                wide
                                filled
                                type="button"
                                content="Post"
                                onClick={this.handleAdd}
                            />
                        </Inline>
                    </div>

                    <div className="comments-pane__separator">
                        <p>Comments ({this.props.comments.length})</p>
                    </div>

                    <Feed className="comments-pane__feed">
                        {
                            this.props.comments.map((option, index) => (
                                <Feed.Event key={index}>
                                    <Feed.Label>
                                        <UserAcronym
                                            fullname={option.author}
                                            popup={option.author}
                                        />
                                    </Feed.Label>

                                    <Feed.Content>
                                        <Feed.Summary>
                                            <Feed.User>{option.author}</Feed.User>
                                            <Feed.Date>
                                                <TimeAgo
                                                    date={option.postedTime}
                                                    format="MMM DD h:mm a"
                                                />
                                            </Feed.Date>
                                            <ModusButton
                                                circular
                                                type="button"
                                                icon="delete"
                                                size="tiny"
                                                className="delete-comment"
                                                onClick={() => this.props.deleteComment(option.id)}
                                            />
                                        </Feed.Summary>

                                        <Feed.Extra text>
                                            {option.text}
                                        </Feed.Extra>
                                    </Feed.Content>
                                </Feed.Event>
                            ))
                        }
                    </Feed>
                </div>
            </div>
        );
    }
}