export default function createDeffered() {
    /* eslint-disable prefer-const */
    let resolve, reject, p = new Promise((res, rej) => [resolve, reject] = [res, rej]);
    return Object.assign(p, { resolve, reject });
    /* eslint-enable prefer-const */
}