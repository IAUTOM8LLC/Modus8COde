import isOverdue from './isOverdue'

export default function mapStatus(task) {
    const { status } = task;
    return {
        ...task,
        isInProgress: status === 'InProgress',
        isCompleted: status === 'Completed',
        needsReview: status === 'NeedsReview',
        isUpcoming: status === 'New',

        isOverdue: isOverdue(task)
    };
}
