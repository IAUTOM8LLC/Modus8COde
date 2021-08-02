export default function (task) {
    return !task.id || task.isUpcoming;
}
