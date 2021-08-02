import React from "react";

export const Button = ({ onClick, children, disabledAdd, className }) => (
  <button disabled={disabledAdd} type="button" onClick={onClick} className={className}>
    {children}
  </button>
);
