import React from 'react'
import { Form } from 'semantic-ui-react'
import cn from 'classnames'

import { SimpleSegment } from '@components'

import './Inline.less'

export default function Inline({ children, className, ...rest }) {
    return (
        <Form as={SimpleSegment} className={cn(['inline-form-elements', className])} {...rest}>
            <Form.Group inline>
                {
                    React.Children.map(children, (child) =>
                        child && <Form.Field>
                            {child}
                        </Form.Field>
                    )
                }
            </Form.Group>
        </Form>
    );
}