import React from 'react'
import { Form, } from 'semantic-ui-react'
import { Field } from 'redux-form'
import ReactQuill, { Quill } from 'react-quill'

import cn from 'classnames'
import axios from 'axios'

import { getAuthHeaders } from '@infrastructure/auth'
import { required as isRequired } from '@utils/validators'

import 'react-quill/dist/quill.snow.css'
import './RichEditor.less'

/* Override default video module to wrap iframe in div */
const BlockEmbed = Quill.import('blots/block/embed');
class Video extends BlockEmbed {
    static tagName = 'div'
    static blotName = 'video'
    static className = 'ql-video'

    static create(value) {
        const node = super.create(value);
        const iframe = document.createElement('iframe');
        iframe.setAttribute('frameborder', '0');
        iframe.setAttribute('allowfullscreen', true);
        iframe.setAttribute('src', value);
        node.appendChild(iframe);
        return node;
    }

    static value(domNode) {
        const node = domNode.firstChild || domNode;
        return node.getAttribute('src');
    }
}
Quill.register({ 'formats/video': Video });

const modules = {
    toolbar: {
        container: [
            [{ size: [] }],
            [{ align: '' }, { align: 'center' }, { align: 'right' }],
            ['bold', 'italic', 'underline', 'strike'],
            [{ color: [] }, { background: [] }],
            ['blockquote'],
            [{ list: 'bullet' }, { list: 'ordered' }, { indent: '-1' }, { indent: '+1' }],
            ['link', 'image', 'video'],
            ['clean']
        ],
        handlers: {
            'image': function () {
                let fileInput = this.container.querySelector('input.ql-image[type=file]');
                if (fileInput === null) {
                    fileInput = document.createElement('input');
                    fileInput.setAttribute('type', 'file');
                    fileInput.setAttribute('accept',
                        'image/png, image/gif, image/jpeg, image/bmp, image/x-icon');
                    fileInput.classList.add('ql-image');
                    fileInput.addEventListener('change', () => {
                        if (fileInput.files !== null && fileInput.files[0] !== null) {
                            const formData = new FormData();
                            formData.append('file', fileInput.files[0]);
                            axios.post('/api/resource/upload-description-file', formData,
                                getAuthHeaders())
                                .then((data) => {
                                    const index = this.quill
                                        .getSelection()
                                        .index;
                                    this.quill.insertEmbed(index, 'image', data.data);
                                })
                        }
                    });
                    this.container.appendChild(fileInput);
                }
                fileInput.click();
            }
        }
    }
}

const renderQuillEditor = ({
    label,
    placeholder,
    additionalClass,
    withValidationMessage,
    withoutToolbar,
    input: { onChange, onBlur /* onBlur breaks redux-forms */, ...inputProps }, // eslint-disable-line no-unused-vars
    meta: { touched, error },
    ...props
}) => {
    const hasError = touched && error !== undefined;

    const elementProps = {
        placeholder,
        ...props,
        ...inputProps,
    }

    return (
        <Form.Field error={hasError} >
            {
                label &&
                <label>{label}</label>
            }
            <ReactQuill
                {...elementProps}
                onChange={(content) => onChange(content)}
                modules={withoutToolbar ? { toolbar: false } : modules}
            />
            {
                withValidationMessage && hasError &&
                <span className={cn([additionalClass, 'error'])}>
                    {error}
                </span>
            }
        </Form.Field>
    );
}

export default function RichEditor({
    required = false,
    validate = [],
    withValidationMessage = true,
    withoutToolbar = false,
    ...rest
}) {
    const validators = validate;

    if (required)
        validators.push(isRequired);

    return (
        <Field
            {...rest}
            withValidationMessage={withValidationMessage}
            component={renderQuillEditor}
            validate={validators}
            withoutToolbar={withoutToolbar}
        />
    );
}
