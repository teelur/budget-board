import elevatedClasses from "~/styles/Elevated.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AssetSelectInputBase, {
  AssetSelectInputBaseProps,
} from "../AssetSelectInputBase/AssetSelectInputBase";

const ElevatedAssetSelectInput = ({
  ...props
}: AssetSelectInputBaseProps): React.ReactNode => {
  return (
    <AssetSelectInputBase
      classNames={{
        input: elevatedClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default ElevatedAssetSelectInput;
