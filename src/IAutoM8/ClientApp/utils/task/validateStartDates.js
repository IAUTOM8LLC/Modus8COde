import moment from 'moment'

export default function (tasks, sourceId, targetId) {
    const parent = tasks.find(t => t.id === Number(sourceId));
    const child = tasks.find(t => t.id === Number(targetId));
    const invalidOptions = [];

    if (child.isUpcoming && moment(child.startDate).isBefore(parent.startDate)) {
        invalidOptions.push(child.title);
    }
    return invalidOptions;
}
