/* eslint-disable prefer-template */
import moment from 'moment'

const format = 'YYYY-MM-DDTHH:mm:ss';

export function getUTCFromCustomFormat(date) {
    return date.utc().format(format) + 'Z';
}

export function formatUTCTime(date) {
    const prop = date.substr(-1) === 'Z' ? date : date + 'Z';
    return moment(prop).utc().format(format) + 'Z';
}

export function getTimeZoneOffset() {
    return moment().utcOffset();
}
