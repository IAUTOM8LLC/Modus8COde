import React from 'react';
// import { Card, Grid } from 'semantic-ui-react';
import { v4 as uuidv4 } from "uuid";

import CardItem from './CardItem';

class PublishedResourcesPane extends React.Component {
    render() {
        const { publishedResources } = this.props;

        return (
            <div className="resource-container">
                <div className="card-container">
                    {publishedResources.length > 0 &&
                        publishedResources.map((task) => (
                            <div className="card-item" key={uuidv4()}>
                                <CardItem
                                    // title={task.title}
                                    formulaName={task.formulaName}
                                    // description={task.description}
                                    completedDate={task.completedDate}
                                    // dateCreated={task.dateCreated}
                                    resources={task.resources}
                                    notes={task.notes}
                                />
                            </div>
                        ))}
                </div>
            </div>
        );
    }
}

export default PublishedResourcesPane;