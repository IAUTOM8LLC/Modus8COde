import React from 'react';
import { Card, Divider, List, Icon } from 'semantic-ui-react';
import { v4 as uuidv4 } from "uuid";

import { formatURL } from "@utils/formatURL";

import {
    // ModusButton,
    TaskDueDate,
    // TaskHeader,
} from '@components';

const CardItem = (props) => {
    const renderFiles = () => {
        const { resources, notes } = props;
        return (
            // <Segment className="resource-files-section">
            <div>
                {/* <h4>Resources</h4> */}
                <List bulleted>
                    {resources && resources.length > 0 && resources.map(resource => {
                        return (
                            <List.Item key={uuidv4()}>
                                <List.Content floated="right" className="download-icon">
                                    <a href={resource.url}>
                                    <Icon name="cloud download" size="large" />
                                    </a>
                                </List.Content>
                                {resource.title}
                            </List.Item>
                        )
                    })}
                    {notes && notes.length > 0 && notes.map(note => {
                        const noteText = formatURL(note.title);
                        return (
                            <List.Item key={uuidv4()}>
                                <span dangerouslySetInnerHTML={{
                                        __html: noteText,
                                    }}></span>
                            </List.Item>
                        );
                    })}
                </List>
            {/* </Segment> */}
            </div>
        );
    };

    // const renderNotes = () => {
    //     const { notes } = props;

    //     if (notes && notes.length) {
    //         return (
    //             <Segment className="resource-notes-section">
    //                 <h4>Task Notes</h4>
    //                 <List>
    //                     {notes.map(note => {
    //                         const noteText = formatURL(note.text);
    //                         return (
    //                             <List.Item key={note.id}>
    //                                 <List.Content>
    //                                     <List.Header 
    //                                         as="span"
    //                                         dangerouslySetInnerHTML={{
    //                                             __html: noteText,
    //                                         }}>
    //                                     </List.Header>
    //                                     <List.Description>
    //                                         <TaskDueDate date={note.dateCreated} />
    //                                     </List.Description>
    //                                 </List.Content>
    //                             </List.Item>
    //                         );
    //                     })}
    //                 </List>
    //             </Segment>
    //         );
    //     } 
    //     else 
    //         return null;
    // };

    return (
        <Card 
            //className="modus completed resource-item-card resource-item-card--completed"
            className="resource-item-card--completed"
        >
            <Card.Content>
                <Card.Header>
                    {props.formulaName && <div className="sub-header_container">
                                {/* <i aria-hidden="true" className="icon formulas" /> */}
                                <div className="sub-header">{props.formulaName}
                                </div>
                            </div>}
                </Card.Header>
                <Card.Meta>
                    {/* <div className="completed">
                        <span>Created On: </span> <TaskDueDate date={props.dateCreated} />
                    </div>
                    {props.completedDate && 
                        <div className="completed">
                            <span>Completed On: </span> <TaskDueDate date={props.completedDate} />
                        </div>
                    } */}
                    {props.completedDate && 
                        <div className="completed">
                            <span>Completed On: </span> <TaskDueDate date={props.completedDate} />
                        </div>
                    }
                </Card.Meta>
                <Divider />
                <Card.Description>
                    {/* <TaskDescription description={props.description} /> */}
                    <div>
                        {renderFiles()}
                        {/* {renderNotes()} */}
                    </div>
                </Card.Description>
            </Card.Content>
        </Card>
    );
};

export default CardItem;