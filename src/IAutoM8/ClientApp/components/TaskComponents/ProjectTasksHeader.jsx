import React from 'react'
import { Segment, Button, Divider, Dropdown } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

import { ALL_PROJECTS_ID } from '@constants/projectCostants'

import {
    ModusButton, ModusIcon, ProjectHeader, Inline, WhenPermitted,
    SkillFilter, UserFilter, ProjectSelector
} from '@components'

import './styles/ProjectTasksHeader.less'

export default function ProjectTasksHeader({
    children,
    name,
    projectId,
    path,
    taskCount = 0,
    overdueCount = 0,
    onAddCard,
    onImportFormula,
    onShowResources,
    params,
    onGoToBoard,
    onGoToListView
}) {

    const isEditor = path.includes('editor');
    const isListView = path.includes('listView');
    const isCalendar = path.includes('calendar');
    const isFeed = path.includes('feed');
    const isKanban = !isEditor && !isCalendar && !isFeed && !isListView;
    const shouldRedirect = params !== undefined && params.taskId === undefined;

    const feedRoutePath = () => {
        if (projectId === ALL_PROJECTS_ID) {
            return '/projects/allProjects/feed';
        }
        return `/projects/${projectId}/feed`;
    }

    const options = [
        {
            key: 1,
            text: 'One-time',
            onClick: () => onAddCard('plain'),
            icon: <ModusIcon name="task one-time" />
        },
        {
            key: 2,
            text: 'Recurring',
            onClick: () => onAddCard('recurrent'),
            icon: <ModusIcon name="task recurring" />
        },
        {
            key: 3,
            text: 'Conditional',
            onClick: () => onAddCard('conditional'),
            icon: <ModusIcon name="task conditional" />
        },
        {
            key: 4,
            text: 'Conditional-Recurring',
            onClick: () => onAddCard('conditional,recurrent'),
            icon: <ModusIcon name="task conditional-recurring" />
        },
    ];

    return (
        <div className="iauto-task-header">
            <Segment clearing basic>
                <ProjectHeader
                    name={name}
                    // tasks={taskCount}
                    // overdue={overdueCount}
                    className="iauto-task-header__left"
                    floated="left"
                />

                <Inline floated="right">
                    {children}
                    {
                        projectId !== ALL_PROJECTS_ID &&
                        <ModusButton
                            content={<div className="icon-all-files">
                                <i />
                                {/* <span>Project resources</span> */}
                                <span>Files</span>
                            </div>}
                            onClick={onShowResources}
                        />
                    }
                    {
                        (projectId !== ALL_PROJECTS_ID && (isKanban || isEditor)) &&
                        <ModusButton
                            grey
                            icon="add"
                            className="import-tasks"
                            content="Tasks from formula"
                            onClick={onImportFormula}
                            whenPermitted="importTasksFromFormula"
                        />
                    }
                    {projectId !== ALL_PROJECTS_ID && isKanban &&
                        <WhenPermitted rule="createProjectTask">
                            <Dropdown
                                simple
                                item
                                text="New task"
                                icon="add"
                                className="add-task-dropdown"
                                options={options}
                            />
                        </WhenPermitted>
                    }
                    <WhenPermitted rule="accessToProjectUiEditor">
                        <Button.Group>
                            {/* {projectId === ALL_PROJECTS_ID && 
                                <ModusButton
                                    filled={isListView}
                                    grey={!isListView}
                                    as={Link}
                                    onClick={onGoToListView ? onGoToListView : () => ''}
                                to={`/projects/getTasksFromAllProjects/listView`}
                                    content="TODAYS TO DO'S"
                                />
                            } */}
                            {/* <ModusButton
                                filled={isKanban}
                                grey={!isKanban}
                                as={Link}
                                onClick={onGoToBoard ? onGoToBoard : () => ''}
                                to={`/projects/${projectId ===
                                    ALL_PROJECTS_ID ? 'getTasksFromAllProjects' : projectId}`}
                                content="Smart board"
                            /> */}
                            {projectId !== ALL_PROJECTS_ID &&
                                <ModusButton
                                    filled={isEditor}
                                    grey={!isEditor}
                                    as={Link}
                                    to={`/projects/${projectId}/editor`}
                                    content="UI Editor"
                                />
                            }
                            {projectId !== ALL_PROJECTS_ID &&
                                <ModusButton
                                    filled={isCalendar}
                                    grey={!isCalendar}
                                    as={Link}
                                    to={`/projects/${projectId}/calendar`}
                                    content="Calendar"
                                />
                            }
                            {/* <ModusButton
                                filled={isFeed}
                                grey={!isFeed}
                                as={Link}
                                className="feed"
                                to={feedRoutePath()}
                                content="       Feed       "
                            /> */}
                        </Button.Group>
                    </WhenPermitted>
                </Inline>
            </Segment>

            <Divider />

            {isKanban &&
                <div className="iauto-task-header__task-selectors">
                    <Inline>
                    <ProjectSelector projectId={projectId} shouldRedirect={shouldRedirect} />
                    <UserFilter />
                    </Inline>
                </div>
            }
        </div>
    );
}
