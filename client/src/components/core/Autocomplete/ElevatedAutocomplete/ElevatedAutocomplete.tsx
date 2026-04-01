import elevatedClasses from "~/styles/Elevated.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import { Autocomplete, AutocompleteProps } from "@mantine/core";

const ElevatedAutocomplete = (props: AutocompleteProps) => {
  return (
    <Autocomplete
      classNames={{
        input: elevatedClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default ElevatedAutocomplete;
