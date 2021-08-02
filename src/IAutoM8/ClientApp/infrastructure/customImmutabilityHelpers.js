import update from 'immutability-helper'

update.extend('$mergeArrayById', function (toMerge, original) {
    return toMerge.reduce((result, entry) => {
        const index = original.findIndex(e => e.id === entry.id);
        return index === -1
            ? update(result, { $push: [entry] })
            : update(result, { [index]: { $set: entry } });
    }, original);
});