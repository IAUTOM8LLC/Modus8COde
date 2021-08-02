import axios from 'axios'
import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reset } from 'redux-form'
import { push } from 'react-router-redux'
import { success, error, info } from 'react-notification-system-redux';
import sortBy from 'lodash/sortBy'
import autobind from 'autobind-decorator'

import { getAuthHeaders } from '@infrastructure/auth'
//import { getUsers } from "@selectors/users";
import { 
loadProjects, 
addProject, 
deleteProject, 
editProject, 
addChildNode,
updateChildNode,
uploadBulkProjects } from '@store/project'
import { loadClients } from '@store/client'
import { filterProjectsByQuery ,getProjectId ,getUsers } from '@selectors/project'
import { toggleDirection, projectAccessor } from '@utils/sort'
import { setFilterColumn } from '@store/layout'
import { selectSearchColumn } from '@selectors/layout'
import { setResources, updateProjectResource, clearResources ,getAllResources } from '@store/resource'

import NotesFormModal from '../../components/modals/NotesFormModal/NotesFormModal'

import ResourceModal from '../../components/modals/ResourceModal/ResourceModal'

import { Prompt, ProjectFormModal ,ProjectUploadModal} from '@components'

import ProjectTable from './components/ProjectTable'
import ProjectsHeader from './components/ProjectsHeader'

import './Projects.less'


@connect(
    (state, props) => ({
        users: getUsers(state),
        roles: state.auth.user.roles,
        ...state.project,
        projects: filterProjectsByQuery(state),
        clients: state.client.clients,
        filterBy: selectSearchColumn(state),
        resources: state.resource,
        projectId: getProjectId(state,props),
        isBulkModelForm: false
    }),
    {
        loadProjects,
        addProject,
        deleteProject,
        editProject,
        uploadBulkProjects,
        reset,
        success, error, info,
        loadClients,
        setFilterColumn,
        setResources,
        updateProjectResource,
        clearResources,
        pushState: push,
        addChildNode,
        updateChildNode,
        getAllResources
    }
)
export default class Projects extends Component {
    state = {
        NotesDetailsModalOpen: false,
        projectDetailsModalOpen: false,
        projectUploadModalOpen: false,
        projectToEdit: {
            id: 0
        },
        projectToEditNotes: {
            id: 0
        },
        ids:null,

        users: [],
        orderColumn: null,
        orderDirection: null,
        orderedProjects: [],

        taskModalOpen: false,
            vendorPopupOpen: false,
            braintreeModalOpen: false,
            commentModalOpen: false,
            pendingTask: {},
            resourceModal: false,
            outsourceTab: false,
            commentTab: false,
            currentCredits: {},
            isPublishClicked: false,
            showCompleteAndPublish: false,
            isEdited:false,
            commentTaskId: null

    }

    componentWillReceiveProps(nextProps) {
        const { orderColumn, orderDirection } = this.state;
        if (orderColumn) {
            let projects = nextProps.projects ? sortBy(nextProps.projects, projectAccessor(orderColumn)) : [];
            if (orderDirection === 'descending') {
                projects = projects.reverse();
            }
            this.setState({
                orderedProjects: projects
            });
        } else {
            this.setState({
                orderedProjects: [...nextProps.projects]
            });
        }
    }

    componentDidMount() {
        this.props.loadProjects();
        this.props.loadClients();
    }

    toggleModal = (opened) => {
        if (!opened) {
            this.props.reset('projectFormModal');
            this.setState({
                projectToEdit: null
            });
        }

        this.setState({
            projectDetailsModalOpen: opened
        });
    }
    toggleUploadModal = (opened) => {
        if (!opened) {
            this.props.reset('projectUploadModal');
            // this.setState({
            //     projectToEdit: null
            // });
        }
        this.setState({
            projectUploadModalOpen: opened
        });
    }

    toggleNotesModal = (opened) => {
        if (!opened) {
            this.props.reset('notesFormModal');
            // this.setState({
            //     projectToEditNotes: null
            // });
        }

        this.setState({
            NotesDetailsModalOpen: opened
        });
    }

    handleShowResources = (id) => {
        this.setState({
            // ...this.state,
            resourceModal: true,
            ids:id
        });
    };

    handleNotesModal = (projectId) => {
        const getProject = axios.get(`api/projects/${projectId}`, getAuthHeaders());
        const getUsers = axios.get(`api/projects/${projectId}/users`, getAuthHeaders());
        axios.all([getProject, getUsers])
            .then(axios.spread(({ data: project }, { data: users }) => {
                this.openNotesModal({
                    ...project,
                    managers: project.managers.map(m => m.userId)
                }, users);
                
                this.props.clearResources();
                this.props.setResources(project.resources);
            }));
            
    }

    handleNotesModalClose = () => {
        this.toggleNotesModal(false);
    };

    handleNotesSubmit = ( project) => {
        const performAction = project.id
            ? this.props.editProject
            : this.props.addProject;

        return performAction({
            ...project,
            managers: (project.managers || []).map(id => ({
                userId: id
            }))
        }).then(({ action }) => {
            const id = action.payload.data.id;
            this.props.updateProjectResource(id, this.props.resources);
            this.toggleNotesModal(false);
            if (!project.id) {
                this.props.pushState(`/projects/${id}`);
            }
        });

    }

    handleCloseResources = () => {
        this.setState({
            // ...this.state,
            resourceModal: false,
            ids:null
        });
    };

