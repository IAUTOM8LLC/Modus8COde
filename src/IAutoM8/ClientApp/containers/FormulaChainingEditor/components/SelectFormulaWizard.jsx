import React, { Component } from 'react'
import { connect } from 'react-redux'
import { error, success } from 'react-notification-system-redux'

import { addInternalFormulaTask } from '@store/formulaTasks'
import { selectFormulasWithTasks } from '@selectors/formula'
import { loadOwnedFormulas, nextPage } from '@store/formula'

import SelectFormulaModal from '@components/modals/ImportTasksFromFormulaWizard/components/SelectFormulaModal'

const STAGES = {
    SELECT_FORMULA: 'SELECT_FORMULA'
}

const initialState = {
    formulaId: null,
    date: null,
    formulaName: '',
    stage: STAGES.SELECT_FORMULA
};

@connect(
    (state, props) => ({
        loading: state.formulaTasks.loading || state.formula.loading || state.users.loading,
        formulas: selectFormulasWithTasks(state, props),
        users: state.users.users
    }),
    {
        addInternalFormulaTask,
        loadOwnedFormulas,
        nextPage,
        error, success
    }
)
export default class SelectFormulaWizard extends Component {
    state = initialState;

    componentDidMount() {
        this.props.loadOwnedFormulas();
    }

    componentWillUnmount() {
        this.props.nextPage();
    }

    handleClose = () => {
        this.setState({ ...initialState });
        this.props.onClose();
    }

    /**
     * Select Formula Modal
     */

    handleFormulaChange = (e, data) => {
        const formulaId = data.value;
        this.setState({
            formulaId,
            formulaName: this.props.formulas.find(f => f.id === formulaId).name
        });
    }

    /**
     * Assign Skills Modal
     */

    handleSubmit = () => {
        const { id, position, onComplete } = this.props;
        const { formulaId } = this.state;

        return this.props
            .addInternalFormulaTask({
                posX: Math.floor(position[0]),
                posY: Math.floor(position[1]),
                formulaProjectId: id,
                internalFormulaProjectId: formulaId
            })
            .then(() => this.setState({ ...initialState }))
            .catch((er) => this.props.error({
                title: 'Cannot add formula',
                message: er.data.message
            }))
            .finally(() => onComplete());
    }

    render() {
        const {
            open,
            formulas,
            loading
        } = this.props;
        const { formulaId, stage } = this.state;

        if (!open)
            return null;

        switch (stage) {
            case STAGES.SELECT_FORMULA:
                return <SelectFormulaModal
                    open={open}
                    loading={loading}
                    formulas={formulas}
                    formulaId={formulaId}
                    onClose={this.handleClose}
                    onSelect={this.handleSubmit}
                    onFormulaChange={this.handleFormulaChange}
                />

            default:
                return null;
        }
    }
}
