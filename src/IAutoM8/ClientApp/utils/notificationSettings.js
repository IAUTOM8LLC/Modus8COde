export default function notificationSettings(type) {
    switch (Number(type)) {
        case 0:
            return 'Assign to task';
        case 1:
            return 'Task in progress';
        case 2:
            return 'Task is overdue';
        case 3:
            return 'Task deadline';
        case 4:
            return 'Task review was declined';
        case 5:
            return 'Task needs review';
        case 6:
            return 'Assign to project';
        case 7:
            return 'Daily summary';
        case 8:
            return 'Task commented';
    }
}
