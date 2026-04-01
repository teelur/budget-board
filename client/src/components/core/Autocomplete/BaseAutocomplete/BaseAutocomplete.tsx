import baseClasses from "~/styles/Base.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import { Autocomplete, AutocompleteProps } from "@mantine/core";

const BaseAutocomplete = (props: AutocompleteProps) => {
  return (
    <Autocomplete
      classNames={{
        input: baseClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default BaseAutocomplete;
