export default function notificationIconStyle(type) {
    switch (type) {
        case 5:
            return 'notification-need-review';
        case 8:
            return 'notification-comment';
        case 9:
            return 'notification-formula-task';
        case 0:
        case 10:
            return 'notification-project-task';
        case 11:
            return 'notification-accepted-by-other';
        case 12:
            return 'notification-completed';
        case 13:
            return 'notification-accepted';
        case 14:
            return 'notification-start-work';
        case 15:
            return 'notification-need-vendor';
        case 16:
            return 'notification-accept-money';
    }
}
