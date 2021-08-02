import React from 'react'
import cn from 'classnames'

import splitToLowerCase from '@utils/splitToLowerCase'

import { UserAcronym, ModusIcon } from '@components'


import './styles/Header.less'

const empty = () => { };

export default function TaskHeader({
    title, username, formulaName,
    doing, reviewing,isVendor,
    canDo, canReview,hasVendor,
    proccessingUserName,
    canBeProccessed,
    role,
    onDoVendorCard = empty,
    status = '',
    onClick = empty,
    onDoCard = empty,
    onReviewCard = empty
}) {
    // console.log('isVendor',isVendor);
    // console.log('role header', role);
    let actionIcon = null;
    

    if (doing && status === 'InProgress' || status === 'Completed') {
        const message = status === 'Completed' ? 'has completed' : 'is doing';
        const name = status === 'Completed' ? 'iauto--completed' : 'iauto--doing';
        actionIcon = <ModusIcon
            name={name}
            popup={`${username} ${message}`}
        />
    }
    // if (proccessingUserName !== null && status === 'InProgress' || status === 'Completed') {
    //     console.log('doing', doing);
    //     console.log('status', status);
    //     const message = status === 'Completed' ? 'has completed' : 'is doing';
    //     const name = status === 'Completed' ? 'iauto--completed' : 'iauto--doing';
    //     actionIcon = <ModusIcon
    //         name={name}
    //         popup={`${username} ${message}`}
    //     />
    // }
    
    else if (reviewing) {
        actionIcon = <ModusIcon
            name="iauto--review"
            popup={`${username} is reviewing`}
        />
    } 
    else if (canDo) { 
        actionIcon = <ModusIcon
            name="iauto--do"
            popup="Start the task"
            onClick={onDoCard}
        />
    }
    else if ((isVendor || role === 'CompanyWorker' ) && proccessingUserName === null) {
            actionIcon = <ModusIcon
                name="iauto--do"
                popup="Start the task"
                onClick={onDoVendorCard}
            />
    } 
    else if (canReview) {
        actionIcon = <ModusIcon
            name="iauto--do"
            popup="Review the task"
            onClick={onReviewCard}
        />
    }
    else if (username) {
        actionIcon = <UserAcronym fullname={username} popup={username} />;
    }

    return (
        <div className={cn(['task-header', `task-header--${splitToLowerCase(status)}`,
            formulaName && 'sub'])}>
            {actionIcon}
            <span
                onClick={onClick}
                className={cn(['task-header__title', actionIcon && 'padded'])}
            >
                {title}
            </span>
        </div>
    );
}
