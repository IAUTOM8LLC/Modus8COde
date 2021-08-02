export const getNotificationId = (state, props) => {
    return Number(props.match.params.notificationId) || 0;
}
