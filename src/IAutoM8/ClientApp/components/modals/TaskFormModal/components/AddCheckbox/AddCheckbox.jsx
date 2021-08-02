import React, { Component } from "react";
import { connect } from "react-redux";
import autobind from "autobind-decorator";

import { addTodo, addTodoTypeTwo } from "@store/formulaTasks";
import {
    addCheckedTodos,
    addProjectTaskChecklist,
    addProjectTaskReviewCheckList
} from '@store/projectTasks';

import { List } from "./List";
import { Input } from "./Input";
import { Button } from "./Button";

@connect(
    (state) => {
        return {
            rootTasks: state,
            pendingTask: state.pendingTask.pendingTask,
        };
    },
    {
        addTodo,
        addTodoTypeTwo,
        addCheckedTodos,
        addProjectTaskChecklist,
        addProjectTaskReviewCheckList
    }
)
export default class AddCheckbox extends Component {
    state = {
        todos: [],
        todoText: "",
        isRun: false,
        isChecked: "",
    };
    constructor(props) {
        super(props);
        this.selectAllHandler = this.selectAllHandler.bind(this);
    }

    @autobind
    initializeTodos() {
        const { isFormulaTask, pendingTask, reviewerTodo } = this.props;
        let todoData = [];
        let reviewerData = [];
        const isChecked = isFormulaTask || pendingTask.isNewTask
            ? 'done'
            : pendingTask.status === 'InProgress'
                ? 'todoIsChecked'
                : 'reviewerIsChecked';

        if (
            pendingTask &&
            pendingTask.formulaTaskChecklists &&
            pendingTask.formulaTaskChecklists.length
        ) {
            todoData = pendingTask.formulaTaskChecklists.filter(
                (item) => item.type === 1
            );
            this.props.addTodo(todoData)
            reviewerData = pendingTask.formulaTaskChecklists.filter(
                (item) => item.type === 2
            );
            this.props.addTodoTypeTwo(reviewerData)

            if (reviewerTodo) {
                this.setState({
                    todos: reviewerData,
                    todoText: "",
                    isRun: true,
                    isChecked
                });
            }
            else {
                this.setState({
                    todos: todoData,
                    todoText: "",
                    isRun: true,
                    isChecked
                });
            }
        } else if (
            pendingTask &&
            pendingTask.taskCheckLists &&
            pendingTask.taskCheckLists.length
        ) {
            this.props.addCheckedTodos(pendingTask.taskCheckLists)
            this.setState({
                todos: pendingTask.taskCheckLists,
                isRun: true,
                isChecked
            });
        } else if (pendingTask.isNewTask) {
            this.setState({
                isRun: true,
                isChecked
            })
        }
    }

    componentDidUpdate() {
        if (!this.state.isRun) {
            this.initializeTodos();
        }
    }

    componentDidMount() {
        if (!this.state.isRun) {
            this.initializeTodos();
        }
    }

    onChangeInput = (e) => {
        this.setState({ todoText: e.target.value });
    };

    onSubmitTodo = () => {
        this.props.isTodoChange
        let finalTodosArray = [];
        if (this.props.reviewerTodo) {
            this.setState(
                ({ todos, todoText }) => ({
                    todos: [
                        ...todos,
                        {
                            id: todos.length + 1,
                            name: todoText,
                            done: false,
                            type: this.props.reviewerTodo ? 2 : 1,
                        },
                    ],
                    todoText: "",
                }),
                () => {
                    this.props.addTodoTypeTwo(this.state.todos);
                    this.props.addProjectTaskReviewCheckList(this.state.todos);
                    finalTodosArray = [...this.state.todos];
                }
            );
        } else {
            this.setState(
                ({ todos, todoText }) => ({
                    todos: [
                        ...todos,
                        {
                            id: todos.length + 1,
                            name: todoText,
                            done: false,
                            type: 1,
                        },
                    ],
                    todoText: "",
                }),
                () => {
                    this.props.addTodo(this.state.todos);
                    this.props.addProjectTaskChecklist(this.state.todos);
                    finalTodosArray = [...this.state.todos];
                }
            );
        }
    };
    onChangeBox = (item) => {
        this.setState(({ todos }) => ({
            todos: todos.map((el) =>
                el.id === item.id ? { ...el, [this.state.isChecked]: !el[this.state.isChecked] } : el
            ),
        }), this.addCheckedTodos);
    };

