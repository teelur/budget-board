import baseClasses from "~/styles/Base.module.css";

import { FileInput, FileInputProps } from "@mantine/core";

export interface BaseFileInputProps extends FileInputProps {}

const BaseFileInput = ({ ...props }: BaseFileInputProps): React.ReactNode => {
  return <FileInput classNames={{ input: baseClasses.input }} {...props} />;
};

export default BaseFileInput;
