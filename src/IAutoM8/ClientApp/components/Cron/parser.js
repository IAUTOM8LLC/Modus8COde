import moment from 'moment'

import { getDefaultCronModel, days as daysData } from './data'

export function parseCron(cronModel, cronTab, useSeconds = false) {
    const modelName = ctorTabToModel(cronTab);

    const model = cronModel[modelName];
    const parser = parsers[modelName];
    const data = parser(model);
    return {
        ...data,
        cron: useSeconds ? data.cron : data.cron.substring(2),
        dayDiff: data.isAsap ? 0 : data.dayDiff
    }
}

export function convertCron(cron, cronTab, useSeconds = false, isAsap = false) {
    if (!cron)
        return getDefaultCronModel();

    if (!useSeconds)
        cron = `0 ${cron}`;

    const cronModel = getDefaultCronModel();
    const expression = cronToExpression(cron);
    const modelName = ctorTabToModel(cronTab);

    const model = cronModel[modelName];
    const converter = converters[modelName];
    converter(model, expression, cron, isAsap);

    return cronModel;
}


const parsers = {
    //this method should be sync with ~\projecttasks\taskimportservice.cs\RecalculateCron
    minutes: function ({ minutes = 1, startFrom = 0 }) {
        return {
            cron: `0 ${startFrom}/${minutes} * 1/1 * ? *`,
            dayDiff: 0,
            isAsap: false
        }
    },
    //this method should be sync with ~\projecttasks\taskimportservice.cs\RecalculateCron
    hours: function ({ selectedOption, startTime = '0:0', hours = 1, startFrom = 0, isAsap = true }) {
        const data = timeToCronUtc(startTime);
        return selectedOption === '0'
            ? {
                cron: `0 0 ${startFrom}/${hours} 1/1 * ? *`,
                dayDiff: 0,
                isAsap
            }
            : {
                cron: `0 ${data.startTime} 1/1 * ? *`,
                dayDiff: data.dayDiff,
                isAsap
            } ;
    },
    //this method should be sync with ~\projecttasks\taskimportservice.cs\RecalculateCron
    days: function ({ selectedOption, startTime = '12:00', days = 1, startFrom = 1, isAsap = true}) {
        const data = selectedOption === '0' ? dateTimeToCronUtc(startTime, startFrom) :
            timeToCronUtc(startTime);
        return selectedOption === '0'
            ? {
                cron: `0 ${data.startTime}/${days} * ? *`,
                dayDiff: data.dayDiff,
                isAsap
            }
            : {
                cron: `0 ${data.startTime} ? * SUN-SAT *`,
                dayDiff: data.dayDiff,
                isAsap
            };
    },

    weeks: function ({ startTime = '0:0', checkedDays = ['?'], isAsap = true }) {
        let utcDays = [];
        if (checkedDays.length === 0) {
            if (isAsap) {
                const cronMoment = moment();
                cronMoment.utc();
                utcDays = [isoWeekdayToWeekday(cronMoment.isoWeekday())];
            }
            else {
                utcDays = ['?'];
            }
        }
        else {
            const time = extractTimeValues(startTime);
            if (checkedDays[0] === '?') {
                utcDays = checkedDays;
            }
            else {
                utcDays = checkedDays.map(day => {
                    const cronMoment = isAsap ? moment() : moment(time);
                    cronMoment.isoWeekday(weekdayToIsoWeekday(day));
                    cronMoment.utc();
                    return isoWeekdayToWeekday(cronMoment.isoWeekday());
                });
            }

        }

        const data = timeToCronUtc(startTime);

        return {
            cron: `0 ${data.startTime} ? * ${utcDays.join(',')} *`,
            dayDiff: data.dayDiff,
            isAsap
        };
    },

    months: function ({
        selectedOption,
        startTime,
        allDay = 1,
        allMonth = 1,
        month = 1,
        day = 'MON',
        dayOrder = '1',
        isAsap = true
    }) {
        if (startTime === undefined) {
            startTime = moment().add(5, 'm').format('HH:mm');
        }
        if (selectedOption === '0') {
            const data = timeToCronUtc(startTime);
            return {
                cron: `0 ${data.startTime} ${allDay} 1/${allMonth} ? *`,
                dayDiff: data.dayDiff,
                isAsap
            };
        } else {
            const data = timeToCronUtc(startTime, day);
            const { startTime: st, day: d } = timeToCronUtc(startTime, day);
            return {
                cron: `0 ${data.startTime} ? 1/${month} ${data.day}#${dayOrder} *`,
                dayDiff: data.dayDiff,
                isAsap
            };
        }
    },

    years: function ({
        selectedOption,
        startTime,
        everyDay = '1',
        everyMonth = '1',
        whichMonth = '1',
        whichDay = 'MON',
        whichDayOrder = '1',
        isAsap = true
    }) {
        if (startTime === undefined) {
            startTime = moment().add(5, 'm').format('HH:mm');
        }
        if (selectedOption === '0') {
            const data = timeToCronUtc(startTime);
            return {
                cron: `0 ${data.startTime} ${everyDay} ${everyMonth} ? *`,
                dayDiff: data.dayDiff,
                isAsap
            };
        } else {
            const data = timeToCronUtc(startTime, whichDay);
            return {
                cron: `0 ${data.startTime} ? ${whichMonth} ${data.day}#${whichDayOrder} *`,
                dayDiff: data.dayDiff,
                isAsap
            };
        }
    }
}


