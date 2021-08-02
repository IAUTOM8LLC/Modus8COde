export default function (s = '') {
    return s
        // catch groups of lower case and upper case letters
        // and split them with hyphen
        .replace(/([a-z])([A-Z])/g, '$1-$2')
        // and put all to lower case
        .toLowerCase();
}