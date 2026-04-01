import { FileInputProps as MantineFileInputProps } from "@mantine/core";
import BaseFileInput from "../Base/BaseFileInput/BaseFileInput";

export interface FileInputProps extends MantineFileInputProps {
  elevation?: number;
}

const FileInput = ({
  elevation = 0,
  ...props
}: FileInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseFileInput {...props} />;
    case 1:
      throw new Error("Surface FileInput not implemented yet");
    case 2:
      throw new Error("Elevated FileInput not implemented yet");
    default:
      return null;
  }
};

export default FileInput;
