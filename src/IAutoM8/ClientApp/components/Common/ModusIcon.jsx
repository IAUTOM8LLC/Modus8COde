import React from 'react';
import { Icon, Popup } from 'semantic-ui-react'

export default function ModusIcon({ name, popup, className, ...props }) {
    const Element = <Icon
        {...props}
        className={`iauto ${name} ${className}`}
    />

    if (popup) {
        return <Popup
            position="top center"
            trigger={Element}
            content={popup}
        />

    }

    return Element;
}