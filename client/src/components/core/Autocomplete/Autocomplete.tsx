import { AutocompleteProps as MantineAutocompleteProps } from "@mantine/core";
import React from "react";
import BaseAutocomplete from "./BaseAutocomplete/BaseAutocomplete";
import SurfaceAutocomplete from "./SurfaceAutocomplete/SurfaceAutocomplete";
import ElevatedAutocomplete from "./ElevatedAutocomplete/ElevatedAutocomplete";

export interface AutocompleteProps extends MantineAutocompleteProps {
  elevation?: number;
}

const Autocomplete = ({
  elevation,
  ...props
}: AutocompleteProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseAutocomplete {...props} />;
    case 1:
      return <SurfaceAutocomplete {...props} />;
    case 2:
      return <ElevatedAutocomplete {...props} />;
    default:
      return null;
  }
};

export default Autocomplete;
