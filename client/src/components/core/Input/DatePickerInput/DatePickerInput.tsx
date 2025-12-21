import BaseDatePickerInput from "../Base/BaseDatePickerInput/BaseDatePickerInput";
import { DatePickerInputProps as MantineDatePickerInputProps } from "@mantine/dates";
import SurfaceDatePickerInput from "../Surface/SurfaceDatePickerInput/SurfaceDatePickerInput";
import ElevatedDatePickerInput from "../Elevated/ElevatedDatePickerInput/ElevatedDatePickerInput";

export interface DatePickerInputProps
  extends MantineDatePickerInputProps<"range"> {
  elevation?: number;
}

const DatePickerInput = ({
  elevation = 0,
  ...props
}: DatePickerInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseDatePickerInput {...props} />;
    case 1:
      return <SurfaceDatePickerInput {...props} />;
    case 2:
      return <ElevatedDatePickerInput {...props} />;
    default:
      return null;
  }
};

export default DatePickerInput;
