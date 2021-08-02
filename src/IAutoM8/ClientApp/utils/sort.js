import moment from 'moment'

export function toggleDirection(direction) {
    return direction === 'ascending' ? 'descending' : 'ascending';
}

export function formulaAccessor(field) {
    switch (field) {
        case 'name':
            return f => f.name.toLowerCase();
        case 'description':
            return f => f.description !== null ? f.description.toLowerCase() : null;
        case 'userName':
            return f => f.owner.userName.toLowerCase();
        case 'dateCreated':
            return f => moment(f.dateCreated).valueOf();
        case 'lastUpdated':
            return f => f.lastUpdated ? moment(f.lastUpdated).valueOf() : 0;
    }
    return null;
}

export function projectAccessor(field) {
    switch (field) {
        case 'name':
            return p => p.name.toLowerCase();
        case 'description':
            return p => p.description !== null ? p.description.toLowerCase() : null;
        case 'client':
            return p => p.client !== null ? p.client.toLowerCase() : null;
        case 'managers':
            return p => p.managers.join(' ').toLowerCase();
        case 'dateCreated':
            return p => moment(p.dateCreated).valueOf();
        case 'lastUpdated':
            return p => p.lastUpdated !== null ? moment(p.lastUpdated).valueOf() : 0;
    }
    return null;
}

export function skillAccessor(field) {
    switch (field) {
        case 'name':
            return t => t.name.toLowerCase();
        case 'teamName':
            return t => t.teamName != null ? t.teamName.toLowerCase() : "";
        case 'users':
            return t => t.users.join(' ').toLowerCase();
        case 'dateCreated':
            return t => moment(t.dateCreated).valueOf();
        case 'lastUpdated':
            return t => t.lastUpdated !== null ? moment(t.lastUpdated).valueOf() : 0;
    }
    return null;
}

export function clientAccessor(field) {
    switch (field) {
        case 'companyName':
            return c => c.companyName.toLowerCase();
        case 'address':
            return c => `${c.streetAddress1} ${c.city} ${c.state} ${c.zip} ${c.country}`.toLowerCase();
        case 'representative':
            return c => `${c.firstName} ${c.lastName},${c.email}`.toLowerCase();
        case 'dateCreated':
            return c => moment(c.dateCreated).valueOf();
        case 'lastUpdated':
            return c => c.lastUpdated !== null ? moment(c.lastUpdated).valueOf() : 0;
    }
    return null;
}

export function userAccessor(field) {
    switch (field) {
        case 'user':
            return c => c.fullName.toLowerCase();
        case 'email':
            return c => c.email.toLowerCase();
        case 'role':
            return c => c.roles[0];
    }
    return null;
}
