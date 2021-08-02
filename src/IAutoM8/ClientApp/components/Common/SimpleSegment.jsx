import React from 'react'
import { Segment } from 'semantic-ui-react'

export default function SimpleSegment({ style, ...props }) {
    const styles = {
        padding: 0,
        margin: 0,
        ...style
    };

    return <Segment basic style={styles} {...props} />
}