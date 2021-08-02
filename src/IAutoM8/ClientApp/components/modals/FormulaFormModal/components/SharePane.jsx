import React from 'react'

import { FormulaSharing } from '@components'

export default function SharePane({ formula }) {
    return (
        <div className="formula-details-modal__pane">
            <FormulaSharing formula={formula} />
        </div>
    );
}
