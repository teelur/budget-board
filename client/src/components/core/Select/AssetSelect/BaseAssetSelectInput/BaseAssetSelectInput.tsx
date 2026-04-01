import baseClasses from "~/styles/Base.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AssetSelectInputBase, {
  AssetSelectInputBaseProps,
} from "../AssetSelectInputBase/AssetSelectInputBase";

const BaseAssetSelectInput = ({
  ...props
}: AssetSelectInputBaseProps): React.ReactNode => {
  return (
    <AssetSelectInputBase
      classNames={{
        input: baseClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default BaseAssetSelectInput;
