import axios from 'axios'
import React, { Component } from 'react'
import { connect } from 'react-redux'
import { reset } from 'redux-form'
import { success, error, info } from 'react-notification-system-redux';
import sortBy from 'lodash/sortBy'
import autobind from 'autobind-decorator'

import { getAuthHeaders } from '@infrastructure/auth'
import { loadClients, addClient, deleteClient, editClient } from '@store/client'
import { filterClientsByQuery } from '@selectors/client'
import { toggleDirection, clientAccessor } from '@utils/sort'
import { setFilterColumn } from '@store/layout'
import { selectSearchColumn } from '@selectors/layout'

import { Prompt, ClientFormModal } from '@components'

import ClientsTable from './components/ClientsTable'
import ClientsHeader from './components/ClientsHeader'

@connect(
    state => ({
        ...state.client,
        clients: filterClientsByQuery(state),
        filterBy: selectSearchColumn(state)
    }),
    {
        loadClients,
        addClient,
        deleteClient,
        editClient,
        reset,
        success, error, info,
        setFilterColumn
    }
)
export default class Clients extends Component {
    state = {
        orderColumn: null,
        orderDirection: null,
        orderedClients: []
    }

    componentWillReceiveProps(nextProps) {
        const { orderColumn, orderDirection } = this.state;
        if (orderColumn) {
            let clients = nextProps.clients ? sortBy(nextProps.clients, clientAccessor(orderColumn)) : [];
            if (orderDirection === 'descending') {
                clients = clients.reverse();
            }
            this.setState({
                orderedClients: clients
            });
        } else {
            this.setState({
                orderedClients: [...nextProps.clients]
            });
        }
    }

    componentDidMount() {
        this.props.loadClients();
    }

    handleEditClient = (clientId) => {
        axios.get(`api/client/${clientId}`, getAuthHeaders())
            .then((response) => {
                this.clientFormModal.show({ initialValues: response.data })
                    .then((client) => {
                        this.props.editClient(client)
                            .then(() => this.props.success({
                                title: 'Client was updated successfully'
                            })).catch(() => this.props.error({
                                title: 'Cannot update client'
                            }));
                        this.props.reset('clientFormModal');
                    }).catch(() => {
                        this.props.reset('clientFormModal');
                    });
            });
    }

    handleAddClient = () => {
        this.clientFormModal.show({})
            .then((client) => {
                this.props.addClient(client)
                    .then(() => this.props.success({
                        title: 'Client was successfully added'
                    })).catch(() => this.props.error({
                        title: 'Cannot add new client'
                    }));
                this.props.reset('clientFormModal');
            }).catch(() => {
                this.props.reset('clientFormModal');
            });
    }

    @autobind
    async handleDeleteClient(clientId) {
        const confirmed = await Prompt.confirm(
            `Do you want to delete client ${this.props.clients.find(x => x.id === clientId).name}?`,
            'Confirm delete client',
            'user circle outline'
        );

        if (confirmed) {
            this.props.deleteClient(clientId)
                .then(() => this.props.success({ title: 'Client was deleted successfully' }))
                .catch(() => this.props.error({ title: 'Cannot delete client' }));
        }
    }

    handleSort = clickedColumn => () => {
        const { orderColumn, orderDirection, orderedClients } = this.state;

        if (orderColumn !== clickedColumn) {
            this.setState({
                orderColumn: clickedColumn,
                orderedClients: sortBy(orderedClients, clientAccessor(clickedColumn)),
                orderDirection: 'ascending'
            });
            return;
        }

        this.setState({
            orderedClients: orderedClients.reverse(),
            orderDirection: toggleDirection(orderDirection)
        });
    }

    render() {
        const {
            setFilterColumn,
            filterBy
        } = this.props;
        const {
            orderColumn,
            orderDirection,
            orderedClients
        } = this.state;

        return (
            <div className="iauto-projects">
                <ClientsHeader
                    onAddClientClick={this.handleAddClient}
                    filterBy={filterBy}
                    setFilterColumn={setFilterColumn}
                />

                <ClientsTable
                    clients={orderedClients}
                    onDelete={this.handleDeleteClient}
                    onEdit={this.handleEditClient}
                    orderColumn={orderColumn}
                    onSort={this.handleSort}
                    sortDirection={orderDirection}
                />

                <ClientFormModal
                    ref={(c) => { this.clientFormModal = c; }}
                    loading={this.props.loading}
                />
            </div>
        );
    }
}
