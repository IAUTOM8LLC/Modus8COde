import React, { Component, Fragment } from "react";
import { connect } from "react-redux";
import { Dropdown } from "semantic-ui-react";
import FineUploaderTraditional from "fine-uploader-wrappers";
import FileInput from "react-fine-uploader/file-input";
import Dropzone from "react-fine-uploader/dropzone";
import Thumbnail from "react-fine-uploader/thumbnail";
import autobind from "autobind-decorator";
import { success } from 'react-notification-system-redux'
import axios from 'axios';
import moment from "moment";

import Filesize from "./Filesize.jsx";

import { setResources } from '@store/resource'
import { getAuthHeaders } from "@infrastructure/auth";
import {
    uploadFileComplete,
    uploadFile,
    deleteFile,
    shareFile,
    shareGlobalFile,
} from "@store/resource";

import { selectFileResources } from "@selectors/resource";

import { ModusButton, ModusIcon, Inline } from "@components";

import "react-fine-uploader/gallery/gallery.css";
import "./FilesPane.less";

@connect(
    (state, props) => ({
        ...state.resource,
        fileResources: props.showOnlyPublished
            ? showOnlyPublishedFiles([...selectFileResources(state)])
            : [...selectFileResources(state)],
        userPermissions: state.auth.permissions,
        pendingTask: state.pendingTask.pendingTask
    }),
    {
        uploadFileComplete,
        uploadFile,
        deleteFile,
        shareFile,
        shareGlobalFile,
        setResources,
        success,
    }
)
export default class FilesPane extends Component {
    constructor(props) {
        super(props);
        this.state = {
            deleting: false,
            // status: false,
            // isRun: false,
            //showPublishIcon: true,
            disabled: [],
        };
        this.initUploader(this.props.isBulkModelForm);
    }

    @autobind
    handlePublishResource(res) {
        // const {
        //     fileResources,
        //     pendingTask,
        //     setResources
        // } = this.props;

        const obj = {
            id: res.id,
            isPublished: true
        }
        const projectId = this.props.pendingTask.id
        axios.put(`/api/tasks/${projectId}/publish-resource`,
            obj, getAuthHeaders())
            .then(ress => {
                if (ress) {

                    const jsonArr = [];
                    this.props.fileResources.map((response) => (
                        jsonArr.push({
                            cameFromParent: response.cameFromParent,
                            id: response.id,
                            isGlobalShared: response.isGlobalShared,
                            isPublished: res.id === response.id ? ress.data : response.isPublished,
                            isShared: response.isShared,
                            isSharedFromParent: response.isSharedFromParent,
                            localId: response.localId,
                            mime: response.mime,
                            name: response.name,
                            size: response.size,
                            status: response.status,
                            type: response.type,
                            url: response.url,
                            originType: response.originType,
                            timeStamp: response.timeStamp,
                        })
                    ));
                    this.props.success({ title: 'Task resource is published successfully' });
                    this.setState({
                        disabled: [...this.state.disabled, obj.id]
                    })

                    this.props.setResources(jsonArr);
                }
            });

    }
    // const fileApi ={this.props.isBulkModelForm ? "api/resource/temp-upload-file"
    // :"/api/resource/upload-file"}
    initUploader = (flag) => {
        console.log('flag',flag);
        this.uploader = new FineUploaderTraditional({
            options: {
                deleteFile: {
                    enabled: true,
                    endpoint: "/api/resource/delete-file",
                    customHeaders: {
                        Authorization: getAuthHeaders().headers.Authorization,
                    },
                },
                // if (this.props.isBulkModelForm) {
                    
                // },
                request: {
                    //endpoint: {this.props.isBulkModelForm ? "api/resource/temp-upload-file"
                    // :"/api/resource/upload-file"}, ///api/resource/temp-upload-file"""
                    endpoint: flag ?  "api/resource/temp-upload-file" :"/api/resource/upload-file",
                    customHeaders: {
                        Authorization: getAuthHeaders().headers.Authorization,
                    },
                },
                validation: {
                    allowedExtensions: [
                        "jpg",
                        "jpeg",
                        "png",
                        "gif",
                        "svg",
                        "bmp",
                        "psd",
                        "ai",
                        "zip",
                        "pdf",
                        "doc",
                        "docx",
                        "ppt",
                        "pptx",
                        "pps",
                        "ppsx",
                        "odt",
                        "xls",
                        "xlsx",
                        "txt",
                        "yuv",
                        "wmv",
                        "webm",
                        "vob",
                        "svi",
                        "roq",
                        "rmvb",
                        "rm",
                        "ogv",
                        "ogg",
                        "nsv",
                        "mxf",
                        "MTS",
                        "M2TS",
                        "TS",
                        "mpg",
                        "mpeg",
                        "m2v",
                        "mp2",
                        "mpe",
                        "mpv",
                        "mp4",
                        "m4p",
                        "mov",
                        "qt",
                        "mng",
                        "mkv",
                        "m4v",
                        "gifv",
                        "gif",
                        "flv",
                        "f4v",
                        "f4p",
                        "f4a",
                        "f4b",
                        "flv",
                        "flv",
                        "drc",
                        "avi",
                        "asf",
                        "amv",
                        "3gp",
                        "3g2",
                    ],
                },
            },
            cors: {
                //all requests are expected to be cross-domain requests
                expected: true,
            },
        });
    };

