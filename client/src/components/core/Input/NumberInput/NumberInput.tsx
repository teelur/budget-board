import { NumberInputProps as MantineNumberInputProps } from "@mantine/core";
import BaseNumberInput from "../Base/BaseNumberInput/BaseNumberInput";
import SurfaceNumberInput from "../Surface/SurfaceNumberInput/SurfaceNumberInput";
import ElevatedNumberInput from "../Elevated/ElevatedNumberInput/ElevatedNumberInput";

export interface NumberInputProps extends MantineNumberInputProps {
  elevation?: number;
}

const NumberInput = ({
  elevation = 0,
  ...props
}: NumberInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseNumberInput {...props} />;
    case 1:
      return <SurfaceNumberInput {...props} />;
    case 2:
      return <ElevatedNumberInput {...props} />;
    default:
      return null;
  }
};

export default NumberInput;
