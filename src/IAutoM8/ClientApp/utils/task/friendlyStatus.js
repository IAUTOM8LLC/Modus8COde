export default function (status) {
    switch (status) {
        case 'InProgress':
            return 'To Do';
        case 'Completed':
            return 'Completed';
        case 'NeedsReview':
            return 'Needs review';
        case 'New':
            return 'Upcoming';
        default:
            return status;
    }
}