    shouldComponentUpdate(nextProps, nextState) {

        if (nextState.disabled !== this.state.disabled) {
            return true;
        }


        if (this.state.deleting !== nextState.deleting) return true;
        // if(nextProps.status === 'NeedsReview') return true
        if (nextProps.fileResources.length !== this.props.fileResources.length)
            return true;
        if (
            this.props.fileResources.some(
                (value, index) =>
                    nextProps.fileResources[index].id !== value.id ||
                    nextProps.fileResources[index].isGlobalShared !==
                    value.isGlobalShared ||
                    nextProps.fileResources[index].isShared !==
                    value.isShared ||
                    nextProps.fileResources[index].status !== value.status
            )
        )
            return true;
        return false;
    }


    componentWillReceiveProps(nextProps) {
        this.uploader.methods
            .addInitialFiles(nextProps.fileResources
                .filter((res) => res.id !== null &&
                    this.uploader.methods.
                        getFile(res.id) === null
                )
                .map((res) => {
                    return {
                        name: res.name,
                        uuid: res.id,
                        size: res.size,
                        thumbnailUrl: res.url,
                    };
                })
            );
    }

    componentWillMount() {
        this.uploader.on("complete", (id, fileName, responseText) => {
            console.log('completeupload', this.props.uploadFileComplete(id, responseText));
            this.props.uploadFileComplete(id, responseText);
        });
        this.uploader.on("upload", (id) => {
        //console.log('this.props.uploadFile',this.props.uploadFile(id, this.uploader.methods.getFile(id)));
            this.props.uploadFile(id, this.uploader.methods.getFile(id));
        });
        this.uploader.on("delete", (id) => {
            this.setState({ deleting: true });
            this.props.deleteFile(id);
        });
        this.uploader.on("deleteComplete", () => {
            this.setState({ deleting: false });
        });
    }

    deleteFile = (id) => {
        this.uploader.methods.deleteFile(id);
    };

    uploadIcon = (className) => {
        const longD =
            "M19.35 10.04C18.67 6.59 15.64 4 12 4 9.11 4 6.6 5.64 5.35 8.04 " +
            "2.34 8.36 0 10.91 0 14c0 3.31 2.69 6 6 6h13c2.76 0 5-2.24 5-5 " +
            "0-2.64-2.05-4.78-4.65-4.96zM14 13v4h-4v-4H7l5-5 5 5h-3z";
        return (
            <svg
                fill="#c3922e"
                height="24"
                viewBox="0 0 24 24"
                width="24"
                className={className}
            >
                <path d="M0 0h24v24H0z" fill="none" />
                <path d={longD} />
            </svg>
        );
    };

    uploadSuccessIcon = () => {
        const longD =
            "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52" +
            " 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z";
        return (
            <svg
                height="24"
                fill="#000000"
                viewBox="0 0 24 24"
                width="24"
                className="react-fine-uploader-gallery-upload-success-icon"
            >
                <path d="M0 0h24v24H0z" fill="none" />
                <path d={longD} />
            </svg>
        );
    };

    deleteIcon = () => {
        const longD =
            "M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2" +
            " 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7" +
            " 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z";
        return (
            <svg height="24" fill="#000000" viewBox="0 0 24 24" width="24">
                <path d={longD} />
                <path d="M0 0h24v24H0z" fill="none" />
            </svg>
        );
    };

    getFileExtension(fileName) {
        return fileName.substring(
            fileName.lastIndexOf(".") + 1,
            fileName.length
        );
    }

