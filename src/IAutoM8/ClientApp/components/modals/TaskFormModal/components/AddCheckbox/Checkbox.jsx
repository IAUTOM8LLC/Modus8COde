import React from "react";

export const Checkbox = ({ onClick, defaultChecked, disabled }) => (
  <input 
    type="checkbox" 
    onClick={onClick} 
    defaultChecked={defaultChecked} 
    checked={defaultChecked}
    disabled={disabled} />
);
