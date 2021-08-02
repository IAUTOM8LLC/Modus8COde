import axios from "axios";
import React, { Component } from "react";
import { connect } from "react-redux";
import { reset } from "redux-form";
import { push } from "react-router-redux";
import { success, error } from "react-notification-system-redux";
import sortBy from "lodash/sortBy";
import autobind from "autobind-decorator";
import { Dimmer, Loader, Tab } from "semantic-ui-react";

import { getAuthHeaders } from "@infrastructure/auth";
import {
    loadFormulas,
    addFormula,
    deleteFormula,
    editFormula,
    unlockFormula,
    lockFormula,
    formulaSelector,
    formulaCategoryClear,
    formulaCategoryApply,
    formulaCategoryRemove,
    formulaSearch,
    formulaSort,
    formulaPage,
    formulaToggleSelection,
    loadCategories,
    formulaToggleAllSelection,
    nextPage,
    formulaOwnes,
    changeFormulaStatus,
    addStar,
    removeStar,
    formulaFilter,
    copyFormula,
    clearFormulas
} from "@store/formula";
import { filterFormulasByQuery } from "@selectors/formula";
import { toggleDirection, formulaAccessor } from "@utils/sort";
import { setFilterColumn } from "@store/layout";
import { selectSearchColumn } from "@selectors/layout";
import {
    setResources,
    updateFormulaResource,
    clearResources,
} from "@store/resource";

import {
    Prompt,
    FormulaFormModal,
    WhenPermitted,
    FormulaToProjectWizard,
    FormulaOverviewModal,
} from "@components";

import FormulasFilterHeader from "./components/FormulasFilterHeader";
import FormulasTable from "./components/FormulasTable";
// import FormulasAllTable from "./components/FormulasAllTable";

import "./Formulas.less";

@connect(
    (state) => ({
        ...state.formula,
        formulas: filterFormulasByQuery(state),
        filterBy: selectSearchColumn(state),
        resources: state.resource,
        //filterButtonModel: state.formula.filterButtonModel
    }),
    {
        loadFormulas,
        addFormula,
        deleteFormula,
        editFormula,
        reset,
        pushState: push,
        success,
        error,
        setFilterColumn,
        setResources,
        updateFormulaResource,
        clearResources,
        unlockFormula,
        lockFormula,
        formulaSelector,
        formulaCategoryClear,
        formulaCategoryApply,
        formulaCategoryRemove,
        formulaSearch,
        formulaSort,
        formulaPage,
        formulaToggleSelection,
        loadCategories,
        formulaToggleAllSelection,
        nextPage,
        formulaOwnes,
        changeFormulaStatus,
        addStar,
        removeStar,
        formulaFilter,
        copyFormula,
        clearFormulas
    }
)
export default class Formulas extends Component {
    state = {
        loading: false,
        formulaDetailsModalOpen: false,
        formulaToEdit: {},
        createProjectWizardOpen: false,
        createProjectFromFormulaId: 0,
        createProjectFromFormulaName: "",
        showCopyFormulaButton: false,
        formulaActualName: "",
        isCreateProject: true,
        isFormulaOverview: true,
        disabled : false
    };

    componentWillReceiveProps(nextProps) {
        const {
            searchModel: {
                page,
                // perPage,
                // isMyFormula,
                isCustom,
                filterCategorieIds,
                // filterTime,
                filterSearch,
                sortField,
                sortDirection,
            },
        } = this.props;
        if (
            //isMyFormula !== nextProps.searchModel.isMyFormula ||
            isCustom !== nextProps.searchModel.isCustom ||
            page !== nextProps.searchModel.page ||
            filterSearch !== nextProps.searchModel.filterSearch ||
            sortField !== nextProps.searchModel.sortField ||
            sortDirection !== nextProps.searchModel.sortDirection ||
            filterCategorieIds.length !==
                nextProps.searchModel.filterCategorieIds.length
        )
            this.props.loadFormulas();
    }

