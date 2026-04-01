import { CodeProps as MantineCodeProps } from "@mantine/core";
import BaseCode from "./BaseCode/BaseCode";
import ElevatedCode from "./ElevatedCode/ElevatedCode";
import SurfaceCode from "./SurfaceCode/SurfaceCode";

export interface CodeProps extends MantineCodeProps {
  elevation?: number;
}

const Code = ({ elevation = 0, ...props }: CodeProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseCode {...props} />;
    case 1:
      return <SurfaceCode {...props} />;
    case 2:
      return <ElevatedCode {...props} />;
    default:
      return null;
  }
};

export default Code;
