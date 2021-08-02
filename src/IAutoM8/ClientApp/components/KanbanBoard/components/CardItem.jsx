import React, { Component } from 'react'
import { connect } from 'react-redux'
import { Card, Rating } from 'semantic-ui-react'
import cn from 'classnames'
import moment from 'moment'

import splitToLowerCase from '@utils/splitToLowerCase'

import { TaskHeader, TaskFooter, TaskDescription, TaskActions } from '@components'

@connect(
    (state, props) => ({
        roles: state.auth.user.roles,
        assignedSkill: state.skill.skills.find(t => t.id === props.assignedSkillId) || {}
    })
)
export default class CardItem extends Component {

    render() {
        const {
            id,
            status,
            title,
            dueDate: serverDueDate,
            startDate,
            highlighted,
            description,
            formulaName,
            reviewRating,

            assignedSkill,
            proccessingUserName, canBeProccessed,
            reviewingUserName, canBeReviewed,
            canChangeProcessingUser,

            isOverdue,
            isConditional,
            isInterval,
            isRecurrent,
            completedDate,

            onEditCard,
            onChangeProcessingUser,
            onStopOutsource,
            onReviewCard,
            onDoCard,
            onDoVendorCard,
            onDeleteCard,
            isGrayed,
            isVendor,
            hasVendor,
            hasAssignedVendor,
            hasAcceptedVendor,
            onOutsourceTabOpen,
            isDisabled,
            roles
        } = this.props;
        const role = roles.toString()
        //console.log('role in card', role);
        const classNames = cn([
            'modus',
            splitToLowerCase(status),
            'link fluid drag-item-card',
            `drag-item-card--${splitToLowerCase(status)}`,
            { 'drag-item-card--highlighted': highlighted },
            { 'grayed': isDisabled ? true : isGrayed }
        ]);

        const dueDate = moment(serverDueDate);
        const startDateParsed = status === 'New' ? moment(startDate) : null;
        const openModal = () => onEditCard(id);

        const canDo = !proccessingUserName && canBeProccessed;
        const canReview = !reviewingUserName && canBeReviewed;
        const hasProcessingUser = (proccessingUserName && status === 'InProgress')
            || (reviewingUserName && status === 'NeedsReview');

        return (
            <li className="drag-item" data-block-id={id}>
                <Card className={classNames}>
                    <Card.Content>
                        <Card.Header>
                            <TaskHeader
                                formulaName={formulaName}
                                title={title}
                                status={status}
                                hasVendor={hasVendor}
                                username={reviewingUserName || proccessingUserName || assignedSkill.name}
                                canDo={canDo}
                                proccessingUserName={proccessingUserName}
                                canBeProccessed={canBeProccessed}
                                canReview={canReview}
                                doing={proccessingUserName !== null}
                                reviewing={reviewingUserName !== null}
                                onClick={openModal}
                                onDoCard={() => onDoCard(id)}
                                onDoVendorCard={() =>onDoVendorCard(id)}
                                onReviewCard={() => onReviewCard(id)}
                                isVendor={isVendor}
                                role= {role}
                            />

                            <TaskActions
                                id={id}
                                canDo={canDo}
                                canReview={canReview}
                                hasProcessingUser={hasProcessingUser}
                                onDoCard={onDoCard}
                                onDoVendorCard={() =>onDoVendorCard()}
                                onReviewCard={onReviewCard}
                                onEditCard={openModal}
                                onChangeProcessingUser={onChangeProcessingUser}
                                onStopOutsource={onStopOutsource}
                                hasAcceptedVendor={hasAcceptedVendor &&
                                    (status === 'InProgress' || status === 'New')}
                                onDeleteCard={onDeleteCard}
                            />
                        </Card.Header>
                        {formulaName && <div className="sub-header_container">
                            <i aria-hidden="true" className="icon formulas" />
                            <div className="sub-header">{formulaName}
                            </div>
                        </div>}
                        {reviewRating &&
                            <Rating
                                disabled
                                maxRating={5}
                                rating={reviewRating}
                                className="task-detail__rating"
                            /> }

                        <TaskDescription description={description} />

                        <TaskFooter
                            dueDate={dueDate}
                            startDate={startDateParsed}
                            isOverdue={isOverdue}
                            isConditional={isConditional}
                            isRecurrent={isRecurrent}
                            isInterval={isInterval}
                            hasVendor={hasVendor}
                            hasAssignedVendor={hasAssignedVendor}
                            status={status}
                            completedDate={completedDate}
                            onOutsourceTabOpen={() => onOutsourceTabOpen(id)}
                        />
                    </Card.Content>
                </Card>
            </li>
        );
    }
}
