import axios from "axios";
import typeToReducer from "type-to-reducer";
import update from "immutability-helper";

import { getAuthHeaders } from "@infrastructure/auth";
import * as settings from "@components/ChainingEditor/settings";

import qs from 'qs';

// Types
const LOAD_FORMULA_TASKS = "react/formula-tasks/LOAD_FORMULA_TASKS";
const CLEAR_FORMULA_TASKS = "react/formula-tasks/CLEAR_FORMULA_TASKS";
const MERGE_FORMULA_TASKS = "react/formula-tasks/MERGE_FORMULA_TASKS";
const LOAD_INTERNAL_FORMULA_TASKS =
    "react/formula-tasks/LOAD_INTERNAL_FORMULA_TASKS";
const ADD = "react/formula-tasks/ADD";
const EDIT = "react/formula-tasks/EDIT";
const DELETE = "react/formula-tasks/DELETE";
const ENABLE_TASK = "react/formula-tasks/ENABLE_TASK";
const DISABLE_TASK = "react/formula-tasks/DISABLE_TASK";
const ADD_CONNECTION = "react/formula-tasks/ADD_CONNECTION";
const REMOVE_CONNECTION = "react/formula-tasks/REMOVE_CONNECTION";
const ADD_TODO = "react/formula-tasks/ADD_TODO";
const ADD_TODO_TYPE_TWO = "react/formula-tasks/ADD_TODO_TYPE_TWO";
const LOAD_DISABLED_FORMULA_TASKS = "react/formula-tasks/LOAD_DISABLED_FORMULA_TASKS";

const initialState = {
    loading: false,
    tasks: [],
    canEdit: true,
    todo: [],
    notes: [],
    disabledFormulaTasks: [],
};

