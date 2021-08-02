import React from 'react';
import { Segment, Button } from 'semantic-ui-react';

import {
    ModusButton, SimpleSegment
} from '@components';

const StatusFilterButtonGroup = ({ filterButtonModel, onChangeFilter }) => {
    return (
        <Segment clearing basic style={{ margin: "0px !important" }}>
            <SimpleSegment clearing floated="left">
                <Button.Group>
                    <ModusButton
                        filled={filterButtonModel.isAllEnabled}
                        grey={!filterButtonModel.isAllEnabled}
                        content="ALL"
                        onClick={() => onChangeFilter("ALL")}
                    />
                    <ModusButton
                        filled={filterButtonModel.isUpcomingEnabled}
                        grey={!filterButtonModel.isUpcomingEnabled}
                        content="UPCOMING"
                        onClick={() => onChangeFilter("UPCOMING")}
                    />
                    <ModusButton
                        filled={filterButtonModel.isOverdueEnabled}
                        grey={!filterButtonModel.isOverdueEnabled}
                        content="OVERDUE"
                        onClick={() => onChangeFilter("OVERDUE")}
                    />
                    {/* <ModusButton
                        filled={filterButtonModel.isAtRiskEnabled}
                        grey={!filterButtonModel.isAtRiskEnabled}
                        content="AT RISK"
                        onClick={() => onChangeFilter("ATRISK")}
                    /> */}
                    <ModusButton
                        filled={filterButtonModel.isTodoEnabled}
                        grey={!filterButtonModel.isTodoEnabled}
                        content="TO DO"
                        onClick={() => onChangeFilter("TODO")}
                    />
                    <ModusButton
                        filled={filterButtonModel.isReviewEnabled}
                        grey={!filterButtonModel.isReviewEnabled}
                        content="REVIEW"
                        onClick={() => onChangeFilter("REVIEW")}
                    />
                    <ModusButton
                        filled={filterButtonModel.isCompletedEnabled}
                        grey={!filterButtonModel.isCompletedEnabled}
                        content="COMPLETED"
                        onClick={() => onChangeFilter("COMPLETED")}
                    />
                </Button.Group>
            </SimpleSegment>
        </Segment>
    );
};

export default StatusFilterButtonGroup;