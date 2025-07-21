import React from 'react';

function DropDown(props) {
  const { options, onChange, name, value} = props;

  return (
    <select id={name} onChange={onChange} defaultValue={value ? value : "Select a value"}>
      <option disabled>Select an option</option>
      {options.map((option, index) => (
        <option key={index} value={option}>
          {option}
        </option>
      ))}
    </select>
  );
}

export default DropDown;
