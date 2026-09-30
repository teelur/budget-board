import BaseMonthPickerInput from "../Base/BaseMonthPickerInput/BaseMonthPickerInput";
import { MonthPickerInputProps as MantineMonthPickerInputProps } from "@mantine/dates";
import SurfaceMonthPickerInput from "../Surface/SurfaceMonthPickerInput/SurfaceMonthPickerInput";
import ElevatedMonthPickerInput from "../Elevated/ElevatedMonthPickerInput/ElevatedMonthPickerInput";

export interface MonthPickerInputProps extends MantineMonthPickerInputProps {
  elevation?: number;
}

const MonthPickerInput = ({
  elevation = 0,
  ...props
}: MonthPickerInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseMonthPickerInput {...props} />;
    case 1:
      return <SurfaceMonthPickerInput {...props} />;
    case 2:
      return <ElevatedMonthPickerInput {...props} />;
    default:
      return null;
  }
};

export default MonthPickerInput;