    componentDidMount() {
        this.props.loadFormulas(true);
        this.props.loadCategories();
    }

    componentWillUnmount() {
        this.props.clearFormulas();
    }

    handleContextRef = (contextRef) =>
        this.setState({
            ...this.state,
            contextRef,
        });

    toggleModal = (opened) => {
        if (opened) {
            this.setState({
                formulaDetailsModalOpen: opened,
                contextRef: this.state.contextRef,
            });
        } else {
            this.props.reset("formulaFormModal");
            this.setState({
                formulaDetailsModalOpen: opened,
                formulaToEdit: {},
                contextRef: this.state.contextRef,
            });
        }
    };

    handleSubmit = (formula) => {
        if (this.state.disabled) {
            return;
        }
        this.setState({disabled: true});

        if (formula.copyClick) {

            if (
                this.state.formulaActualName.toLowerCase().trim() ===
                formula.name.toLowerCase().trim()
            ) {
                this.props.error({
                    title:
                        "Please change the formula name. It must be different from Vault Formula Name",
                });
                return false;
            }
            // axios
            //     .post(`/api/formula/copyformula`, formula, getAuthHeaders())
            //     .then((response) => {
            //         this.props.loadFormulas(false);
            //         const id = response.data[0].result;
            //         this.toggleModal(false);
            //         this.props.updateFormulaResource(id, this.props.resources);

            //     });

            this.props
                .copyFormula(formula)
                .then((response) => {
                    this.props.loadFormulas(false);
                    this.toggleModal(false);
                    this.props.success({ title: "Formula copied successfully" })
                })
                .catch((error) => {
                    this.props.error({ title: "Error, unable to copy the formula" });
                    this.toggleModal(false);
                });
        }
        else {
            const submitAction = formula.id
                ? this.props.editFormula
                : this.props.addFormula;

            return submitAction(formula).then(({ action }) => {
                const id = action.payload.data.id;                
                this.toggleModal(false);
                this.props.updateFormulaResource(id, this.props.resources);
                if (!formula.id) {
                    this.props.pushState(`/formulas/${id}`);
                }
            });
        }
        

    };

    @autobind
    async handleDeleteFormula(formulaId) {
        const confirmed = await Prompt.confirm(
            `Do you want to delete formula ${
            this.props.formulas.find((x) => x.id === formulaId).name
            }?`,
            "Confirm delete formula",
            "lab"
        );

        if (confirmed) {
            this.props
                .deleteFormula(formulaId)
                .then(() =>
                    this.props.success({
                        title: "Formula was deleted successfully",
                    })
                )
                .catch((exception) => {
                    let message = "Cannot delete formula";
                    if (exception) {
                        message = exception.data.message;
                    }
                    this.props.error({ title: message });
                });
        }
    }

    handleAddFormulaClick = () => {
        this.props.clearResources();
        this.toggleModal(true);
    };

    handlePublishClick = (formulaId) => {
        this.props.changeFormulaStatus(formulaId)
            .catch(() => {
                this.props.error({
                    title: "There is some issue in sync.",
                })
            });
            
            // .then((response) => {
            //     this.props.loadFormulas(false);
            //     if (response.value.status === 200) {
            //         this.props.success({
            //             title: "Formula published successfully",
            //         });
            //     } else {
            //         this.props.error({
            //             title: "There is some issue in sync.",
            //         });
            //     }
            // });
    };

    handleEdit = (formulaId) => {
        axios
            .get(`/api/formula/${formulaId}`, getAuthHeaders())
            .then((response) => {
                this.setState({
                    formulaToEdit: response.data,
                    contextRef: this.state.contextRef,
                    showCopyFormulaButton: false,
                });
                this.props.clearResources();
                this.toggleModal(true);
                this.props.setResources(response.data.resources);
            });
    };

