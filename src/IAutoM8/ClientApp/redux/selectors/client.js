import { createSelector } from 'reselect'

import { clientAccessor } from '@utils/sort'

import { selectSearchQueryTrimLowerCase, selectSearchColumn } from './layout'

const getClients = (state) => state.client.clients;

export const filterClientsByQuery = createSelector(
    [selectSearchQueryTrimLowerCase, selectSearchColumn, getClients],
    (query, column, clients) => {
        if (query) {
            const accessor = clientAccessor(column);
            return clients.filter(p =>
                accessor
                && accessor(p)
                && accessor(p).includes(query)
            ) || [];
        }

        return clients;
    }
);
