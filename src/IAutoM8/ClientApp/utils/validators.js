export const required = value => value
    ? undefined
    : 'Required'

export const maxLength = max => value => value && value.length > max
    ? `Must be ${max} characters or less`
    : undefined

export const minLength = min => value => value && value.length < min
    ? `Must be ${min} characters or more`
    : undefined

export const number = value => value && isNaN(Number(value))
    ? 'Must be a number'
    : undefined

export const minValue = min => value => value < min
    ? `Must be at least ${min}`
    : undefined

export const email = value => value && !/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(value)
    ? 'Invalid email address'
    : undefined

export const tooOld = value => value && value > 65
    ? 'You might be too old for this'
    : undefined

export const aol = value => value && /.+@aol\.com/.test(value)
    ? 'Really? You still use AOL for your email?'
    : undefined

export const alphaNumeric = value => value && /[^a-zA-Z0-9 -]/i.test(value)
    ? 'Only alphanumeric characters'
    : undefined

export const phoneNumber = value => value && !/^(0|[1-9][0-9]{9})$/i.test(value)
    ? 'Invalid phone number, must be 10 digits'
    : undefined

export const fullName = value => value && (value.trim() === '' || !/^[a-zA-Z\s]*$/i.test(value))
        ? 'Invalid full name'
        : undefined

export const passwordsMatch = pass => passConfirm =>
    pass && passConfirm && pass !== passConfirm
        ? 'Passwords does not match'
        : undefined
export const dateOfBirth = value => value && (new Date(value) >= new Date())
    ? 'Date of birth should be less than now'
    : undefined

export const onlyFutureDate = (value) => !value || new Date(value) > new Date()
    ? undefined
    : 'Cannot select past date'

export const dateGreaterThen = (greater, less, lessName) =>
    !greater || !less || new Date(greater) > new Date(less)
        ? undefined
        : `Date must be greater then ${lessName}`

export const minUsersNumber = min => value => value && value.length < min
    ? `Must be ${min} users selected or more`
    : undefined

export const passwordRequirements = (value) => {
    if (!value)
        return undefined;

    if (value.length < 6) {
        return 'Must be at least 6 characters';
    } else if (!/\d/.test(value)) {
        return 'Password should contain digits';
    } else if (!/(?=.*[a-z])/.test(value)) {
        return 'Password should contain a lower case letter';
    } else if (!/(?=.*[A-Z])/.test(value)) {
        return 'Password should contain an upper case letter';
    } else if (!/(?=.*[_\W])/.test(value)) {
        return 'Password should contain a special symbol';
    } else {
        return undefined;
    }
}