    selectAllHandler = (e) => {
        if (e.target.checked) {
            this.setState(({ todos }) => ({
                todos: todos.map((item) =>
                    !item[this.state.isChecked] ? { ...item, [this.state.isChecked]: true } : item
                ),
            }), this.addCheckedTodos);
        } else {
            this.setState(({ todos }) => ({
                todos: todos.map((item) =>
                    item[this.state.isChecked] ? { ...item, [this.state.isChecked]: false } : item
                ),
            }), this.addCheckedTodos);
        }
    };

    addCheckedTodos = () => {
        this.props.addCheckedTodos(this.state.todos);
    }

    // handleDel = item => {
    //     this.setState(({ todos }) => ({
    //         todos: todos.filter(el => el.id !== item.id)
    //     }));
    // };

    handleDel = (item) => {
        // this.props.isTodoChange(true)
        if (this.props.reviewerTodo) {
            this.setState(
                ({ todos }) => ({
                    todos: todos.filter((el) => el.id !== item.id),
                }),
                () => {
                    this.props.addTodoTypeTwo(this.state.todos);
                }
            );
        } else {
            this.setState(
                ({ todos }) => ({
                    todos: todos.filter((el) => el.id !== item.id),
                }),
                () => {
                    this.props.addTodo(this.state.todos);
                }
            );
        }
    };

    deleteAllTodos = () => {
        this.props.isTodoChange(true)
        this.setState(() => ({ todos: [] }), this.addCheckedTodos);
    }

    urlify = (text) => {
        const urlRegex = /(https?:\/\/[^\s]+)/g;
        let urlText = text.replace(urlRegex, (url) => {
            return `<a href="${url}">${url}</a>`;
        });
        let newText = urlText.split('\n').map(i => {
            return `${i}<br/>`
        }).join("");
        return newText;
    };

    areSomeUnchecked = () => this.state.todos.some(todo => !todo[this.state.isChecked])

    render() {
        const { todos, todoText } = this.state;
        const { isFormulaTask, pendingTask, isDisableChecklists } = this.props;


        let reviewerTraining = "";
        if (pendingTask.reviewerTraining != undefined)
            reviewerTraining = this.urlify(pendingTask.reviewerTraining);


        const canAdd = isFormulaTask || pendingTask.isNewTask;
        let canModify = isFormulaTask || pendingTask.isNewTask
            ? true
            : ['New', 'Completed'].includes(pendingTask.status)
                ? false
                : true;
        if (isDisableChecklists) canModify = false;

        return (
            <div className="todo_list_container">
                {canAdd && (
                    <div style={{ padding: "10px 15px 0" }}>
                        <Input value={todoText} onChange={this.onChangeInput} />
                        <span style={{ marginTop: "5px" }}>
                            <Button
                                className="ui button iauto purple button-flex-order1 addTodoBtn"
                                disabledAdd={todoText.length === 0}
                                onClick={this.onSubmitTodo}
                            >
                                Add
                            </Button>
                        </span>
                    </div>
                )}

                {todos && todos.length > 0 ? (
                    <div style={{ padding: "10px 15px 0" }}>
                        {canAdd && <div className="form-header ui checkbox">
                            <input type="checkbox" onClick={this.selectAllHandler}
                                checked={!this.areSomeUnchecked()} disabled={!canModify} />
                            <label> Select all</label>
                            {canAdd && !this.areSomeUnchecked() &&
                                <Button className="ui button iauto grey button-flex-order2 set-line-height"
                                    onClick={this.deleteAllTodos}>
                                    Delete All</Button>
                            }
                        </div>
                        }

                        {!canAdd && <div className="chkList"> Checklist Items</div>}

                        <List
                            list={todos}
                            onChangeBox={this.onChangeBox}
                            handleDel={this.handleDel}
                            isFromProject={this.props.isFromProject}
                            isChecked={this.state.isChecked}
                            canModify={canModify}
                            canDelete={canAdd}
                            allChecked={pendingTask.status === 'Completed'}
                        />


                        {!isFormulaTask && (pendingTask.status === 'NeedsReview'
                            || pendingTask.status === 'Completed')
                            && reviewerTraining != ""
                            && <div className="chkList">

                                Reviewer Training:
                        <div>

                                    <div className="notes-pane__note-item-info revTraining">
                                        <span
                                            className="task-note-text"
                                            dangerouslySetInnerHTML={{
                                                __html: reviewerTraining,
                                            }}
                                        />
                                    </div>
                                </div>
                            </div>}


                    </div>
                ) : (
                        <p style={{ margin: "10px" }}>No item added</p>
                    )}
            </div>
        );
    }
}
