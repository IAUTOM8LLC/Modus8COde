import moment from 'moment'

export default function (task) {
    if (!task) {
        return false;
    }
    const overdueStatus = ['inprogress', 'new'];
    const { status = '', dueDate = '' } = task;

    return overdueStatus.includes(status.toLowerCase()) && moment(dueDate).isBefore(moment());
}