    handleSubmit = (project) => {
        const performAction = project.id
            ? this.props.editProject
            : this.props.addProject;

        return performAction({
            ...project,
            managers: (project.managers || []).map(id => ({
                userId: id
            }))
        }).then(({ action }) => {
            const id = action.payload.data.id;
            this.props.updateProjectResource(id, this.props.resources);
            this.toggleModal(false);
            if (!project.id) {
                this.props.pushState(`/projects/${id}`);
            }
        });
    }

    @autobind
    async handleDeleteProject(projectId) {
        const confirmed = await Prompt.confirm(
            `Do you want to delete project ${this.props.projects.find(x => x.id === projectId).name}?`,
            'Confirm delete project',
            'list layout'
        );

        if (confirmed) {
            this.props.deleteProject(projectId)
                .then(() => this.props.success({ title: 'Project was deleted successfully' }))
                .catch(() => this.props.error({ title: 'Cannot delete project' }));
        }
    }

    openProjectModal = (project, users) => {
        this.setState({
            projectToEdit: project,
            users: users.map(u => ({
                key: u.id,
                value: u.id,
                text: `${u.fullName} (${u.email})`
            }))
        });
        this.toggleModal(true);
    }
    openNotesModal = (project,users ) => {
        this.setState({
            projectToEditNotes: project,
            users: users.map(u => ({
                key: u.id,
                value: u.id,
                text: `${u.fullName} (${u.email})`
            }))
        });
        this.toggleNotesModal(true);
    }

    handleEdit = (projectId) => {
        const getProject = axios.get(`api/projects/${projectId}`, getAuthHeaders());
        const getUsers = axios.get(`api/projects/${projectId}/users`, getAuthHeaders());
        axios.all([getProject, getUsers])
            .then(axios.spread(({ data: project }, { data: users }) => {
                this.openProjectModal({
                    ...project,
                    managers: project.managers.map(m => m.userId)
                }, users);
                this.props.clearResources();
                this.props.setResources(project.resources);
            }));
    }

    handleAddProjectClick = () => {
        this.props.clearResources();
        axios.get(`api/projects/${0}/users`, getAuthHeaders())
            .then(({ data: users }) => this.openProjectModal({}, users));
    }
    onUploadBulkProject = () => {
        this.toggleUploadModal(true)
        // this.props.clearResources();
        this.setState({
            isBulkModelForm: true
        })
    }
    handleUploadSubmit = (file) => {
        const fileName = this.props.resources.fileResources.map(file => {
            return file.name
        })
        this.props.uploadBulkProjects(fileName.toString())
        .then(() => this.props.loadProjects())
        
        this.props.success({ title: 'Project was saved successfully' })
        this.toggleUploadModal(false)
    }

    handleProjectDetailsModalClose = () => {
        this.toggleModal(false);
    }
    handleProjectUploadModalClose = () => {
        this.toggleUploadModal(false)
    }

    handleSort = clickedColumn => () => {
        const { orderColumn, orderDirection, orderedProjects } = this.state;

        if (orderColumn !== clickedColumn) {
            this.setState({
                orderColumn: clickedColumn,
                orderedProjects: sortBy(orderedProjects, projectAccessor(clickedColumn)),
                orderDirection: 'ascending'
            });
            return;
        }

        this.setState({
            orderedProjects: orderedProjects.reverse(),
            orderDirection: toggleDirection(orderDirection)
        });
    }

    render() {
        const {
            setFilterColumn,
            filterBy,
            roles,
        } = this.props;
        const {
            orderColumn,
            orderDirection,
            orderedProjects,
        } = this.state;
        
        const isVendor = roles.includes('Vendor' );
        const role  = roles.toString()
        return (
            <div className="iauto-projects projects">
                <ProjectsHeader
                    onAddProjectClick={this.handleAddProjectClick}
                    onUploadBulkProject ={this.onUploadBulkProject}
                    filterBy={filterBy}
                    setFilterColumn={setFilterColumn}
                />

                <ProjectTable
                    projects={orderedProjects}
                    onDelete={this.handleDeleteProject}
                    onEdit={this.handleEdit}
                    orderColumn={orderColumn}
                    onSort={this.handleSort}
                    sortDirection={orderDirection}
                    onAddChildProject={this.props.addChildNode}
                    onUpdateChildProject={this.props.updateChildNode}
                    onShowResources = {this.handleShowResources}
                    handleNotesModal = {this.handleNotesModal}
                    isVendor ={isVendor}
                    role = {role}
                />

                <ProjectFormModal
                    users={this.state.users}
                    clients={this.props.clients}
                    projectToEdit={this.state.projectToEdit}
                    initialValues={this.state.projectToEdit}
                    open={this.state.projectDetailsModalOpen}
                    onClose={this.handleProjectDetailsModalClose}
                    onSubmit={this.handleSubmit}
                />
                <NotesFormModal 
                    open={this.state.NotesDetailsModalOpen}
                    initialValues={this.state.projectToEditNotes}
                    projectToEditNotes={this.state.projectToEditNotes}
                    onClose = {this.handleNotesModalClose}
                    onSubmit = {this.handleNotesSubmit}
                />
                <ProjectUploadModal 
                    isBulkModelForm ={this.state.isBulkModelForm}
                    users={this.state.users}
                    clients={this.props.clients}
                    projectToEdit={this.state.projectToEdit}
                    initialValues={this.state.projectToEdit}
                    open={this.state.projectUploadModalOpen}
                    onClose={this.handleProjectUploadModalClose}
                    onSubmit={this.handleUploadSubmit}
                />
                {this.state.ids && <ResourceModal
                    projectId={this.state.ids}
                    open={this.state.resourceModal}
                    onClose={this.handleCloseResources}
                />}
                
            </div>
        );
    }
}
