import React from "react";

import { Checkbox } from "./Checkbox";
import { Button } from "./Button";
import "./AddCheckbox.less";

export const List = ({ list, onChangeBox, handleDel, isChecked, canModify, canDelete, allChecked }) => {

    return(
        <ul className="todo_list">
        {list.map(item => (
            <li
                key={item.id}
                style={{/* textDecoration: item.done ? "line-through" : null,*/ margin: '0 0 10px 0' }}
            >
                <div className="todo_items_container ui checkbox">

                        <Checkbox
                            onClick={() => onChangeBox(item)}
                             defaultChecked={allChecked ? true: item[isChecked]}
                             checked={item[isChecked]}
                          //  checked={allChecked ? true: item[isChecked]}
                            disabled={!canModify}
                        />{" "}
                        <label>  {item.name}</label>

                    {canDelete && (
                    <span className="delete_buttton" >
                    <Button className="ui mini circular icon button iauto"
                     onClick={() => handleDel(item)}>
                         <i aria-hidden="true"
                         className="icon iauto iauto--remove undefined"></i>
                         </Button></span>
                    )}

                </div>


            </li>
        ))}
    </ul>
    )

};
