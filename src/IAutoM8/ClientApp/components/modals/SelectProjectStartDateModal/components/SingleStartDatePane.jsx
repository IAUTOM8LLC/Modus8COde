import React from 'react'

import { DateTimeInput } from '@components'

import { required, onlyFutureDate } from '@utils/validators'

export default function SingleStartDatePane() {
    return (
        <DateTimeInput
            name="projectStartDateTime"
            label="Start date for all tasks"
            validate={[required, onlyFutureDate]}
        />
    );
}