const converters = {
    //this method should be sync with ~\projecttasks\taskimportservice.cs\RecalculateCron
    minutes: function (model, expression) {
        model.minutes = expression[2];
        model.startFrom = expression[1];
    },
    //this method should be sync with ~\projecttasks\taskimportservice.cs\RecalculateCron
    hours: function (model, expression, cron, isAsap) {
        if (expression.length === 6) {
            model.hours = expression[3];
            model.startFrom = expression[2];
            model.selectedOption = '0';
        } else {
            model.selectedOption = '1';
            model.isAsap = isAsap;
            if (!isAsap)
                model.startTime = cronTimeToLocal(expression);
        }
    },
    //this method should be sync with ~\projecttasks\taskimportservice.cs\RecalculateCron
    days: function (model, expression, cron, isAsap) {
        if (expression.length === 5) {
            const localDateTime = cronDateTimeToLocal(expression);
            model.days = expression[4];
            model.startFrom = localDateTime.day;
            model.selectedOption = '0';
            model.isAsap = isAsap;
            if (!isAsap)
                model.startTime = localDateTime.startTime;
        } else {
            model.selectedOption = '1';
            model.isAsap = isAsap;
            if (!isAsap)
                model.startTime = cronTimeToLocal(expression);
        }
    },

    weeks: function (model, expression, cron, isAsap) {
        let localDays = cron
            .split(' ')[5] // * * * * * MON,FRI *
            .split(',');

        if (localDays.length > 0 && localDays[0] !== '?') {
            const time = extractTimeValues(
                startTimeFromExpression(expression)
            );

            localDays = localDays.map(day => {
                const cronMoment = isAsap ? moment() : moment.utc(time);
                cronMoment.isoWeekday(weekdayToIsoWeekday(day));
                cronMoment.local();
                return isoWeekdayToWeekday(cronMoment.isoWeekday());
            });
        }

        model.checkedDays = localDays;
        model.isAsap = isAsap;
        if (!isAsap)
            model.startTime = cronTimeToLocal(expression);
    },

    months: function (model, expression, cron, isAsap) {
        if (cron.indexOf('#') !== -1) {
            const { startTime, day } = cronTimeToLocal(
                expression,
                parseDay(cron, expression)
            );

            model.day = day;
            model.dayOrder = expression[5];
            model.month = expression[4];
            model.selectedOption = '1';
            model.isAsap = isAsap;
            if (!isAsap)
                model.startTime = startTime;
        } else {
            model.allDay = expression[3];
            model.allMonth = expression[5];
            model.selectedOption = '0';
            model.isAsap = isAsap;
            if (!isAsap)
                model.startTime = cronTimeToLocal(expression);
        }
    },

    years: function (model, expression, cron, isAsap) {
        if (cron.indexOf('#') !== -1) {
            const { startTime, day } = cronTimeToLocal(
                expression,
                parseDay(cron, expression)
            );

            model.whichDay = day;
            model.whichDayOrder = expression[4];
            model.whichMonth = expression[3];
            model.selectedOption = '1';
            model.isAsap = isAsap;
            if (!isAsap)
                model.startTime = startTime;

        } else {
            model.selectedOption = '0';
            model.everyMonth = expression[4];
            model.everyDay = expression[3];
            model.isAsap = isAsap;
            if (!isAsap)
                model.startTime = cronTimeToLocal(expression);
        }
    }
}