    handleCopy = (formulaId) => {
        axios
            .get(`/api/formula/${formulaId}`, getAuthHeaders())
            .then((response) => {
                this.setState({
                    formulaToEdit: { ...response.data, copyClick: true },
                    contextRef: this.state.contextRef,
                    showCopyFormulaButton: true,
                    formulaActualName: response.data.name
                });



                this.props.clearResources();
                this.toggleModal(true);
                this.props.setResources(response.data.resources);
            });
    };

    handleView = (formulaId) => {
        axios
            .get(`/api/formula/${formulaId}`, getAuthHeaders())
            .then((response) => {
                this.setState({
                    formulaToEdit: {
                        ...response.data,
                        isView: true,
                    },
                    contextRef: this.state.contextRef,
                });
                this.props.clearResources();
                this.toggleModal(true);
                this.props.setResources(response.data.resources);
            });
    };

    handleFormulaDetailsModalClose = () => {
        this.toggleModal(false);
    };

    toggleCreateProjectModal = (opened, formulaId, formulaName) => {
        if (opened) {
            this.setState({
                createProjectWizardOpen: opened,
                createProjectFromFormulaId: formulaId || 0,
                createProjectFromFormulaName: formulaName,
                contextRef: this.state.contextRef,
            });
        } else {
            this.props.reset("setProjectNameAndHierarchiesForm");
            this.props.reset("selectProjectStartDateForm");
            this.setState({
                createProjectWizardOpen: opened,
                createProjectFromFormulaId: 0,
                createProjectFromFormulaName: "",
                contextRef: this.state.contextRef,
            });
        }
    };

    handleCreateProjectClick = (formulaId, formulaName) => {
        this.toggleCreateProjectModal(true, formulaId, formulaName);
    };

    handleSort = (clickedColumn) => () => {
        this.props.formulaSort(clickedColumn);
    };

    handleUnlock = (formulaId) => {
        this.props
            .unlockFormula(formulaId)
            .then(() => {
                this.props.success({
                    title: "Formula was unlocked successfully",
                });
            })
            .catch((exception) => {
                let message = "Cannot unlock formula";
                if (exception) {
                    message = exception.data.message;
                }
                this.props.error({ title: message });
            });
    };

    handleLock = (formulaId) => {
        this.props
            .lockFormula(formulaId)
            .then(() => {
                this.props.success({
                    title: "Formula was locked successfully",
                });
            })
            .catch((exception) => {
                let message = "Cannot lock formula";
                if (exception) {
                    message = exception.data.message;
                }
                this.props.error({ title: message });
            });
    };

    handleFormulaSelector = (e, data) => {
        switch (data.activeIndex) {
            case 0:
                this.props.formulaSelector(false);
                break;
            case 1:
                this.props.formulaSelector(true);
                break;
        }
    };
    handleFormulaToggleSelect = (value) =>
        this.props.formulaToggleSelection(value);
    handleToggleSelection = () => this.props.formulaToggleAllSelection();

    handleCopyFormula = (formulaId) => {
        this.setState({
            ...this.state,
            loading: true,
            message: "Sharing formula",
        });
        axios
            .get(`/api/formula-sharing/${formulaId}`, getAuthHeaders())
            .then(() => {
                this.props.formulaOwnes([formulaId]);
                this.setState({
                    ...this.state,
                    loading: false,
                });
            })
            .catch((response) => {
                this.props.error({
                    ...this.state,
                    title: response.data.message
                        ? response.data.message
                        : response.data,
                });
                this.setState({
                    ...this.state,
                    loading: false,
                });
            });
    };

    handleCopyAllFormulas = () => {
        const { selectedFormulas, formulaOwnes, error } = this.props;
        this.setState({
            ...this.state,
            loading: true,
            message: "Sharing formulas",
        });
        axios
            .post(`/api/formula-sharing`, selectedFormulas, getAuthHeaders())
            .then(({ data }) => {
                axios
                    .all(
                        data.map((formulaId) =>
                            axios
                                .get(
                                    `/api/formula-sharing/${formulaId}`,
                                    getAuthHeaders()
                                )
                                .catch((response) => {
                                    error({
                                        ...this.state,
                                        title: response.data.message
                                            ? response.data.message
                                            : response.data,
                                    });
                                })
                        )
                    )
                    .then(() => {
                        formulaOwnes(selectedFormulas);
                        this.setState({
                            ...this.state,
                            loading: false,
                        });
                    });
            });
    };

