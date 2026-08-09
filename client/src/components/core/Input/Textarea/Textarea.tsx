import { TextareaProps as MantineTextareaProps } from "@mantine/core";
import BaseTextarea from "../Base/BaseTextarea/BaseTextarea";
import SurfaceTextarea from "../Surface/SurfaceTextarea/SurfaceTextarea";
import ElevatedTextarea from "../Elevated/ElevatedTextarea/ElevatedTextarea";

export interface TextareaProps extends MantineTextareaProps {
  elevation?: number;
}

const Textarea = ({
  elevation = 0,
  ...props
}: TextareaProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseTextarea {...props} />;
    case 1:
      return <SurfaceTextarea {...props} />;
    case 2:
      return <ElevatedTextarea {...props} />;
    default:
      return null;
  }
};

export default Textarea;
