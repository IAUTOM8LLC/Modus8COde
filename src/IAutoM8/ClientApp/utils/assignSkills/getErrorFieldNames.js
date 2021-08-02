/* eslint-disable max-len */

export const getErrorFieldNames = (obj, name = "") => {
    const errArray = [];

    errArray.push(Object.keys(obj).map((key) => {
                const next = obj[key];

                if (next) {
                    if (typeof next === "string") {
                        return name + key;
                    }

                    // Keep looking
                    if (next.map) {
                        errArray
                            .push(next.map((item, index) => getErrorFieldNames(item, `${name}${key}[${index}].`))
                            .filter((o) => o));
                    }
                }
                return null;
            })
            .filter((o) => o)
    );

    return flatten(errArray);
};

const flatten = (arr) => {
    return arr.reduce((flat, toFlatten) => flat.concat(Array.isArray(toFlatten) ? flatten(toFlatten) : toFlatten), []);
};