    handleAddStar = (formulaId) => {
        this.props
            .addStar(formulaId)
            .catch((err) => {
                let message = "";
                if (err) {
                    message = err.data.message;
                }
                this.props.error({ title: message });
            });
    };

    handleRemoveStar = (formulaId) => {
        this.props
            .removeStar(formulaId)
            .catch((err) => {
                let message = "";
                if (err) {
                    message = err.data.message;
                }
                this.props.error({ title: message });
            });
    };

    handleChangeFilter = (clickedButton) => {
        this.props.formulaFilter(clickedButton);
    };

    renderTab = () => {
        const {
            searchModel: {
                sortField, 
                sortDirection
            },
            formulas,
            nextPage,
            canNextPage,
        } = this.props;

        const formulaTable = (
            <FormulasTable
                formulas={formulas}
                onDelete={this.handleDeleteFormula}
                onEdit={this.handleEdit}
                onCreateProject={this.handleCreateProjectClick}
                // onOpenFormulaOverview={this.handleCreateProjectClick}
                orderColumn={sortField}
                onSort={this.handleSort}
                onCopy={this.handleCopy}
                sortDirection={sortDirection}
                onUnlock={this.handleUnlock}
                onLock={this.handleLock}
                onAddStar={this.handleAddStar}
                onRemoveStar={this.handleRemoveStar}
                nextPage={canNextPage ? nextPage : null}
                loggedInUserRole={
                    this.props.roles.length > 0 ? this.props.roles[0] : ""
                }
                changeFormulaStatus={this.handlePublishClick}
                // isAdmin={isAdmin}
                loading={this.props.loading}
            />
        );

        const customTab = {
            menuItem: { key: "custom", content: "Custom" },
            pane: {
                key: "custom",
                content: <React.Fragment>{formulaTable}</React.Fragment>,
            }
        }

        let panes = [];
        panes = [
            {
                menuItem: { key: "public", content: "Public" },
                pane: {
                    key: "public",
                    content: <React.Fragment>{formulaTable}</React.Fragment>,
                },
            },
            this.props.searchModel.isAdmin !=undefined && !this.props.searchModel.isAdmin ? customTab : ""
            // {
            //     menuItem: { key: "custom", content: "Custom" },
            //     pane: {
            //         key: "custom",
            //         content: <React.Fragment>{ formulaTable }</React.Fragment>,
            //     },
            // },
        ].filter((pane) => pane);

        return (
            <Tab
                panes={panes}
                renderActiveOnly={false}
                menu={{ secondary: true, pointing: true }}
                onTabChange={this.handleFormulaSelector}
            />
        );
    };

