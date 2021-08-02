export default function (errors) {
    let tabs = [];
    for (const property in errors) {
        if (errors.hasOwnProperty(property) && errors[property]) {
            tabs = tabs.concat(tabsToFieldsMap[property]);
        }
    }
    return [...new Set(tabs)];
}
const defaultMessage = 'There are invalid fields on the tab'
const tabsToFieldsMap = {
    title: [{ pane: 'information', message: defaultMessage }],

    startDate: [{ pane: 'schedule', message: defaultMessage }],
    dueDate: [{ pane: 'schedule', message: defaultMessage }],
    duration: [
        { pane: 'schedule', message: defaultMessage },
        { pane: 'recurrence', message: defaultMessage }
    ],

    condition: [{ pane: 'conditional', message: defaultMessage }],

    assignedSkillId: [{ pane: 'members', message: defaultMessage }],
    assignedUserIds: [{ pane: 'members', message: defaultMessage }],

    reviewingSkillId: [{ pane: 'members', message: 'Required someone to review' }],
    reviewingUserIds: [{ pane: 'members', message: 'Required someone to review' }]
}