    renderFiles(isFormula, resources) {
        const {
            isTaskModal,
            userPermissions,
            resourceList,
            isVendor,
            isFormulaTask,
        } = this.props;

        const { deleting } = this.state;

        const havePermissions = (res) => {
            return (
                res.isSharedFromParent ||
                (!userPermissions.editProjectTask && res.id !== null)
            );
        };
        const shareActions = (res) => {
            if (havePermissions(res))
                return (
                    <ModusButton
                        circular
                        type="button"
                        icon="iauto--share"
                        className="unactive"
                        popup="Can't share shared resource"
                    />
                );

            const options = [
                {
                    key: 1,
                    text: res.isShared
                        ? "Unshare with children"
                        : "Share with children",
                    icon: <ModusIcon name="iauto--share-children" />,
                    onClick: () => this.props.shareFile(res.localId),
                },
                {
                    key: 2,
                    text: res.isGlobalShared
                        ? "Unshare globally"
                        : "Share globally",
                    icon: <ModusIcon name="iauto--share-globally" />,
                    onClick: () => this.props.shareGlobalFile(res.localId),
                },
            ];

            const trigger = (
                <ModusButton
                    circular
                    type="button"
                    icon="iauto--share"
                    className="share"
                />
            );

            return (
                <Dropdown
                    item
                    simple
                    icon={null}
                    inline
                    className="share-actions"
                    trigger={trigger}
                    options={options}
                />
            );
        };

        return (
            <Fragment>
                <ul className={"react-fine-uploader-gallery-files ABC setMinHeight"}>
                    {resources && resources.length && resources.map((res) => {

                        let createdDate = res.timeStamp;
                        if (createdDate != undefined && createdDate != null) {
                            const arr = res.timeStamp.split(".");

                            if (arr.length == 2) {
                                createdDate = arr[0] + "Z";
                            }
                        }


                        return (

                            <li key={res.localId}>
                                <div
                                    style={{
                                        display: 'flex',
                                        justifyContent: 'space-between'
                                    }}
                                    className="files-pane__file-item"
                                >
                                    <div style={{ display: 'flex' }}>
                                        <div className="files-pane__file-item-thumbnail">
                                            <Thumbnail
                                                id={res.localId}
                                                uploader={this.uploader}
                                                fromServer={res.id !== null}
                                                maxSize={50}
                                                notAvailablePlaceholder={
                                                    <span>
                                                        {this.getFileExtension(res.name)}
                                                    </span>
                                                }
                                                className="react-fine-uploader-gallery-thumbnail"
                                            />
                                        </div>

                                        <div className="files-pane__file-item-info setResStyle">
                                            <span className="react-fine-uploader-filename file-name">
                                                {res.name}
                                            </span>
                                            <br />
                                            {resourceList && (
                                                <div className="icon-file"></div>
                                            )}
                                            <Filesize size={res.size} />
                                            {resourceList && (
                                                <div className="task-inherit">
                                                    <div className="icon-task-conteiner"></div>
                                                    <span>
                                                        used in tasks:{" "}
                                                        {res.taskNames.length}
                                                    </span>
                                                    <Dropdown
                                                        scrolling
                                                        icon={
                                                            <div className="icon-task-list"></div>
                                                        }
                                                        options={res.taskNames.map(
                                                            (m, i) => {
                                                                return {
                                                                    key: i,
                                                                    text: m,
                                                                };
                                                            }
                                                        )}
                                                    ></Dropdown>
                                                </div>
                                            )}
                                            {res.status !== "Loaded" && <p>{res.status}</p>}

                                            {isTaskModal && !resourceList && (
                                                <div className="share-icons">
                                                    {res.isGlobalShared && (
                                                        <ModusIcon
                                                            name="iauto--share-globally"
                                                            popup="Shared globally"
                                                        />
                                                    )}
                                                    {res.isShared && (
                                                        <ModusIcon
                                                            name="iauto--share-children"
                                                            popup="Shared with children"
                                                        />
                                                    )}
                                                    {res.isSharedFromParent &&
                                                        res.cameFromParent && (
                                                            <ModusIcon
                                                                name="iauto--share-children"
                                                                popup="Shared from parent"
                                                            />
                                                        )}
                                                    {res.isSharedFromParent &&
                                                        !res.cameFromParent && (
                                                            <ModusIcon
                                                                name="iauto--share-globally"
                                                                popup="Shared from globall tasks"
                                                            />
                                                        )}
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                    <div className="files-pane__file-item-info">
                                        {createdDate && moment(moment(createdDate) || moment()).fromNow()}
                                    </div>
                                    <div className="files-pane__file-item-actions">
                                        <Inline>

                                            {
                                                !isFormulaTask &&
                                                isFormula &&
                                                (!res.isPublished
                                                    //&& this.state.showPublishIcon
                                                    ?
                                                    res.id &&
                                                    (
                                                        <ModusButton
                                                            key={res.id}
                                                            type="button"
                                                            circular
                                                            icon="rss"
                                                            onClick={() => this.handlePublishResource(res)}
                                                            disabled={this.state.disabled.includes(res.id)}
                                                            className={
                                                                res.isPublished
                                                                    ? "published publishCss"
                                                                    : "publish publishCss"
                                                            }
                                                            popup="Publish to all project task."
                                                        />
                                                    ) :

                                                    <ModusButton icon="rss"
                                                        circular
                                                        disabled
                                                        className="published publishCss"
                                                        popup="Publish to all project task."
                                                    />
                                                )}
                                            {isTaskModal &&
                                                !resourceList &&
                                                shareActions(res)}
                                            <ModusButton
                                                as="a"
                                                circular
                                                icon="iauto--download"
                                                className="download"
                                                href={res.url}
                                                download={res.name}
                                            />
                                            {!resourceList && !isVendor && (isFormula || isFormulaTask) && (
                                                <ModusButton
                                                    circular
                                                    type="button"
                                                    icon="iauto--remove-white"
                                                    className={
                                                        deleting ||
                                                            (isTaskModal &&
                                                                havePermissions(res))
                                                            ? "unactive"
                                                            : "delete"
                                                    }
                                                    disabled={
                                                        res.isPublished
                                                    }
                                                    onClick={
                                                        deleting ||
                                                            (isTaskModal &&
                                                                havePermissions(res))
                                                            ? null
                                                            : () =>
                                                                this.deleteFile(
                                                                    res.localId
                                                                )
                                                    }
                                                    popup={
                                                        deleting
                                                            ? "Another resource is deleting"
                                                            : isTaskModal &&
                                                                havePermissions(res)
                                                                ? "Can't delete shared resource"
                                                                : "Delete"
                                                    }
                                                />
                                            )}
                                        </Inline>
                                    </div>
                                </div>
                            </li>
                        )
                    }
                    )}
                </ul>
            </Fragment>
        );
    }

    render() {
        console.log('check',this.props.isBulkModelForm);

        //const { isFormulaTask } = this.props;

        const acceptTypes = () => {
            return (
                "." +
                this.uploader.options.validation.allowedExtensions.join(",.")
            );
        };

        let fileResources = [];
        let taskResources = [];
        if (!this.props.isFormulaTask) {
            fileResources = this.props.fileResources.length && this.props.fileResources.filter((file) => {
                if (file['originType'] !== undefined) {
                    return !file.originType
                } else {
                    return 0;
                }
            })
            taskResources = this.props.fileResources.length && this.props.fileResources.filter((file) => {
                if (file['originType'] === undefined) {
                    return true
                } else {
                    return file.originType;
                }
            })
        }

        return (
            //console.log('uploader', this.uploader)
            <div className="files-pane">
                {!this.props.isFormulaTask && fileResources.length > 0 && (
                    this.renderFiles(this.props.isFormulaTask, fileResources)
                )}

                {!this.props.resourceList && !this.props.showOnlyPublished && (
                    <Dropzone
                        multiple
                        uploader={this.uploader}
                        className="react-fine-uploader-gallery-dropzone setPadding"
                        dropActiveClassName="dragging"
                    >
                        <div style={{ display: "flex", justifyContent: 'space-between' }}>
                            <div><p>
                                {this.uploadIcon(
                                    "react-fine-uploader-gallery-dropzone-upload-icon"
                                )}
                            </p>
                                <p>Drag and drop files here</p>
                            </div>
                            <div style={{ margin: 'auto' }}><p className="or">or</p></div>
                            <section style={{ marginTop: 'auto' }}>
                                <FileInput
                                    accept={acceptTypes()}
                                    multiple
                                    uploader={this.uploader}
                                    className="ui button iauto purple wide"
                                >
                                {this.props.isBulkModelForm ? "Select File" : "Select Files"}
                            </FileInput>
                            </section>
                        </div>
                    </Dropzone>
                )}

                {!this.props.isFormulaTask ?
                    !!taskResources.length && this.renderFiles(true, taskResources) :
                    !!this.props.fileResources.length &&
                    this.renderFiles(true, this.props.fileResources)
                }
            </div>
        );
    }
}

const showOnlyPublishedFiles = (files) =>
    files.filter((file) => file.isPublished);
