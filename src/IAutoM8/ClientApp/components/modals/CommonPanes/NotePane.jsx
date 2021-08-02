import React, { Component } from "react";
import { Fragment } from "react";
import { Dropdown, Icon } from 'semantic-ui-react';
import moment from "moment";
import axios from 'axios';

import { getAuthHeaders } from '@infrastructure/auth';

import { formatURL } from "@utils/formatURL";

import { ModusButton, TimeAgo, Inline } from "@components";

// import { Button } from "../TaskFormModal/components/AddCheckbox/Button";

import "./NotePane.less";

export default class NotePane extends Component {
    constructor(props) {
        super(props);

        this.state = {
            noteText: "",
        };
    }

    handleChange = (event) => {
        this.setState({ noteText: event.target.value });
    };
    handleNewAddNotes = () => {
        const { formulaId, onAddNewNotes } = this.props;

        const taskNote = {
            text: this.state.noteText,
            formulaId: formulaId ? formulaId : 0,
        };

        onAddNewNotes(taskNote);
        this.setState({ noteText: "" });
    };

    handleShareWithFormula = () => {
        const { formulaId, taskId, onAddNewNotes,projectId } = this.props;
        const { noteText } = this.state;

        axios.get(`/api/tasks/getformulasharedtasks/${taskId}`, getAuthHeaders())
            .then(response => {
                const filteredSharedTaskIds = response.data.filter(id => id !== taskId);
                const taskNote = {
                    text: noteText,
                    formulaId: formulaId ? formulaId : 0,
                    projectTaskId: taskId ? taskId : -999,
                    projectId:projectId ? projectId:0
                };
                onAddNewNotes(taskNote, filteredSharedTaskIds);

                this.setState({ noteText: "" });
            })
            .catch(() => this.setState({ noteText: "" }));
    };

    handleShareToDownstream = () => {
        const { formulaId, taskId, onAddNewNotes,projectId } = this.props;
        const { noteText } = this.state;

        // Returns the TaskIds for which the Note should be shared.
        axios.get(`/api/tasks/getdownstreamsharedtasks/${taskId}`, getAuthHeaders())
            .then(response => {
                const filteredSharedTaskIds = response.data.filter(id => id !== taskId);
                const taskNote = {
                    text: noteText,
                    formulaId: formulaId ? formulaId : 0,
                    projectTaskId: taskId ? taskId : -999,
                    projectId:projectId ? projectId:0
                };
                onAddNewNotes(taskNote, filteredSharedTaskIds);
                this.setState({ noteText: "" });
            })
            .catch(() => this.setState({ noteText: "" }));
    };
    
    urlify = (text) => {
        const urlRegex = /(https?:\/\/[^\s]+)/g;
        const urlText = text.replace(urlRegex, (url) => {
            return `<a href="${url}" target="_blank" rel="noopener noreferrer">${url}</a>`;
        });
        const newText = urlText.split('\n').map(i => {
            return `${i}<br/>`
        }).join("");
        return newText;
    };

    renderNotes() {
        const { isFormulaTask, taskNotes } = this.props;
        
        if (taskNotes && taskNotes.length > 0) {
            return (
                <ul className="ul-notes-list">
                    {taskNotes &&
                        taskNotes.length > 0 &&
                        taskNotes.map((note) => {
                            const noteRender = formatURL(note.text);

                            let createdDate = note.dateCreated;
                            if (createdDate != undefined) {
                                const arr = note.dateCreated.split(".");

                                if (arr.length == 2) {
                                    createdDate = arr[0] + "Z";
                                }
                            }

                            return (
                                <li key={note.id}>
                                    <div
                                        style={{
                                            display: "flex",
                                            justifyContent: "space-between",
                                        }}
                                        className="notes-pane__note-item"
                                    >
                                        <div className="notes-pane__note-item-info text">
                                            <span
                                                    className="task-note-text"
                                                    dangerouslySetInnerHTML={{
                                                __html: noteRender,
                                            }}
                                                />
                                            </div>
                                        <div className="notes-pane__note-item-info text">
                                            <TimeAgo
                                                date={moment(createdDate)}
                                            />
                                        </div>
                                        <div className="notes-pane__note-item-actions">
                                            <Inline>
                                                {!isFormulaTask && (
                                                    <ModusButton
                                                        circular
                                                        type="button"
                                                        icon="rss"
                                                        className={
                                                            note.isPublished
                                                                ? "publish publishCss"
                                                                : "published publishCss"
                                                        }
                                                        disabled={
                                                            note.isPublished
                                                                ? true
                                                                : false
                                                        }
                                                        onClick={() =>
                                                            this.props.onPublishNotes(
                                                                note.id,
                                                                !note.isPublished
                                                            )
                                                        }
                                                        popup="Publish to all project task."
                                                    />
                                                )}
                                                <ModusButton
                                                    circular
                                                    type="button"
                                                    icon="iauto--remove-white"
                                                    className="delete"
                                                    onClick={() =>
                                                        this.props.onDeleteNotes(
                                                            note.id
                                                        )
                                                    }
                                                />
                                            </Inline>
                                        </div>
                                    </div>
                                </li>
                            );
                        })}
                </ul>
            );
        } else {
            return (<p style={{ margin: "10px" }}>Notes not added</p>);
        }
    }

    render() {

        //console.log("isEditedNote", this.props.isEdited);
        const toDownstream = !this.props.isFormulaTask? {
            key: 2,
            text: 'To Downstream',
            icon: <Icon name="arrow down" style={{color:"rgb(195, 146, 46)"}} size="large" />,
            onClick: () => this.handleShareToDownstream(),
        }: <div></div>

        const withFormula = !this.props.isFormulaTask? {key: 1,
            text: 'With Formula',
            icon: <Icon   style={{color:"rgb(195, 146, 46)",background:
                        "url(/images/icons-sprite.svg) no-repeat",backgroundSize: "112px 494px",
                        backgroundPosition:
                            "-87px -251px",
                        height: "20px",
                        width: "19px",marginLeft: "5px"}} size="large" />,
            onClick: () => this.handleShareWithFormula()}
            : {key: 1,
            text: 'With Formula',
            icon: <Icon   style={{color:"rgb(195, 146, 46)",background:
                        "url(/images/icons-sprite.svg) no-repeat",backgroundSize: "112px 494px",
                        backgroundPosition:
                            "-87px -251px",
                        height: "20px",
                        width: "19px",marginLeft: "5px"}} size="large" />,
            onClick: () => this.handleNewAddNotes()}//replace handleShareWithFormula()  with handleNewAddNotes() 5jan AT

        const options = [
            withFormula,

            toDownstream
        ];

        return (
            <Fragment>
                <div className="notes-section">
                    <textarea
                        type="text"
                        className="notes-input-text"
                        value={this.state.noteText}
                        onChange={this.handleChange}
                    />
                    {/* <Button
                        className="ui button iauto purple button-flex-order1 addTodoBtn"
                        disabledAdd={!this.state.noteText}
                        onClick={this.handleNewAddNotes}
                    >
                        Add
                    </Button> */}
                    <div>
                        <Dropdown
                            simple
                            labeled
                            button
                            text="Share"
                            icon="share alternate"
                            className="icon"
                            style={{ 
                                color: '#fefeff', 
                                background:  "#5d2684", //this.props.isEdited ? "#988f9f" :
                                fontWeight: 'normal',
                                textTransform:'uppercase',
                                fontSize:".8em",
                                padding:"15px"
                            }}
                            options={options}
                        />
                    </div>
                </div>
                <div>
                    {this.renderNotes()}
                </div>
            </Fragment>
        );
    }
}
