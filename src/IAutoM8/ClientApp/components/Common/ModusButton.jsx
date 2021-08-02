import React from 'react'
import { Button, Popup, Icon } from 'semantic-ui-react'
import cn from 'classnames'

import { WhenPermitted, ModusIcon } from '@components'

export default function ModusButton({
    grey,
    circular,
    filled,
    icon,
    popup,
    content,
    padded,
    wide,
    whenPermitted,
    className,
    ...props
}) {
    const classNames = cn([
        'iauto',
        grey && 'grey',
        filled && 'purple',
        padded && 'padded',
        wide && 'wide',
        className
    ]);

    const renderIcon = () => {
        if (!icon)
            return null;

        return icon.startsWith('iauto')
            ? <ModusIcon name={icon} />
            : <Icon name={icon} size="large" />
    };

    let Element = circular
        ? (
            <Button
                {...props}
                className={classNames}
                circular
                size="mini"
                icon={renderIcon()}
            />
        )
        : (
            <Button {...props} className={classNames}>
                {renderIcon()}
                {content}
            </Button>
        );

    if (popup) {
        Element = (
            <Popup
                position="top center"
                trigger={Element}
                content={popup}
            />
        )
    }

    if (whenPermitted) {
        Element = (
            <WhenPermitted rule={whenPermitted}>
                {Element}
            </WhenPermitted>
        );
    }

    return Element;
}