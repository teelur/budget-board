import surfaceClasses from "~/styles/Surface.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AssetSelectInputSurface, {
  AssetSelectInputBaseProps,
} from "../AssetSelectInputBase/AssetSelectInputBase";

const SurfaceAssetSelectInput = ({
  ...props
}: AssetSelectInputBaseProps): React.ReactNode => {
  return (
    <AssetSelectInputSurface
      classNames={{
        input: surfaceClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default SurfaceAssetSelectInput;
