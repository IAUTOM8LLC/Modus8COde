import React from 'react'
import { Form } from 'semantic-ui-react'

import { 
    TextInput, 
    // DropdownInput, 
    RichEditor 
} from '@components'

// export default function InformationPane({ categories, showCategory }) {    
export default function InformationPane() {
    return (
        <div className="formula-details-modal__pane">
            <Form as="section">
                {/* <TextInput required name="name" label="Notes name" /> */}
                {/* <span>{this.props.projectToEdit.name}</span> */}
                <RichEditor
                    style={{height: "270px"}}
                    name="description"
                    placeholder={"Type the description"}
                />

                {/* 
                    Don't remove the code block for the descrition
                    Client don't need the following right now, as per his feedback
                    Now, the user can type the description and overview into one place
                */}
                {/* <TextInput
                    rows={7}
                    type="textarea"
                    name="description"
                    label="Description"
                />

                <RichEditor
                    label="Formula Overview"
                    name="formulaOverview"
                    placeholder={"Type the overview"}
                /> */}

                {/* As per the client's feedback, he don't need this block right now */}
                {/* {!showCategory && (
                    <DropdownInput
                        fluid
                        multiple
                        name="categoryIds"
                        label="Categories"
                        options={categories.map((u) => ({
                            key: u.id,
                            value: u.id,
                            text: u.name,
                        }))}
                    />
                )} */}
            </Form>
        </div>
    );
}
