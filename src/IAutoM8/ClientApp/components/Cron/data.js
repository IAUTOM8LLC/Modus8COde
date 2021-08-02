export const getDefaultCronModel = () => ({
    minutes: {
        minutes: 1,
        startFrom: 0,
        isAsap: false
    },
    hours: {
        selectedOption: '0',
        hours: 1,
        startFrom: 0,
        isAsap: false
    },
    days: {
        selectedOption: '0',
        days: 1,
        startFrom: 1,
        isAsap: true
    },
    weeks: {
        selectedOption: '0',
        checkedDays: [],
        isAsap: true
    },
    months: {
        selectedOption: '0',
        allDay: 1,
        allMonth: 1,
        month: 1,
        day: 'MON',
        dayOrder: '1',
        isAsap: true
    },
    years: {
        selectedOption: '0',
        everyDay: 1,
        everyMonth: '1',
        whichDay: 'MON',
        whichMonth: '1',
        whichDayOrder: '1',
        isAsap: true
    }
});

export const months = [
    { key: 1, text: 'January', value: '1' },
    { key: 2, text: 'February', value: '2' },
    { key: 3, text: 'March', value: '3' },
    { key: 4, text: 'April', value: '4' },
    { key: 5, text: 'May', value: '5' },
    { key: 6, text: 'June', value: '6' },
    { key: 7, text: 'July', value: '7' },
    { key: 8, text: 'August', value: '8' },
    { key: 9, text: 'September', value: '9' },
    { key: 10, text: 'October', value: '10' },
    { key: 11, text: 'Novermber', value: '11' },
    { key: 12, text: 'December', value: '12' }
];

export const daysOrder = [
    { key: 1, text: 'First', value: '1' },
    { key: 2, text: 'Second', value: '2' },
    { key: 3, text: 'Third', value: '3' },
    { key: 4, text: 'Fourth', value: '4' }
];

export const days = [
    { key: 1, text: 'Monday', value: 'MON' },
    { key: 2, text: 'Tuesday', value: 'TUE' },
    { key: 3, text: 'Wednesday', value: 'WED' },
    { key: 4, text: 'Thursday', value: 'THU' },
    { key: 5, text: 'Friday', value: 'FRI' },
    { key: 6, text: 'Saturday', value: 'SAT' },
    { key: 7, text: 'Sunday', value: 'SUN' }
];

export const minutes = [...Array(60).keys()].map((v) => ({
    key: v,
    value: v.toString(),
    text: v < 10 ? `0${v}` : v
}));

export const hours = [...Array(24).keys()].map((v) => ({
    key: v,
    value: v.toString(),
    text: v < 10 ? `0${v}` : v
}));

export const stMinutes = [...Array(4).keys()].map((v) => ({
    key: v * 15,
    value: (v * 15).toString(),
    text: (v * 15) < 10 ? `0${v * 15}` : (v * 15)
}));

export function startTimeOptions (startTime) {
    const data = [...Array(stMinutes.length * hours.length).keys()].map((v, i) => {
        const hour = hours[Math.trunc(i / stMinutes.length)];
        const minute = stMinutes[i % stMinutes.length];

        const amHour = (hour.value % 12) === 0 ? 12 : hour.value % 12;
        const am = hour.value < 12 ? 'AM' : 'PM';

        return {
            key: `${hour.text}${minute.text}`,
            value: `${hour.value}:${minute.value}`,
            text: `${amHour}:${minute.text}${am}`
        }
    });
    if (startTime) {
        const tokens = startTime.split(':');
        const minutes = tokens[1] % 15;
        if (minutes !== 0) {
            const hourValue = tokens[0] * 1 < 10 ? `0${tokens[0]}` : tokens[0];
            const minuteValue = tokens[1] * 1 < 10 ? `0${tokens[1]}` : tokens[1];
            const amHour = (tokens[0] % 12) === 0 ? 12 : tokens[0] % 12;
            const am = tokens[0] * 1 < 12 ? 'AM' : 'PM';
            const minuteBound = tokens[1] * 1 - minutes < 10 ? '00' : tokens[1];
            data.splice(0, 0, {
                key: `${hourValue}${minuteValue}`,
                value: `${tokens[0]}:${tokens[1]}`,
                text: `${amHour}:${minuteValue}${am}`
            })
        }
    }
    return data;
}