// Reducer
const reducer = {
    [MERGE_FORMULA_TASKS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state, action) => ({
            ...state,
            loading: false,
            tasks: update(state.tasks, {
                $mergeArrayById: action.payload[0].data,
            }),
        }),
    },
    [LOAD_FORMULA_TASKS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state, action) => {
            return {
                ...state,
                loading: false,
                tasks: action.payload[0].data,
                canEdit: action.payload[1].data,
            };
        },
    },
    [CLEAR_FORMULA_TASKS]: {
        FULFILLED: (state, action) => ({
            ...initialState,
        }),
    },
    [LOAD_INTERNAL_FORMULA_TASKS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state, action) => {
            const [taskResponse, disabledTaskResponse, parentTaskId] = action.payload;
            let minPosX = 0;
            let minPosY = 0;
            if (taskResponse.data.length) {
                minPosX = taskResponse.data[0].posX;
                minPosY = taskResponse.data[0].posY;
                for (const task of taskResponse.data) {
                    minPosX = minPosX < task.posX ? minPosX : task.posX;
                    minPosY = minPosY < task.posY ? minPosY : task.posY;
                }
            }
            const padLeft = settings.DefaultPadding.left;
            const padTop = settings.DefaultPadding.top;
            
            return {
                ...state,
                loading: false,
                disabledFormulaTasks: (disabledTaskResponse.data || []).map(t => {
                    return t.childFormulaId + "" + t.internalChildFormulaTaskId;
                }),
                tasks: update(state.tasks, {
                    $push: taskResponse.data.map((t) => {
                        return {
                            ...t,
                            posX: t.posX - minPosX + padLeft,
                            posY: t.posY - minPosY + padTop,
                            parentTaskId,
                            id: parentTaskId + "" + t.id,
                            idDashed: parentTaskId + "-" + t.id,
                            parentTasks: t.parentTasks.map(
                                (id) => parentTaskId + "" + id
                            ),
                            childTasks: t.childTasks.map(
                                (id) => parentTaskId + "" + id
                            ),
                            condition: t.condition
                                ? {
                                      ...t.condition,
                                      options: t.condition.options.map((op) => {
                                          return {
                                              ...op,
                                              assignedTaskId: op.assignedTaskId
                                                  ? parentTaskId +
                                                    "" +
                                                    op.assignedTaskId
                                                  : null,
                                          };
                                      }),
                                  }
                                : null,
                        };
                    }),
                }),
            };
        },
    },

    [ADD]: {
        FULFILLED: (state, action) => {
            return {
                ...state,
                tasks: update(state.tasks, { $push: [action.payload.data] }),
            };
        },
    },
    [EDIT]: {
        FULFILLED: (state, action) => {
            const { tasks } = state;
            const index = tasks.findIndex(
                (t) => t.id === Number(action.payload.data.id)
            );
            return {
                ...state,
                tasks: update(tasks, {
                    [index]: { $set: action.payload.data },
                }),
            };
        },
    },

    [DELETE]: {
        PENDING: (state, action) => {
            const task = state.tasks.find((t) => t.id === action.payload);

            const toDelete = [task.id];

            const includeNestedFormulaTasks = (_task) => {
                const nestedTasks = state.tasks
                    .filter((t) => t.parentTaskId === _task.id)
                    .map((t) => {
                        if (t.internalFormulaProjectId)
                            includeNestedFormulaTasks(t);
                        return t.id;
                    });

                toDelete.push(...nestedTasks);
            };

            if (task.internalFormulaProjectId)
                // recursive calls !
                includeNestedFormulaTasks(task);

            return {
                ...state,
                loading: true,
                tasks: state.tasks.filter((p) => !toDelete.includes(p.id)),
            };
        },
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state) => ({ ...state, loading: false }),
    },

    [ADD_CONNECTION]: {
        PENDING: (state, action) => {
            let { parentTaskId, childTaskId } = action.payload;
            parentTaskId = Number(parentTaskId);
            childTaskId = Number(childTaskId);

            const { tasks } = state;

            const sourceId = tasks.findIndex((t) => t.id === parentTaskId);
            const targetId = tasks.findIndex((t) => t.id === childTaskId);

            const tryAdd = (id) => (ids) =>
                ids.includes(id) ? ids : [...ids, id];

            const updatedTasks = update(tasks, {
                [sourceId]: { childTasks: { $apply: tryAdd(childTaskId) } },
                [targetId]: { parentTasks: { $apply: tryAdd(parentTaskId) } },
            });

            return {
                ...state,
                tasks: updatedTasks,
            };
        },
    },

    [REMOVE_CONNECTION]: {
        PENDING: (state, action) => {
            let { parentTaskId, childTaskId } = action.payload;
            parentTaskId = Number(parentTaskId);
            childTaskId = Number(childTaskId);

            const { tasks } = state;

            const sourceId = tasks.findIndex((t) => t.id === parentTaskId);
            const targetId = tasks.findIndex((t) => t.id === childTaskId);

            const updatedTasks = update(tasks, {
                [sourceId]: {
                    childTasks: {
                        $apply: (x) => x.filter((i) => i !== childTaskId),
                    },
                },
                [targetId]: {
                    parentTasks: {
                        $apply: (x) => x.filter((i) => i !== parentTaskId),
                    },
                },
            });

            return {
                ...state,
                tasks: updatedTasks,
            };
        },
    },
    [ADD_TODO]: (state, action) => {
        return {
            ...state,
            todo: action.payload,
        };
    },
    [ADD_TODO_TYPE_TWO]: (state, action) => {
        return {
            ...state,
            todoTypeTwo: action.payload,
        };
    },
    [DISABLE_TASK]: {
        FULFILLED: (state, action) => ({ 
            ...state,
            disabledFormulaTasks: update(state.disabledFormulaTasks,
                { $push: [action.payload[1].toString()] })
        }),
        PENDING: (state) => ({ ...state }),
        REJECTED: (state) => ({ ...state }),
    },
    [ENABLE_TASK]: {
        FULFILLED: (state, action) => ({
            ...state,
                disabledFormulaTasks: state.disabledFormulaTasks
                    .filter(t => t !== action.payload[1].toString())
        }),
        PENDING: (state) => ({ ...state }),
        REJECTED: (state) => ({ ...state }),
    },
    [LOAD_DISABLED_FORMULA_TASKS]: {
        PENDING: (state) => ({ ...state, loading: true }),
        REJECTED: (state) => ({ ...state, loading: false }),
        FULFILLED: (state, action) => {
            return {
                ...state,
                loading: false,
                disabledFormulaTasks: action.payload.data.map(t => {
                    return t.childFormulaId + "" + t.internalChildFormulaTaskId;
                })
            };
        }
    }
};

// Actions
const baseUrl = "/api/formulaTask";

export function loadTasks(formulaId, preserveExisting = false) {
    return {
        type: preserveExisting ? MERGE_FORMULA_TASKS : LOAD_FORMULA_TASKS,
        payload: Promise.all([
            axios.get(`${baseUrl}/formula/${formulaId}`, getAuthHeaders()),
            axios.get(
                `${baseUrl}/formula-status/${formulaId}`,
                getAuthHeaders()
            ),
        ]),
    };
}

