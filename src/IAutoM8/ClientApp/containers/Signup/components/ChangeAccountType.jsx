import React from 'react'
import { Grid, Segment, Header } from 'semantic-ui-react'
import { Link } from 'react-router-dom'

export default function ChangeAccountType({ header, text, linkTo, linkLabel}) {
    return (
        <Grid.Column className="signup-alternative">
            <Segment basic className="table-seg" >
                <Segment basic className="table-cell-seg">
                    <Header>{header}</Header>

                    {text}

                    <Segment basic>
                        <Link to={linkTo} className="ui fluid button iauto basic inverted">
                            {linkLabel}</Link>
                    </Segment>
                </Segment>
            </Segment>
        </Grid.Column>
    );
}
