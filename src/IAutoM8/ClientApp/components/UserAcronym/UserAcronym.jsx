import React from 'react'

import { Popup } from 'semantic-ui-react'

import './UserAcronym.less'

export default function UserAcronym({ fullname = '', popup }) {

    const acronym = (fullname
        .match(/\b(\w)/g) || []) // Awesome Name -> ['A', 'N']
        .slice(0, 2) // Support only 2 word names...ha ha
        .map(m => m.toUpperCase())
        .join('');

    const Element = <span className="user-acronym" data-acronym={acronym} />;

    if (popup) {
        return (
            <Popup
                content={popup}
                trigger={Element}
                position="top center"
            />
        );
    }

    return Element;
}