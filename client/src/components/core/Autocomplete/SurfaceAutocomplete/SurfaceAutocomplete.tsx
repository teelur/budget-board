import surfaceClasses from "~/styles/Surface.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import { Autocomplete, AutocompleteProps } from "@mantine/core";

const SurfaceAutocomplete = (props: AutocompleteProps) => {
  return (
    <Autocomplete
      classNames={{
        input: surfaceClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default SurfaceAutocomplete;
