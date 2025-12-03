import { PinInputProps as MantinePinInputProps } from "@mantine/core";
import BasePinInput from "../Base/BasePinInput/BasePinInput";
import SurfacePinInput from "../Surface/SurfacePinInput/SurfacePinInput";
import ElevatedPinInput from "../Elevated/ElevatedPinInput/ElevatedPinInput";

export interface PinInputProps extends MantinePinInputProps {
  elevation?: number;
}

const PinInput = ({
  elevation = 0,
  ...props
}: PinInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BasePinInput {...props} />;
    case 1:
      return <SurfacePinInput {...props} />;
    case 2:
      return <ElevatedPinInput {...props} />;
    default:
      throw new Error("Invalid elevation level for PinInput component");
  }
};

export default PinInput;
