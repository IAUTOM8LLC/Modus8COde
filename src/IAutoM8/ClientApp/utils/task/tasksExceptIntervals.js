export default function (tasks = []) {
    return tasks.filter(t => !t.isInterval);
}