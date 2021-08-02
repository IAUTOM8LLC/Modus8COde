export const selectSearchColumn = ({ layout }) => layout.filterBy;

export const selectSearchQuery = ({ layout }) => {
    const { searchQuery = '' } = layout;
    return searchQuery;
}

export const selectSearchQueryTrimLowerCase = (state) =>
    selectSearchQuery(state).trim().toLowerCase();