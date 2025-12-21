import React from "react";
import { AssetSelectInputBaseProps } from "./AssetSelectInputBase/AssetSelectInputBase";
import BaseAssetSelectInput from "./BaseAssetSelectInput/BaseAssetSelectInput";
import ElevatedAssetSelectInput from "./ElevatedAssetSelectInput/ElevatedAssetSelectInput";
import SurfaceAssetSelectInput from "./SurfaceAssetSelectInput/SurfaceAssetSelectInput";

export interface AssetSelectProps extends AssetSelectInputBaseProps {
  elevation?: number;
}

const AssetSelect = ({
  elevation = 0,
  ...props
}: AssetSelectProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseAssetSelectInput {...props} />;
    case 1:
      return <SurfaceAssetSelectInput {...props} />;
    case 2:
      return <ElevatedAssetSelectInput {...props} />;
    default:
      return null;
  }
};

export default AssetSelect;
