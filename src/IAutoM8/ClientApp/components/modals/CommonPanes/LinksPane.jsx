import React, { Component } from 'react'
import { connect } from 'react-redux'
import { Input, List, Dropdown } from 'semantic-ui-react'
import ReactPlayer from 'react-player'
import cn from 'classnames'

import { addLink, deleteLink, shareVideo, shareGlobalVideo } from '@store/resource'
import { selectVideoResources } from '@selectors/resource'

import { ModusButton, ModusIcon, Inline } from '@components'

import './LinksPane.less'

@connect(
    (state) => ({
        ...state.resource,
        videoResources: selectVideoResources(state),
        userPermissions: state.auth.permissions
    }),
    {
        addLink,
        deleteLink,
        shareVideo,
        shareGlobalVideo
    }
)
export default class LinksPane extends Component {
    state = {
        url: ''
    }

    handleChange = (event) => {
        this.setState({ url: event.target.value });
    }

    handlePush = () => {
        const { url = '' } = this.state;

        if (url.trim() === '') {
            return;
        }

        this.props.addLink(url);

        this.setState({ url: '' });
    }

    render() {
        const { isTaskModal, userPermissions, resourceList, isVendor } = this.props;

        const havePermissions = (res) => {
            return res.isSharedFromParent || (!userPermissions.editProjectTask && res.id !== null);
        }
        const shareActions = (res) => {
            if (havePermissions(res))
                return <ModusButton
                    circular
                    type="button"
                    icon="iauto--share"
                    className="unactive"
                    popup="Can't share shared resource"
                />;

            const options = [
                {
                    key: 1,
                    text: res.isShared ? 'Unshare with children' : 'Share with children',
                    icon: <ModusIcon name="iauto--share-children" />,
                    onClick: () => this.props.shareVideo(res.localId)
                },
                {
                    key: 2,
                    text: res.isGlobalShared ? 'Unshare globally' : 'Share globally',
                    icon: <ModusIcon name="iauto--share-globally" />,
                    onClick: () => this.props.shareGlobalVideo(res.localId)
                }
            ];

            const trigger = <ModusButton
                circular
                type="button"
                icon="iauto--share"
                className="share"
            />;

            return <Dropdown
                item
                simple
                icon={null}
                inline
                className="share-actions-dropdown"
                trigger={trigger}
                options={options}
            />
        };

        return (
            <div className="task-details-modal__pane">
                <div className="video-links">
                    {!resourceList && !isVendor &&
                        <Inline className="video-links__input">
                            <Input
                                fluid
                                placeholder="Past video link here"
                                value={this.state.url}
                                onChange={this.handleChange}
                            />
                            <ModusButton
                                wide
                                filled
                                type="button"
                                content="Add link"
                                onClick={this.handlePush}
                            />
                        </Inline>
                    }

                    {
                        this.props.videoResources.length > 0 &&
                        <List className="video-links__items">
                            {
                                this.props.videoResources.map((resource, index) => (
                                    <List.Item key={index} className="video-links__item">
                                        <ReactPlayer
                                            controls
                                            style={{
                                                display: 'inline-block',
                                                float: 'left'
                                            }}
                                            width={380}
                                            height={214}
                                            url={resource.url}
                                        />

                                        {
                                            resourceList &&
                                            <div className="task-inherit">
                                                <div className="icon-task-conteiner">
                                                </div>
                                                <span>used in tasks: {resource.taskNames.length}</span>
                                                <Dropdown
                                                    scrolling
                                                    icon={<div className="icon-task-list"></div>}
                                                    options={resource.taskNames.map((m, i) => {
                                                        return {
                                                            key: i,
                                                            text: m
                                                        }
                                                    })}>
                                                </Dropdown>
                                            </div>
                                        }
                                        <div className="share-actions">
                                            {isTaskModal && !isVendor
                                                && !resourceList && shareActions(resource)}
                                        </div>
                                        {
                                            !resourceList && !isVendor &&
                                            <ModusButton
                                                circular
                                                type="button"
                                                icon="iauto--remove-white"
                                                className={cn(
                                                    'delete',
                                                    isTaskModal && havePermissions(resource) && 'unactive'
                                                )}
                                                onClick={isTaskModal && havePermissions(resource)
                                                    ? null
                                                    : () => this.props.deleteLink(resource.localId)
                                                }
                                                popup={isTaskModal && havePermissions(resource)
                                                    ? "Can't delete shared link"
                                                    : "Delete"
                                                }
                                            />
                                        }
                                    </List.Item>
                                ))
                            }
                        </List>
                    }
                </div>
            </div>
        );
    }
}
