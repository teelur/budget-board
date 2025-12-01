import BaseDateInput from "../Base/BaseDateInput/BaseDateInput";
import ElevatedDateInput from "../Elevated/ElevatedDateInput/ElevatedDateInput";
import SurfaceDateInput from "../Surface/SurfaceDateInput/SurfaceDateInput";
import { DateInputProps as MantineDateInputProps } from "@mantine/dates";

export interface DateInputProps extends MantineDateInputProps {
  elevation?: number;
}

const DateInput = ({
  elevation = 0,
  ...props
}: DateInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseDateInput {...props} />;
    case 1:
      return <SurfaceDateInput {...props} />;
    case 2:
      return <ElevatedDateInput {...props} />;
    default:
      throw new Error("Invalid elevation level for DateInput");
  }
};

export default DateInput;
