import React from 'react'
import { Modal, Dropdown } from 'semantic-ui-react'

import { ModusButton } from '@components'

export default function SelectFormulaModal({
    // opts
    header = "Select formula to add",
    // data
    open,
    loading,
    formulas,
    formulaId,
    // actions
    onClose,
    onSelect,
    onFormulaChange
}) {

    //FormulaDto expected
    const options = formulas.map((f) => ({
        value: f.id,
        text: f.name
    }));

    return (
        <Modal
            open={open}
            size="small"
            onClose={onClose}
        >
            <Modal.Header>{header}</Modal.Header>

            <Modal.Content>
                <Dropdown
                    placeholder="Select formula"
                    fluid
                    search
                    selection
                    options={options}
                    onChange={onFormulaChange}
                    defaultValue={formulaId}
                />
            </Modal.Content>

            <Modal.Actions className="modal-flex-actions">
                <ModusButton
                    className="button-flex-order1"
                    filled content="Select" loading={loading} onClick={onSelect} />
                <ModusButton
                    className="button-flex-order2"
                    grey type="button" content="Cancel" onClick={onClose} />
            </Modal.Actions>
        </Modal>
    );
}
