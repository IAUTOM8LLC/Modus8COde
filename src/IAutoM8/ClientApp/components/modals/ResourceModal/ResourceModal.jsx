import React, { Component } from 'react';
import { Modal } from 'semantic-ui-react';
import axios from 'axios';

import { getAuthHeaders } from '@infrastructure/auth';

import { ModusButton } from '@components';

import { ALL_PROJECTS_ID } from '@constants/projectCostants';

import PublishedResourcesPane from './components/PublishedResourcesPane';

import './ResourceModal.less'

export default class ResourceModal extends Component {
    state = {
        publishedResources: [],
    };

    componentDidMount() {
        this.loadProjectResources();
    }

    componentDidUpdate(prevProps) {
        if (prevProps.open !== this.props.open) {
            this.loadProjectResources();
        }
    }

    loadProjectResources = () => {
        if (this.props.projectId !== ALL_PROJECTS_ID) {
            const getProjectResources = async () => {
                const { data } = await axios
                    .get(`/api/tasks/projects/resources/${this.props.projectId}`, getAuthHeaders());

                this.setState({ publishedResources: data });
            };
    
            getProjectResources();
        }
    };

    render() {
        const {
            open,
            onClose
        } = this.props;

        if (!open)
            return null;

        // const panes = [
        //     {
        //         menuItem: { key: 'files', content: 'Files' },
        //         // pane: { key: 'files', content: <FilesPane resourceList /> }
        //         pane: { key: 'files', content: <PublishedResourcesPane /> }
        //     },
        //     {
        //         menuItem: { key: 'links', content: 'Links' },
        //         pane: { key: 'links', content: <LinksPane resourceList /> }
        //     }
        // ];

        // const res = this.state.publishedResources.reduce((accumulator, current) => {
        //     (accumulator[current.formulaName] = accumulator[current.formulaName] || []).push(current);
        //     return accumulator;
        // }, {});

        const publishedResources = [];
        this.state.publishedResources.map(i => {
            const index = publishedResources
                .findIndex(y => y.formulaName === i.formulaName);

            if (index === -1 && i.formulaName) {
                const resources = i.resources.map(e => { 
                    return {
                        id: e.id, 
                        title: e.name,
                        url: e.url
                    } 
                });
                const notes = i.notes.map(e => { 
                    return { 
                        id: e.id,
                        title: e.text 
                    }
                });

                publishedResources.push({ 
                    formulaName: i.formulaName, 
                    resources: resources,
                    notes: notes,
                    completedDate: i.completedDate,
                });
            }
        });

        return (
            <Modal
                open={open}
                size="large"
                className="task-details-modal tab-modal"
                onClose={onClose}
            >
                <Modal.Header>Files</Modal.Header>

                <Modal.Content>
                <PublishedResourcesPane 
                            publishedResources={publishedResources} />
                </Modal.Content>

                <Modal.Actions>
                    <ModusButton grey type="button" content="Cancel" onClick={onClose} />
                </Modal.Actions>
            </Modal>
        );
    }
}
