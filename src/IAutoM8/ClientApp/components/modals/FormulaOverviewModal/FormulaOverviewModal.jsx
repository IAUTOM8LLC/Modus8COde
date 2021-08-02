import React from "react";
import { connect } from "react-redux";
import { reduxForm } from "redux-form";
import { Form, Header, Modal } from "semantic-ui-react";

import { loadFormulaById } from "@store/formula";
import { setResources, clearResources } from "@store/resource";

import { ModusButton, RichEditor } from "@components";

import "../SetProjectNameAndHierarchiesModal/SetProjectNameAndHierarchiesModal.less";

class FormulaOverviewModal extends React.Component {
    constructor(props) {
        super(props);
    }

    componentDidMount() {
        // if (
        //     this.props.formulaId !== prevProps.formulaId &&
        //     prevProps.formulaId > 0
        // )
            this.props.loadFormulaById(this.props.formulaId).then(() => {
                this.props.clearResources();
                this.props.setResources(this.props.selectedFormula.resources);
                this.props.initialize(this.props.selectedFormula);
            });
        }

    render() {
        const {
            open,
            onNext,
            onClose,
            loading,
            modalHeader,
            formulaName,
        } = this.props;
        return (
            <Modal
                as="form"
                open={open}
                className="task-details-modal"
                onClose={onClose}
            >
                <Modal.Header>{modalHeader}</Modal.Header>

                <Modal.Content>
                    <Header as="h4">
                        <div className="create-project">
                            <i aria-hidden="true" className="icon formulas" />
                            {formulaName}
                        </div>
                    </Header>
                    <Form as="section">
                        <RichEditor
                            readOnly={true}
                            withoutToolbar
                            name="description"
                        />
                    </Form>
                </Modal.Content>

                <Modal.Actions className="modal-flex-actions">
                    <ModusButton
                        className="button-flex-order1"
                        filled
                        content="Run Formula"
                        type="button"
                        loading={loading}
                        onClick={onNext}
                    />
                    <ModusButton
                        className="button-flex-order2"
                        grey
                        type="button"
                        content="Cancel"
                        onClick={onClose}
                    />
                </Modal.Actions>
            </Modal>
        );
    }
}

export default connect(
    (state) => ({
        selectedFormula: state.formula.selectedFormula,
        loading: state.formula.loading,
        resources: state.resource,
    }),
    {
        loadFormulaById,
        setResources,
        clearResources,
    }
)(
    reduxForm({
        form: "viewFormulaOverviewForm",
        enableReinitialize: true,
    })(FormulaOverviewModal)
);