// ----------------------------------------
// Utils
// ----------------------------------------

const cronToExpression = (cron) => cron.match(/\d+/g);
const startTimeFromExpression = (expression) => `${expression[2]}:${expression[1]}`;

const isoWeekdayToWeekday = (iso) => daysData.find(d => d.key === iso).value;
const weekdayToIsoWeekday = (ddd) => daysData.find(d => d.value === ddd).key;

function parseDay(cron) {
    const dddIndex = cron.indexOf('#');
    const ddd = cron.substring(dddIndex, dddIndex - 3);
    return weekdayToIsoWeekday(ddd);
}

function extractTimeValues(time) {
    const tokens = time.split(':');
    const hour = Number(tokens[0]);
    const minute = Number(tokens[1]);

    return { hour, minute };
}

function ctorTabToModel(cronTab) {
    switch (cronTab) {
        case 0: return 'minutes';
        case 1: return 'hours';
        case 2: return 'days';
        case 3: return 'weeks';
        case 4: return 'months';
        case 5: return 'years';
    }
}

// ----------------------------------------
// Timezone utils
// ----------------------------------------

function timeToCronUtc(time, day) {
    const cronMoment = moment(extractTimeValues(time));
    if (day) {
        cronMoment.isoWeekday(weekdayToIsoWeekday(day));
    }
    const localDay = cronMoment.date();
    cronMoment.utc();
    const startTime = `${cronMoment.minutes()} ${cronMoment.hours()}`;

    if (day) {
        return {
            startTime,
            day: isoWeekdayToWeekday(cronMoment.isoWeekday()),
            dayDiff: localDay - cronMoment.date()
        };

    } else {
        return {
            startTime,
            dayDiff: localDay - cronMoment.date()
        }
    }
}
function dateTimeToCronUtc(time, day) {
    const cronMoment = moment(extractTimeValues(time));
    cronMoment.date(day)
    cronMoment.utc();
    const startTime = `${cronMoment.minutes()} ${cronMoment.hours()} ${cronMoment.date()}`;

    return {
        startTime,
        dayDiff: day - cronMoment.date()
    }
}

function cronTimeToLocal(expression, day) {
    const time = startTimeFromExpression(expression);

    const cronMoment = moment.utc(extractTimeValues(time));
    if (day) {
        cronMoment.isoWeekday(day);
    }

    cronMoment.local();

    const startTime = `${cronMoment.hours()}:${cronMoment.minutes()}`;

    if (day) {
        return {
            startTime,
            day: isoWeekdayToWeekday(cronMoment.isoWeekday())
        };
    }

    return startTime;
}

function cronDateTimeToLocal(expression) {
    const time = startTimeFromExpression(expression);

    const cronMoment = moment.utc(extractTimeValues(time));

    cronMoment.date(expression[3]);

    cronMoment.local();

    const startTime = `${cronMoment.hours()}:${cronMoment.minutes()}`;

    return {
        startTime,
        day: cronMoment.date()
    };
}
