import { TextInputProps as MantineTextInputProps } from "@mantine/core";
import BaseTextInput from "../Base/BaseTextInput/BaseTextInput";
import SurfaceTextInput from "../Surface/SurfaceTextInput/SurfaceTextInput";
import ElevatedTextInput from "../Elevated/ElevatedTextInput/ElevatedTextInput";

export interface TextInputProps extends MantineTextInputProps {
  elevation?: number;
}

const TextInput = ({
  elevation = 0,
  ...props
}: TextInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseTextInput {...props} />;
    case 1:
      return <SurfaceTextInput {...props} />;
    case 2:
      return <ElevatedTextInput {...props} />;
    default:
      return <BaseTextInput {...props} />;
  }
};

export default TextInput;