    render() {
        const {
            // setFilterColumn,
            // filterBy,
            searchModel: {
                // page,
                // perPage,
                totalCount,
                // isMyFormula,
                filterCategorieIds,
                filterSearch,
                // sortField,
                // sortDirection,
            },
            filterButtonModel,
            // selectedFormulas,
            // formulas,
            formulaCategoryClear,
            formulaCategoryApply,
            formulaCategoryRemove,
            formulaSearch,
            // formulaPage,
            categories,
            // nextPage,
            // canNextPage,
            // changeFormulaStatus,
        } = this.props;
        const {
            createProjectWizardOpen,
            createProjectFromFormulaId,
            createProjectFromFormulaName,
            message,
            loading,
            contextRef,
            showCopyFormulaButton
        } = this.state;

        return (
            <div className="iauto-formulas" ref={this.handleContextRef}>
                <Dimmer page active={loading}>
                    <Loader>{message}</Loader>
                </Dimmer>

                <FormulasFilterHeader
                    onAddFormulaClick={this.handleAddFormulaClick}
                    // perPage={perPage}
                    totalCount={totalCount}
                    // isMyFormula={isMyFormula}
                    // onChangeFormulaSelector={this.handleFormulaSelector}
                    formulaCategoryClear={formulaCategoryClear}
                    formulaCategoryApply={formulaCategoryApply}
                    formulaCategoryRemove={formulaCategoryRemove}
                    filterCategorieIds={filterCategorieIds}
                    categories={categories}
                    formulaSearch={formulaSearch}
                    filterSearch={filterSearch}
                    onDownloadProjects={this.handleCopyAllFormulas}
                    onChangeFilter={this.handleChangeFilter}
                    contextRef={contextRef}
                    loggedInUserRole={
                        this.props.roles.length > 0 ? this.props.roles[0] : ""
                    }
                    filterButtonModel={filterButtonModel}
                />

                <div className="formulas-pane tab-modal">
                    {this.renderTab()}
                </div>

                {/* {isMyFormula ? (
                    <FormulasTable
                        formulas={formulas}
                        onDelete={this.handleDeleteFormula}
                        onEdit={this.handleEdit}
                        onCopy={this.handleCopy}
                        onCreateProject={this.handleCreateProjectClick}
                        orderColumn={sortField}
                        onSort={this.handleSort}
                        sortDirection={sortDirection}
                        onUnlock={this.handleUnlock}
                        onLock={this.handleLock}
                        onAddStar={this.handleAddStar}
                        onRemoveStar={this.handleRemoveStar}
                        nextPage={canNextPage ? nextPage : null}
                        loggedInUserRole={
                            this.props.roles.length > 0
                                ? this.props.roles[0]
                                : ""
                        }
                        changeFormulaStatus={this.handlePublishClick}
                    />
                ) : (
                    <FormulasAllTable
                        formulas={formulas}
                        onView={this.handleView}
                        onDownloadProject={this.handleCopyFormula}
                        orderColumn={sortField}
                        onSort={this.handleSort}
                        sortDirection={sortDirection}
                        onChange={this.handleFormulaToggleSelect}
                        selectedFormulas={selectedFormulas}
                        onToggleSelection={this.handleToggleSelection}
                        onAddStar={this.handleAddStar}
                        onRemoveStar={this.handleRemoveStar}
                        nextPage={canNextPage ? nextPage : null}                        
                    />
                )} */}

                <FormulaFormModal
                    disabled = {this.state.disabled}
                    initialValues={this.state.formulaToEdit}
                    categories={this.props.categories}
                    open={this.state.formulaDetailsModalOpen}
                    onClose={this.handleFormulaDetailsModalClose}
                    onSubmit={this.handleSubmit}
                    loggedInUserRole={
                        this.props.roles.length > 0 ? this.props.roles[0] : ""
                    }
                    showCopyFormula={showCopyFormulaButton}
                    formulaActualName={this.state.formulaActualName}

                />
                <FormulaToProjectWizard
                    open={createProjectWizardOpen}
                    formulaId={createProjectFromFormulaId}
                    formulaName={createProjectFromFormulaName}
                    onClose={() => this.toggleCreateProjectModal(false)}
                />

                {/* {this.state.isFormulaOverview && (
                    <FormulaOverviewModal
                        open={createProjectWizardOpen}
                        onClose={() => this.toggleCreateProjectModal(false)}
                        formulaId={createProjectFromFormulaId}
                        formulaName={createProjectFromFormulaName}
                        modalHeader="Formula Overview"
                        // isCreateProject={this.state.isCreateProject}
                    />
                )}
                {this.state.isCreateProject && (
                    <WhenPermitted rule="createProjectFromFormula">
                        <FormulaToProjectWizard
                            open={createProjectWizardOpen}
                            formulaId={createProjectFromFormulaId}
                            formulaName={createProjectFromFormulaName}
                            onClose={() => this.toggleCreateProjectModal(false)}
                        />
                    </WhenPermitted>
                )} */}
            </div>
        );
    }
}
