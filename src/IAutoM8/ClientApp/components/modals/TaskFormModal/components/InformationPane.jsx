import React, { Fragment, Component } from "react";
import { Accordion, Form, Popup, Tab, Icon } from "semantic-ui-react";
import cn from "classnames";

import { TextInput, RichEditor } from "@components";

import FilesPane from "../../CommonPanes/FilesPane";

import NotePane from "../../../../components/modals/CommonPanes/NotePane";

import AddCheckbox from "./AddCheckbox/AddCheckbox";
import LockTrainingButton from './LockTrainingButton';

import "./InformationPane.less";


export default class InformationPane extends Component {
    state = {
        activeIndex: 0,
    };

    toggleShowResources = () => {
        this.setState(({ showResources }) => ({
            showResources: !showResources,
        }));
    };

    handleClick = (e, titleProps) => {
        const { index } = titleProps;
        const { activeIndex } = this.state;
        const newIndex = activeIndex === index ? -1 : index;

        this.setState({ activeIndex: newIndex });
    };

    render() {
    
        const {
            showTrainingNotification,
            hideTrainingNotification,
            isFormulaTask,
            taskNotes,
            formulaId,
            onAddNewNotes,
            onDeleteNotes,
            onPublishNotes,
            showTrainingTab,
            taskId,
            isTrainingLocked,
            isTrainingLockIconVisible,
            isEdited,
            projectId
        } = this.props;  
        

        const onHideNotification = () => {
            if (showTrainingNotification) hideTrainingNotification();
        };

        const fileUploadContent = (
            <Fragment>
                <FilesPane isTaskModel isFormulaTask={isFormulaTask} isEdited={isEdited}/>
            </Fragment>
        );
        const notesContent = (
            <Fragment>
                <NotePane
                    isEdited={isEdited}
                    isFormulaTask={isFormulaTask}
                    taskNotes={taskNotes}
                    formulaId={formulaId}
                    taskId={taskId}
                    onAddNewNotes={onAddNewNotes}
                    onDeleteNotes={onDeleteNotes}
                    onPublishNotes={onPublishNotes}
                    projectId={projectId}
                />
            </Fragment>
        );
        const resourcePanel = [
            {
                key: "file-template",
                title: "Files Section",
                content: { content: fileUploadContent },
            },
            {
                key: "note-template",
                title: "Notes Section",
                content: { content: notesContent },
            },
        ];
        const resourceContent = (
            <Accordion.Accordion
                defaultActiveIndex={[0, 1]}
                exclusive={false}
                panels={resourcePanel}
            />
        );
        const rootPanel = [
            {
                key: "resource-template",
                title: "Resources",
                content: { content: resourceContent },
            },
        ];
        const todoContent = () => (
            <Fragment>
                <AddCheckbox
                    isFormulaTask={isFormulaTask}
                    isDisableChecklists={showTrainingNotification || false}
                />
                {/* <div className="setBorder"></div> */}
                <div className="resourceDiv field">
                    <Accordion
                        defaultActiveIndex={0}
                        panels={rootPanel}
                        fluid
                        styled
                    />
                </div>
            </Fragment>
        );

        const renderTainingTab = showTrainingTab === undefined || showTrainingTab ? ({
            menuItem: {
                key: showTrainingTab ? "training" : "",
                content: showTrainingNotification ? (
                    <Popup
                        trigger={
                            <div>
                                Training
                                    <Icon name="warning sign" color="red" />
                            </div>
                        }
                        size="mini"
                        content="Must review training before to-do list can be completed"
                    />
                ) : (
                        <div>Training</div>
                    ),
                onClick: onHideNotification,
            },
            pane: {
                key: "description",
                content: (
                    <div>
                        {isTrainingLockIconVisible && (
                            <div style={{ float: "right" }}>
                                <LockTrainingButton
                                    name="isTrainingLocked"
                                    isTrainingLocked={isTrainingLocked}
                                    taskId={taskId}
                                />
                            </div>
                        )}
                        <RichEditor
                            label="Description"
                            name="description"
                            placeholder="Type description"
                        />
                    </div>
                ),
            },
        }) : "";

        const panes = [
            {
                menuItem: { key: "todo", content: "To do" },
                pane: {
                    key: "checklist",
                    content: todoContent(),
                },
            },
            renderTainingTab,
        ].filter((pane) => pane);

        return (
            <div className="task-details-modal__pane">
                <Form as="section">
                    <TextInput required name="title" label="Task name" />
                    <TextInput
                        additionalClass="sub-header"
                        type="label"
                        name="formulaName"
                        label="From formula"
                    />
                    <div className="tabDiv">
                        <Tab
                            panes={panes}
                            renderActiveOnly={false}
                            menu={{ secondary: true, pointing: true }}
                        />
                    </div>
                </Form>
            </div>
        );
    }
}
