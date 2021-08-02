import React from "react";

const inputStyle = {
  width: '45%',
  marginRight: '20px',
  height: '35px'
}
export const Input = ({ value, onChange }) => (
  <input type="text" maxLength = "60" value={value} onChange={onChange} style={inputStyle}/>
);