export function loadInternalTasks(formulaId, childFormulaId, parentTaskId) {
    return {
        type: LOAD_INTERNAL_FORMULA_TASKS,
        payload: Promise.all([
            axios.get(`${baseUrl}/formula/${formulaId}`, getAuthHeaders()),
            axios
                .get(`${baseUrl}/get-disabled-tasks`, {
                    params: {
                        parentFormulaId: formulaId,
                        childFormulaId: childFormulaId,
                        parentFormulaTaskId: parentTaskId,
                    },
                    paramsSerializer: function (params) {
                        return qs.stringify(params, { arrayFormat: 'repeat' })
                    },
                    ...getAuthHeaders(),
                }),
            Promise.resolve(parentTaskId),
        ]),
    };
}

export function loadTasksWithNestedFormulas(formulaId) {
    return (dispatch) => {
        // recursive calls
        const loadNestedTasks = (tasks) => {
            tasks
                .filter((task) => task.internalFormulaProjectId !== null)
                .forEach((task) =>
                    dispatch(
                        loadInternalTasks(
                            task.internalFormulaProjectId,
                            task.id
                        )
                    ).then((response) =>
                        loadNestedTasks(response.value[0].data)
                    )
                );
        };

        return dispatch(loadTasks(formulaId)).then((response) =>
            loadNestedTasks(response.value[0].data)
        );
    };
}

export function getTask(taskId) {
    return {
        type: LOAD_FORMULA_TASKS,
        payload: axios.get(`${baseUrl}/${taskId}`, getAuthHeaders()),
    };
}

export function addTask(task) {
    return {
        type: ADD,
        payload: axios.post(baseUrl, task, getAuthHeaders()),
    };
}

export function addInternalFormulaTask(task) {
    return {
        type: ADD,
        payload: axios.post(
            `${baseUrl}/add-formula-task`,
            task,
            getAuthHeaders()
        ),
    };
}

export function editTask(task) {
    return {
        type: EDIT,
        payload: axios.put(`${baseUrl}`, task, getAuthHeaders()),
    };
}

export function deleteTask(taskId) {
    return {
        type: DELETE,
        payload: {
            data: taskId,
            promise: axios.delete(`${baseUrl}/${taskId}`, getAuthHeaders()),
        },
    };
}

export function addConnection(parentTaskId, childTaskId) {
    return {
        type: ADD_CONNECTION,
        payload: {
            data: { parentTaskId, childTaskId },
            promise: axios.put(
                `${baseUrl}/connect`,
                { parentTaskId, childTaskId },
                getAuthHeaders()
            ),
        },
    };
}

export function removeConnection(parentTaskId, childTaskId) {
    return {
        type: REMOVE_CONNECTION,
        payload: {
            data: { parentTaskId, childTaskId },
            promise: axios.put(
                `${baseUrl}/disconnect`,
                { parentTaskId, childTaskId },
                getAuthHeaders()
            ),
        },
    };
}
export function clearTasks() {
    return {
        type: CLEAR_FORMULA_TASKS,
        payload: {
            promise: Promise.resolve(),
        },
    };
}
export function addTodo(todoList) {
    return {
        type: ADD_TODO,
        payload: todoList,
    };
}
export function addTodoTypeTwo(todoList) {
    return {
        type: ADD_TODO_TYPE_TWO,
        payload: todoList,
    };
}
export function disableTask(value, id) {
    return {
        type: DISABLE_TASK,
        payload: Promise.all([
            axios.post(`${baseUrl}/disable-formula-task`, value, getAuthHeaders()),
            Promise.resolve(id),
        ])
    };
}
export function enableTask(value, id) {
    return {
        type: ENABLE_TASK,
        payload: Promise.all([
            axios.delete(`${baseUrl}/enable-formula-task`, {
                data: value,
                headers: getAuthHeaders().headers
            }),
            Promise.resolve(id),
        ])
    }
}
export function loadDisabledFormulaTasks(parentFormulaId, childFormulaId, parentTaskId) {
    return {
        type: LOAD_DISABLED_FORMULA_TASKS,
        payload: axios
            .get(`${baseUrl}/formula/${parentFormulaId}/${childFormulaId}/${parentTaskId}`
                , getAuthHeaders()),
    };
}

export default typeToReducer(reducer, initialState);
